using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Dota2StatsTracker.Modules
{
    public class GeneralModule : ModuleBase
    {
        private readonly CommandService _commandService;

        public GeneralModule(CommandService commandService)
        {
            _commandService = commandService;
        }

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong~!");
        }

        [Command("info")]
        [Summary("Информация о юзере")]
        public async Task Info(IUser user = null)
        {
            user ??= Context.User;

            var builder = new EmbedBuilder().WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                    .WithDescription("Information")
                    .WithColor(Color.Purple)
                    .AddField("User ID", user.Id, true)
                    .AddField("Discriminator", user.DiscriminatorValue, true)
                    .AddField("Created At", user.CreatedAt.ToString("dd/MM/yyyy"), true)
                    .AddField("Joined At", (user as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                    .AddField("Roles", string.Join(" ", (user as SocketGuildUser).Roles.Select(r => r.Mention)), true)
                    .WithCurrentTimestamp();

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

        [Command("server")]
        [Summary("Информация о сервере")]
        public async Task ServerInfo()
        {
            var builder = new EmbedBuilder().WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Server information")
                .WithTitle(Context.Guild.Name)
                .WithColor(Color.Purple)
                .AddField("Created At", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"), true)
                .AddField("Members", (Context.Guild as SocketGuild).MemberCount, true)
                .AddField("Online users", (Context.Guild as SocketGuild).Users.Count(u => u.Status == UserStatus.Online), true);

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

        [Command("phrase")]
        [Summary("Рандомная текстовая фраза")]
        public async Task RandomPhrase()
        {
            var phrase = Enums.PhraseTypeConverter.TextByPhraseType(Enums.PhraseTypeConverter.GetRandomPhraseType());

            await ReplyAsync(phrase);
        }

        [Command("help")]
        [Summary("Список доступных комманд Паджа")]
        public async Task Help()
        {
            var helpCommandResponse = string.Empty;

            foreach (var module in _commandService.Modules)
            {
                helpCommandResponse += $"**{module.Name}** \n";

                foreach (var command in module.Commands)
                {
                    helpCommandResponse += $"!{command.Name} - {command.Summary ?? "No description"} \n";
                }
            }

            await ReplyAsync(helpCommandResponse);
        }
    }
}
