using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Dota2StatsTracker.Services;
using Dota2StatsTracker.Services.ApiServices;

namespace Dota2StatsTracker.Modules
{
    public class VkModule : ModuleBase
    {
        private readonly VkApiService _vkApiService;

        public VkModule(VkApiService vkApiService)
        {
            _vkApiService = vkApiService;
        }

        [Command("freshMeat")]
        [Summary("Рандомный мем с паджом")]
        public async Task FreshMeat()
        {
            var post = await _vkApiService.GetRandomPost();

            var builder = new EmbedBuilder().WithImageUrl(post.AttachmentUrl)
                .WithDescription(post.Description)
                .WithFooter($"🗨 {post.CommentsCount} 👍 {post.LikesCount}");

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
    }
}
