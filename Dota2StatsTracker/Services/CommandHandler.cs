using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Dota2StatsTracker.Config;
using Dota2StatsTracker.Exceptions;
using Dota2StatsTracker.Extensions;
using Microsoft.Extensions.Options;
using Victoria;

namespace Dota2StatsTracker.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly string _prefix;

        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly LavaNode _lavaNode;

        private DateTime _lastTimeVoiceServerUpdatedEventHandlerExecuted = DateTime.MinValue;
        private readonly IReadOnlyCollection<string> _positiveEmojis;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, LavaNode lavaNode, IOptions<Settings> settings)
        {
            _prefix = settings.Value.Prefix;
            _provider = provider;
            _client = client;
            _service = service;
            _lavaNode = lavaNode;

            _positiveEmojis = settings.Value.PositiveEmojis.RemoveExtraSpaces().Split(" ");
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.Ready += OnReadyAsync;
            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            _client.ReactionAdded += OnReactionAdded;

            _service.CommandExecuted += OnCommandExecuted;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedEntity, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var emoteName = reaction.Emote.Name;

            if (_positiveEmojis.Contains(emoteName))
            {
                if (_lavaNode.Players.Any())
                {
                    var player = _lavaNode.Players.First();
                    var searchResponse = await _lavaNode.SearchAsync(Enums.PhraseTypeConverter.AudioPathFromPhraseType(Enums.PhraseType.Thanks));
                    await player.PlayAsync(searchResponse.Tracks[0]);
                }
                else
                {
                    await channel.SendMessageAsync(Enums.PhraseTypeConverter.TextByPhraseType(Enums.PhraseType.Thanks));
                }
            }
        }

        private async Task OnUserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState voiceState1, SocketVoiceState voiceState2)
        {
            if (socketUser.Id == _client.CurrentUser.Id)
                return;

            var channel = voiceState2.VoiceChannel;
            if (channel == null)
                return;

            var guild = (socketUser as IGuildUser).Guild;

            if (!_lavaNode.HasPlayer(guild))
                await _lavaNode.JoinAsync(channel);

            if (_lastTimeVoiceServerUpdatedEventHandlerExecuted.AddSeconds(5) > DateTime.Now)
                return;

            var searchResponse = await _lavaNode.SearchAsync(Enums.PhraseTypeConverter.AudioPathFromPhraseType(Enums.PhraseTypeConverter.GetRandomGreetingPhraseType()));
            await _lavaNode.GetPlayer(guild).PlayAsync(searchResponse.Tracks[0]);

            _lastTimeVoiceServerUpdatedEventHandlerExecuted = DateTime.Now;
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess)
            {
                var channel = context.Channel as ISocketMessageChannel;

                if (result is ExecuteResult execResult && execResult.Exception is PudgeBotException pudgeEx)
                {
                    await channel.SendMessageAsync(pudgeEx.Message);

                    if (pudgeEx.FileForTipUrl != null)
                        await channel.SendFileAsync(pudgeEx.FileForTipUrl);

                    return;
                }

                await channel.SendMessageAsync($"Паджу стало плохо {result.ErrorReason}");
            }
        }

        private async Task OnReadyAsync()
        {
            if (!_lavaNode.IsConnected)
                await _lavaNode.ConnectAsync();
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage {Source: MessageSource.User} message))
                return;

            var argPos = 0;
            if (!message.HasStringPrefix(_prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }
    }
}
