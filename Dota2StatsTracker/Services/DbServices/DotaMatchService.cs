using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2StatsTracker.Services.ApiServices;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Dota2StatsTracker.Services.DbServices
{
    public class DotaMatchService
    {
        private readonly OpenDotaApiService _openDotaApiService;
        private readonly DotaMatchRepository _dotaMatchRepository;

        public DotaMatchService(OpenDotaApiService openDotaApiService, DotaMatchRepository dotaMatchRepository)
        {
            _openDotaApiService = openDotaApiService;
            _dotaMatchRepository = dotaMatchRepository;
        }

        public async Task<IReadOnlyCollection<DotaMatch>> GetMatchesAndAssignToAccountAsync(int accountId, int steam32Id)
        {
            var matches = (await _openDotaApiService.GetAccountMatchesIdsAsync(steam32Id)).Select(m => new DotaMatch { OpenDotaMatchId = m, DotaAccountId = accountId }).ToArray();

            _dotaMatchRepository.Create(matches);

            return matches;
        }
    }
}
