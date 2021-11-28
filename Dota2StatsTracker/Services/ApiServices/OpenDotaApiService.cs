using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dota2StatsTracker.Config;
using Dota2StatsTracker.Exceptions;
using Dota2StatsTracker.Services.HelperServices;
using Microsoft.Extensions.Options;
using OpenDotaDotNet;
using OpenDotaDotNet.JsonConverters;
using OpenDotaDotNet.Models.Players;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Dota2StatsTracker.Services.ApiServices
{
    public class OpenDotaApiService
    {
        private const string OpenDotaUrl = "https://www.opendota.com/";
        private readonly string _exposeMatchHistoryImageUrl;

        private readonly OpenDotaApi _openDotaApiClient;
        private readonly DotaConstantService _dotaConstantService;

        public OpenDotaApiService(DotaConstantService dotaConstantService, IOptions<Settings> settings)
        {
            _openDotaApiClient = new OpenDotaApi();
            _dotaConstantService = dotaConstantService;

            _exposeMatchHistoryImageUrl = settings.Value.ImagesContentPath + "exposeMatchHistory.jpg";
        }

        public async Task<WinLoseStats> GetWinRatioAsync(int steam32Id, int? laneRole = null, int? heroId = null, int? againstHeroId = null)
        {
            var againstHeroIds = new List<int>();
            if (againstHeroId.HasValue)
                againstHeroIds.Add((int)againstHeroId);

            var stats = await _openDotaApiClient.Players.GetPlayerWinLossByIdAsync(steam32Id, new PlayerEndpointParameters { LaneRole = laneRole, HeroId = heroId, AgainstHeroIds = againstHeroIds });

            return new WinLoseStats { Wins = stats.Wins, Total = stats.Wins + stats.Losses };
        }

        public async Task<IReadOnlyCollection<long>> GetAccountMatchesIdsAsync(int steam32Id)
        {
            await _openDotaApiClient.Players.RefreshPlayerMatchHistoryAsync(steam32Id);

            return (await _openDotaApiClient.Players.GetPlayerMatchesAsync(steam32Id)).OrderByDescending(m => m.StartTime).Select(m => m.MatchId).ToArray();
        }

        public async Task<string> GetPersonaNameBySteamIdAsync(int steam32Id)
        {
            try
            {
                var player = await _openDotaApiClient.Players.GetPlayerByIdAsync(steam32Id);
                return player.Profile.Name;
            }
            catch (NullReferenceException)
            {
                throw new PudgeBotException($"Падж ничего не смог найти 😿. Убедитесь, что история игр открыта. \n Если она была закрыта, зайдите на {OpenDotaUrl} и нажмите Refresh", _exposeMatchHistoryImageUrl);
            }
        }

        public async Task<PlayerProfile> GetPlayerProfileAsync(int steam32Id)
        {
            var player = await _openDotaApiClient.Players.GetPlayerByIdAsync(steam32Id);
            var playerHeroStats = (await _openDotaApiClient.Players.GetPlayerHeroesAsync(steam32Id)).Take(3).ToArray();

            var heroStats = new List<HeroStats>();

            var playerProfile = new PlayerProfile
            {
                AccountId = player.Profile.AccountId,
                AvatarUrl = player.Profile.Avatar.AbsoluteUri,
                AvatarMediumUrl = player.Profile.Avatarmedium.AbsoluteUri,
                AvatarFullUrl = player.Profile.Avatarfull.AbsoluteUri,
                LastLogin = player.Profile.LastLogin,
                MmrEstimate = player.MmrEstimate.Estimate,
                ProfileUrl = player.Profile.Profileurl.AbsoluteUri,
                PersonaName = player.Profile.Personaname,
                KnownName = player.Profile.Name,
                HasDotaPlus = (bool)player.Profile.Plus,
                LeaderBoardRank = player.LeaderboardRank,
            };

            if (player.RankTier.HasValue)
            {
                var tier = await AdjustRankTierAsync((int)player.RankTier, steam32Id, player.LeaderboardRank);
                playerProfile.Rank = new PlayerRank { Tier = tier, LocalImageUrl = _dotaConstantService.GetRankImageLocalUrl(tier) };
            }

            foreach (var playerHeroStat in playerHeroStats)
            {
                heroStats.Add(new HeroStats
                {
                    WinLoseStats = new WinLoseStats { Total = playerHeroStat.Games, Wins = playerHeroStat.Win },
                    Hero = _dotaConstantService.Heroes.First(h => h.Id == playerHeroStat.HeroId),
                });
            }

            playerProfile.HeroesStats = heroStats;

            return playerProfile;
        }

        public async Task<MatchInfo> GetLastMatchAsync(int steam32Id)
        {
            var playerMatches = await _openDotaApiClient.Players.GetPlayerMatchesAsync(steam32Id);
            var latestMatch = playerMatches.OrderByDescending(p => p.StartTime).First();

            return await MatchInfoAsync(latestMatch.MatchId, steam32Id);
        }

        public async Task<MatchInfo> MatchInfoAsync(long matchId, int steam32Id)
        {
            var detailedMatchInfo = await _openDotaApiClient.Matches.GetMatchByIdAsync(matchId);
            var detailedPlayerInfo = detailedMatchInfo.Players.First(p => p.AccountId == steam32Id);

            var matchItems = new[]
            {
                detailedPlayerInfo.Item0, detailedPlayerInfo.Item1, detailedPlayerInfo.Item2, detailedPlayerInfo.Item3,
                detailedPlayerInfo.Item4, detailedPlayerInfo.Item5
            }.ToArray();

            var items = _dotaConstantService.Items.Where(i => matchItems.Contains(i.Id)).ToArray();

            var isRadiant = detailedPlayerInfo.IsRadiant;

            var matchInfo = new MatchInfo
            {
                MatchId = matchId,
                PlayerSlot = detailedPlayerInfo.PlayerSlot,
                Items = items,
                Hero = _dotaConstantService.Heroes.First(h => h.Id == detailedPlayerInfo.HeroId),
                Win = isRadiant && detailedPlayerInfo.RadiantWin || !isRadiant && !detailedPlayerInfo.RadiantWin,
                PlayerName = detailedPlayerInfo.Personaname,
                KnownName = detailedPlayerInfo.Name,
                LastHits = detailedPlayerInfo.LastHits,
                NetWorth = detailedPlayerInfo.TotalGold,
                KDA = new Kda { Kills = (int)detailedPlayerInfo.Kills, Deaths = detailedPlayerInfo.Deaths, Assists = detailedPlayerInfo.Assists },
                Duration = TimeSpan.FromSeconds(Convert.ToDouble(detailedMatchInfo.Duration)),
                StartedAt = DateTimeOffset.FromUnixTimeSeconds(detailedMatchInfo.StartTime).UtcDateTime,
                HeroDamage = detailedPlayerInfo.HeroDamage,
                Level = detailedPlayerInfo.Level,
                IsRadiant = isRadiant,
                TeamKills = isRadiant ? detailedMatchInfo.RadiantScore : detailedMatchInfo.DireScore,
                EnemyTeamKills = isRadiant ? detailedMatchInfo.DireScore : detailedMatchInfo.RadiantScore,
                ChatMessages = detailedMatchInfo.Chat != null ? detailedMatchInfo.Chat.Where(c => c.Type != "chatwheel").Select(c => new ChatMessage
                {
                    Message = c.Key,
                    Time = TimeSpan.FromSeconds(Convert.ToDouble(c.Time)),
                    SenderName = c.Unit,
                }).ToArray() : Array.Empty<ChatMessage>(),
            };

            if (detailedPlayerInfo.RankTier.HasValue)
            {
                var tier = await AdjustRankTierAsync((int)detailedPlayerInfo.RankTier, steam32Id);

                matchInfo.Rank = new PlayerRank { Tier = tier, LocalImageUrl = _dotaConstantService.GetRankImageLocalUrl(tier) };
            }

            return matchInfo;
        }

        public async Task<HeroStats> GetHeroStatsAsync(string name)
        {
            var hero = _dotaConstantService.GetHeroByName(name);
            if (hero == null)
                return null;

            var response = await new HttpClient().GetAsync("https://api.opendota.com/api/heroStats");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new LongConverter());
            options.Converters.Add(new IntConverter());
            options.Converters.Add(new NullableIntConverter());
            options.Converters.Add(new StringConverter());
            options.Converters.Add(new BoolConverter());

            var heroStatsModel = JsonSerializer.Deserialize<List<FixedHeroStatsModel>>(responseBody, options).First(m => m.HeroId == hero.Id);

            return new HeroStats
            {
                Hero = hero,
                WinLoseStats = new WinLoseStats { Wins = heroStatsModel.HeraldWins, Total = heroStatsModel.HeraldPicks }
            };
        }

        private async Task<int> AdjustRankTierAsync(int tier, int steam32Id, int? leaderBoardRank = null)
        {
            if (tier < 80) 
                return tier;

            leaderBoardRank ??= (await GetPlayerProfileAsync(steam32Id)).LeaderBoardRank;

            if (leaderBoardRank.HasValue)
            {
                if (leaderBoardRank.Value <= 10)
                    return 100;

                if (leaderBoardRank.Value <= 100)
                    return 90;
            }

            return tier;
        }
    }

    public class PlayerProfile
    {
        public PlayerRank Rank { get; set; }

        public int? LeaderBoardRank { get; set; }

        public int? MmrEstimate { get; set; }

        public long AccountId { get; set; }

        public string PersonaName { get; set; }

        public string KnownName { get; set; }

        public string AvatarUrl { get; set; }

        public string AvatarMediumUrl { get; set; }

        public string AvatarFullUrl { get; set; }

        public string ProfileUrl { get; set; }

        public bool HasDotaPlus { get; set; }

        public DateTimeOffset? LastLogin { get; set; }

        public IReadOnlyCollection<HeroStats> HeroesStats { get; set; }

    }

    public class HeroStats
    {
        public Hero Hero { get; set; }
        public WinLoseStats WinLoseStats { get; set; }
    }

    public class MatchInfo
    {
        public long MatchId { get; set; }

        public int PlayerSlot { get; set; }

        public bool Win { get; set; }

        public bool IsRadiant { get; set; }

        public Hero Hero { get; set; }

        public PlayerRank Rank { get; set; }

        public TimeSpan Duration { get; set; }

        public IReadOnlyCollection<ChatMessage> ChatMessages { get; set; }

        public IReadOnlyCollection<Item> Items { get; set; }

        public Kda KDA { get; set; }

        public string PlayerName { get; set; }

        public string KnownName { get; set; }

        public long NetWorth { get; set; }

        public int LastHits { get; set; }

        public DateTime StartedAt { get; set; }

        public double HeroDamage { get; set; }

        public int Level { get; set; }

        public int TeamKills { get; set; }

        public int EnemyTeamKills { get; set; }
    }

    public class Kda
    {
        public int Kills { get; set; }

        public int Deaths { get; set; }

        public long Assists { get; set; }

        public double KDA => Math.Round((Kills + (double)Assists) / Deaths, 1);
    }

    public class ChatMessage
    {
        public string Message { get; set; }

        public TimeSpan Time { get; set; }

        public string SenderName { get; set; }
    }

    public class PlayerRank
    {
        public int Tier { get; set; }

        public string LocalImageUrl { get; set; }
    }

    public class WinLoseStats
    {
        public int Total { get; set; }
        public int Wins { get; set; }
        public int Loses => Total - Wins;
        public double WinRatio => Math.Round(Wins / (double)Total * 100, 1);
    }
}
