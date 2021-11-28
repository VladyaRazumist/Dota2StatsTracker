using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dota2StatsTracker.Services.ApiServices;
using Dota2StatsTracker.Services.HelperServices;
using Infrastructure;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Dota2StatsTracker.Jobs
{
    public class GetNotificationsJob
    {
        private readonly OpenDotaApiService _openDotaApiService;
        private readonly OpenDotaEmberBuilder _emberBuilder;
        private readonly DiscordSocketClient _client;

        private readonly DotaMatchRepository _dotaMatchRepository;
        private readonly UserRepository _userRepository;
        private readonly DbUnitOfWork _dbUnitOfWork;

        public GetNotificationsJob(OpenDotaApiService openDotaApiService, OpenDotaEmberBuilder emberBuilder, DiscordSocketClient client,
            DotaMatchRepository dotaMatchRepository, UserRepository userRepository, DbUnitOfWork dbUnitOfWork)
        {
            _openDotaApiService = openDotaApiService;
            _emberBuilder = emberBuilder;
            _client = client;

            _dotaMatchRepository = dotaMatchRepository;
            _userRepository = userRepository;
            _dbUnitOfWork = dbUnitOfWork;
        }

        public async Task CheckNewMatches(int steamId, ulong channelId)
        {
            var accountInfo = await _userRepository.FirstAsync(u => u.Account.SteamId == steamId, u => new
            {
                UserName = u.Name,
                u.Account.Id,
                u.Account.SteamId,
                u.Account.Matches,
            });

            var matchIds = await _openDotaApiService.GetAccountMatchesIdsAsync(accountInfo.SteamId);
            var newMatchIds = matchIds.Except(accountInfo.Matches.Select(m => m.OpenDotaMatchId)).ToArray();

            if (!newMatchIds.Any())
                return;

            foreach (var matchId in newMatchIds)
            {
                _dotaMatchRepository.Create(new DotaMatch { DotaAccountId = accountInfo.Id, OpenDotaMatchId = matchId });
            }

            await _dbUnitOfWork.CommitAsync();

            var lastMatchId = newMatchIds[0];
            var matchInfo = await _openDotaApiService.MatchInfoAsync(lastMatchId, accountInfo.SteamId);

            var embed = _emberBuilder.BuildMatchInfoEmbed(matchInfo);
            var channel = (_client.GetChannel(channelId)) as IMessageChannel;

            await channel.SendMessageAsync($"{accountInfo.UserName} только что закончил матч", false, embed);
        }
    }
}
