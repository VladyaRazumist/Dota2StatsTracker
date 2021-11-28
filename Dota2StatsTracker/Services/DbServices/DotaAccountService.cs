using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2StatsTracker.Services.ApiServices;
using Infrastructure;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Dota2StatsTracker.Services.DbServices
{
    public class DotaAccountService
    {
        private readonly UserService _userService;
        private readonly OpenDotaApiService _openDotaApiService;
        private readonly DotaMatchService _dotaMatchService;

        private readonly DotaAccountRepository _dotaAccountRepository;
        private readonly UserRepository _userRepository;
        private readonly DbUnitOfWork _dbUnitOfWork;

        public DotaAccountService(UserService userService, OpenDotaApiService openDotaApiService, DotaMatchService dotaMatchService, DotaAccountRepository dotaAccountRepository, UserRepository userRepository, DbUnitOfWork dbUnitOfWork)
        {
            _userService = userService;
            _openDotaApiService = openDotaApiService;
            _dotaMatchService = dotaMatchService;

            _dotaAccountRepository = dotaAccountRepository;
            _userRepository = userRepository;
            _dbUnitOfWork = dbUnitOfWork;
        }

        public async Task<DotaAccount> CreateAndAssignToUserAsync(int steam32Id, ulong discordUserId)
        {
            var dbUser = await _userService.GetUser(discordUserId);
            var personaName = await _openDotaApiService.GetPersonaNameBySteamIdAsync(steam32Id);

            var newAccount = new DotaAccount { SteamId = steam32Id, PersonaName = personaName };

            _dotaAccountRepository.Create(newAccount);

            await _dbUnitOfWork.CommitAsync();

            dbUser.AccountId = newAccount.Id;

            _userRepository.Update(dbUser);

            await _dotaMatchService.GetMatchesAndAssignToAccountAsync(newAccount.Id, steam32Id);

            return newAccount;
        }
    }
}
