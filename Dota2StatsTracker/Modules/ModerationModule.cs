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
    public class ModerationModule : ModuleBase
    {
        [Command("purge")]
        [Summary("Падж поглотит указанное количество сообщений")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();

            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"Падж поглотил {messages.Count()} сообщений");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }
    }
}
