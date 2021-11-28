using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dota2StatsTracker.Exceptions;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Dota2StatsTracker.Services.DbServices
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> CreateUserIfDoesNotExists(ulong discordUserId, string username)
        {
            var userExists = await _userRepository.AnyAsync(u => u.DiscordUserId == discordUserId);
            if (userExists)
                throw new PudgeBotException($"{username} вы уже зарегистрированы в системе Падж 😎");

            var user = new User { DiscordUserId = discordUserId, Name = username };

            _userRepository.Create(user);

            return user;
        }

        public async Task<User> GetUser(ulong discordUserId)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.DiscordUserId == discordUserId);
            if (user == null)
                throw new PudgeBotException("Необходима регистрация в системе Падж : !Register");

            return user;
        }

        public async Task<DotaAccount> GetUserAccount(ulong discordUserId)
        {
            var userInfo = await _userRepository.FirstOrDefaultAsync(u => u.DiscordUserId == discordUserId, u => new
            {
                u.Account
            });

            if (userInfo == null)
                throw new PudgeBotException("Необходима регистрация в системе Падж : !Register");

            if (userInfo.Account == null)
                throw new PudgeBotException("Необходимо привязать аккаунт в системе Падж : !AddSteam");

            return userInfo.Account;
        }
    }
}
