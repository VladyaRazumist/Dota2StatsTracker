using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Dota2StatsTracker.Exceptions;
using Dota2StatsTracker.Jobs;
using Dota2StatsTracker.Services.DbServices;
using Hangfire;
using Hangfire.Storage;
using Infrastructure;

namespace Dota2StatsTracker.Modules
{
    public class UserModule : ModuleBase
    {
        private const string SteamIdUrl = "https://steamid.xyz/";

        private readonly UserService _userService;
        private readonly DotaAccountService _dotaAccountService;

        private readonly GetNotificationsJob _getNotificationsJob;
        private readonly DbUnitOfWork _dbUnitOfWork;

        public UserModule(UserService userService, DotaAccountService dotaAccountService, GetNotificationsJob getNotificationsJob, DbUnitOfWork dbUnitOfWork)
        {
            _userService = userService;
            _dotaAccountService = dotaAccountService;

            _getNotificationsJob = getNotificationsJob;

            _dbUnitOfWork = dbUnitOfWork;
        }

        [Command("Register")]
        [Summary("Регистрация в системе Падж.")]
        public async Task Register()
        {
            var createdUser = await _userService.CreateUserIfDoesNotExists(Context.User.Id, Context.User.Username);

            await _dbUnitOfWork.CommitAsync();

            await ReplyAsync($"{Context.User.Username}, успешно зарегистрирован в системе Падж, ваш ID : {createdUser.Id}");
        }

        [Command("GetNotifications")]
        [Summary("Получать уведомления о новых матчах")]
        public async Task SchedulePlayedMatchNotification()
        {
            var dotaAccount = await _userService.GetUserAccount(Context.User.Id);
            var jobData = JobStorage.Current.GetConnection().GetRecurringJobs().Where(j => j.Id == dotaAccount.SteamId.ToString()).ToArray();

            if (jobData.Any())
            {
                await ReplyAsync($"{Context.User.Username}, Падж уже следит за аккаунтом с steamId {dotaAccount.SteamId} 😎");
                return;
            }

            RecurringJob.AddOrUpdate(($"{dotaAccount.SteamId}"), () => _getNotificationsJob.CheckNewMatches(dotaAccount.SteamId, Context.Channel.Id), "*/5 * * * *");

            await ReplyAsync($"{Context.User.Username}, Падж следит за вашим аккаунтом со steamId {dotaAccount.SteamId}");
        }

        [Command("RemoveNotifications")]
        [Summary("Перестать получать уведомления о новых матчах")]
        public async Task RemovePlayedMatchNotificationFromSchedule()
        {
            var dotaAccount = await _userService.GetUserAccount(Context.User.Id);
            var jobData = JobStorage.Current.GetConnection().GetRecurringJobs().Where(j => j.Id == dotaAccount.SteamId.ToString()).ToArray();

            if (!jobData.Any())
            {
                await ReplyAsync($"{Context.User.Username}, Падж за вами и так не следил 😎");
                return;
            }

            RecurringJob.RemoveIfExists(($"{dotaAccount.SteamId}"));
            await ReplyAsync($"{Context.User.Username}, Падж больше следит за вашим аккаунтом со steamId {dotaAccount.SteamId} 😿");
        }

        [Command("AddSteam")]
        [Summary("Привязать стим аккаунт")]
        public async Task AddSteamAccount(int? steam32Id = null)
        {
            if (!steam32Id.HasValue)
                throw new PudgeBotException($"Укажите steam32Id через пробел, найти можно тут {SteamIdUrl}");

            var account = await _dotaAccountService.CreateAndAssignToUserAsync(steam32Id.Value, Context.User.Id);

            await _dbUnitOfWork.CommitAsync();

            await ReplyAsync($"{Context.User.Username}, Падж успешно привязал аккаунт {account.PersonaName}. Проверьте правильность данных !lastMatch");
        }
    }
}
