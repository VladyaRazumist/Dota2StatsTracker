using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Dota2StatsTracker.Exceptions;
using Dota2StatsTracker.Services.ApiServices;
using Dota2StatsTracker.Services.DbServices;
using Dota2StatsTracker.Services.HelperServices;

namespace Dota2StatsTracker.Modules
{
    public class OpenDotaStatsModule : ModuleBase
    {
        private readonly UserService _userService;

        private readonly OpenDotaApiService _openDotaApiService;
        private readonly DotaConstantService _dotaConstantService;
        private readonly OpenDotaEmberBuilder _emberBuilder;

        public OpenDotaStatsModule(UserService userService, OpenDotaApiService openDotaApiService, OpenDotaEmberBuilder emberBuilder, DotaConstantService dotaConstantService)
        {
            _openDotaApiService = openDotaApiService;
            _userService = userService;
            _dotaConstantService = dotaConstantService;
            _emberBuilder = emberBuilder;
        }

        [Command("profile")]
        [Summary("Профиль")]
        public async Task Profile()
        {
            var userSteamId = (await _userService.GetUserAccount(Context.User.Id)).SteamId;

            var playerProfile = await _openDotaApiService.GetPlayerProfileAsync(userSteamId);
            var playerStats = await _openDotaApiService.GetWinRatioAsync(userSteamId);

            var embed = _emberBuilder.BuildPlayerProfileEmbed(playerProfile, playerStats);

            await ReplyAsync(null, false, embed);
        }

        [Command("hero")]
        [Summary("Информация о герое")]
        public async Task Hero(string heroName = null)
        {
            if (heroName == null)
            {
                await ReplyAsync("Укажи героя бро, вот так : !hero Pudge");
                return;
            }

            var hero = _dotaConstantService.GetHeroByName(heroName);

            if (hero == null)
            {
                var possibleHeroes = _dotaConstantService.FindPossibleHeroesByName(heroName);

                if (!possibleHeroes.Any())
                    throw new PudgeBotException($"{Context.User.Username}, Падж не знает такого героя. А Падж знает все.");

                await ReplyAsync($"Имеешь ввиду : {string.Join(", ", possibleHeroes.Select(h => h.LocalizedName))}, бро?");
                return;
            }

            var embed = _emberBuilder.BuildHeroInfoEmbed(hero);

            await ReplyAsync(null, false, embed);
        }

        [Command("stats")]
        [Summary("Стата героя")]
        public async Task Stats(string heroName = null)
        {
            var heroStats = await _openDotaApiService.GetHeroStatsAsync(heroName);

            if (heroStats == null)
                throw new PudgeBotException($"{Context.User.Username}, Падж не знает такого героя. А Падж знает все.");

            var embed = _emberBuilder.BuildHeroStatsEmbed(heroStats);

            await ReplyAsync(null, false, embed);
        }

        [Command("lastMatch")]
        [Summary("Последний сыгранный матч")]
        public async Task LastMatch()
        {
            var userSteamId = (await _userService.GetUserAccount(Context.User.Id)).SteamId;

            var matchInfo = await _openDotaApiService.GetLastMatchAsync(userSteamId);
            var embed = _emberBuilder.BuildMatchInfoEmbed(matchInfo);

            await ReplyAsync(null, false, embed);
        }
    }
}
