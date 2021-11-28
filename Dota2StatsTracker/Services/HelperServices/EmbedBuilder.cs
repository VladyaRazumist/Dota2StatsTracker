using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Discord;
using Dota2StatsTracker.Services.ApiServices;

namespace Dota2StatsTracker.Services.HelperServices
{
    public class OpenDotaEmberBuilder
    {
        private const string OpenDotaMatchesUrl = "https://www.opendota.com/matches/";
        private const string OpenDotaPlayersUrl = "https://www.opendota.com/players/";

        public Embed BuildMatchInfoEmbed(MatchInfo matchInfo)
        {
            var chatMessages = string.Empty;

            foreach (var chatMessage in matchInfo.ChatMessages)
            {
                chatMessages += $"{chatMessage.SenderName} {((chatMessage.Time < TimeSpan.Zero) ? "-" : "") + chatMessage.Time.ToString(@"mm\:ss")} : {chatMessage.Message} \n";
            }

            var playerName = matchInfo.PlayerName;

            if (matchInfo.KnownName != null)
                playerName += $" ({matchInfo.KnownName})";

            var result = matchInfo.Win ? "Win ✅" : "Lose ❌";

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(matchInfo.Hero.ImgUrl)
                .WithColor(matchInfo.Hero.PrimaryAttributeColor)
                .AddField("Score", $"{matchInfo.TeamKills}  ⚔️  {matchInfo.EnemyTeamKills}")
                .AddField("KDA", $"🌟  {matchInfo.KDA.Kills}/{matchInfo.KDA.Deaths}/{matchInfo.KDA.Assists} --> **{matchInfo.KDA.KDA.ToString(CultureInfo.InvariantCulture)}**", true)
                .AddField("Result", $"**{result}**", true)
                .AddField("Net Worth", $"{matchInfo.NetWorth} 💰", true)
                .AddField("Hero damage", $"{matchInfo.HeroDamage} 🩸", true)
                .AddField("Lasthits", $"{matchInfo.LastHits} 🦴", true)
                .AddField("Duration", matchInfo.Duration.ToString(@"hh\:mm\:ss") + " ⏲️", true)
                .AddField("Played At (UTC)", matchInfo.StartedAt.ToString("U"), true)
                .AddField("OpenDota", $"{OpenDotaMatchesUrl}{matchInfo.MatchId}", true)
                .AddField("Items", string.Join(", ", matchInfo.Items.Select(i => i.Name)))
                .WithAuthor($"{playerName}", $"{matchInfo.Rank.LocalImageUrl}");

            if (chatMessages.Any())
                builder.AddField("Chat", chatMessages);

            return builder.Build();
        }

        public Embed BuildHeroStatsEmbed(HeroStats heroStats)
        {
            var builder = new EmbedBuilder().WithThumbnailUrl(heroStats.Hero.ImgUrl)
                .WithDescription("Hero stats on Herald")
                .WithTitle(heroStats.Hero.LocalizedName)
                .WithColor(heroStats.Hero.PrimaryAttributeColor)
                .AddField("Picks", heroStats.WinLoseStats.Total, true)
                .AddField("Wins", heroStats.WinLoseStats.Wins, true)
                .AddField("Loses", heroStats.WinLoseStats.Loses, true)
                .AddField("Win ratio", $"{heroStats.WinLoseStats.WinRatio.ToString(CultureInfo.InvariantCulture)}%");

            return builder.Build();
        }

        public Embed BuildHeroInfoEmbed(Hero hero)
        {
            var builder = new EmbedBuilder().WithThumbnailUrl(hero.ImgUrl)
                .WithDescription("Hero characteristics")
                .WithTitle(hero.LocalizedName)
                .WithColor(hero.PrimaryAttributeColor)
                .AddField("AttackType", hero.AttackType, true)
                .AddField("PrimaryAttr", hero.PrimaryAttr, true)
                .AddField("MoveSpeed", hero.MoveSpeed, true)
                .AddField("Roles", string.Join(",", hero.Roles));

            return builder.Build();
        }

        public Embed BuildPlayerProfileEmbed(PlayerProfile profile, WinLoseStats playerStats)
        {
            var playerName = profile.PersonaName;

            if (profile.KnownName != null)
                playerName += $" ({profile.KnownName})";

            var bestHeroesString = string.Empty;

            foreach (var heroStats in profile.HeroesStats)
            {
                bestHeroesString += $"**{heroStats.Hero.LocalizedName}** : WR: **{heroStats.WinLoseStats.WinRatio.ToString(CultureInfo.InvariantCulture)}**%, W/L: **{heroStats.WinLoseStats.Wins}**/**{heroStats.WinLoseStats.Loses}** \n";
            }

            var author = string.Empty;

            if (profile.LeaderBoardRank.HasValue)
                author += $"(Top {profile.LeaderBoardRank})";

            author += $"  {playerName}";

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(profile.AvatarUrl)
                .WithColor(Color.Purple)
                .AddField("Total games", playerStats.Total, true)
                .AddField("WinRatio", $"{playerStats.WinRatio.ToString(CultureInfo.InvariantCulture)}%", true)
                .AddField("~MMR", profile.MmrEstimate, true)
                .AddField("Wins", playerStats.Wins)
                .AddField("Loses", playerStats.Loses, true)
                .AddField("Best heroes", bestHeroesString)
                .WithAuthor($"{author}", profile.Rank.LocalImageUrl)
                .AddField("Profile", $"{OpenDotaPlayersUrl}{profile.AccountId}")
                .WithFooter($"SteamId : {profile.AccountId}");

            if (profile.HasDotaPlus)
                builder.AddField("Dota Plus", "**Yes** 👌");

            return builder.Build();
        }
    }
}
