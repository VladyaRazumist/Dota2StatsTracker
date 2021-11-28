using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Victoria;
using Victoria.Enums;

namespace Dota2StatsTracker.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public AudioModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        [Command("Join")]
        [Summary("Падж зайдет в войс канал")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }

            var searchResponse = await _lavaNode.SearchAsync(Enums.PhraseTypeConverter.AudioPathFromPhraseType(Enums.PhraseType.Spawn));
            await _lavaNode.GetPlayer(Context.Guild).PlayAsync(searchResponse.Tracks[0]);
        }

        [Command("Leave")]
        [Summary("Падж покинет войс канал")]
        public async Task LeaveAsync()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Падж уже покинул канал!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            var searchResponse = await _lavaNode.SearchAsync(Enums.PhraseTypeConverter.AudioPathFromPhraseType(Enums.PhraseType.DoNotWannaLeave));

            await _lavaNode.GetPlayer(Context.Guild).PlayAsync(searchResponse.Tracks[0]);
            await Task.Delay(4000);
            await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
        }

        [Command("PlayYT")]
        [Summary("Играть видео с YouTube в войс")]
        public async Task PlayAsync([Remainder] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                var track = searchResponse.Tracks[0];
                player.Queue.Enqueue(track);
                await ReplyAsync($"Enqueued: {track.Title}");
            }
            else
            {
                var track = searchResponse.Tracks[0];

                await player.PlayAsync(track);
                await ReplyAsync($"Now Playing: {track.Title}");
            }
        }


        [Command("Die")]
        [Summary("Падж не боится смерти.")]
        public async Task Die()
        {
            var searchResponse = await _lavaNode.SearchAsync(Enums.PhraseTypeConverter.AudioPathFromPhraseType(Enums.PhraseType.Die));
            await _lavaNode.GetPlayer(Context.Guild).PlayAsync(searchResponse.Tracks[0]);
        }

        [Command("RandomVoice")]
        [Summary("Случайная фраза в войс")]
        public async Task RandomVoicePhrase()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            var randomPhraseType = Enums.PhraseTypeConverter.GetRandomPhraseType();

            var searchResponse = await _lavaNode.SearchAsync(Enums.PhraseTypeConverter.AudioPathFromPhraseType(randomPhraseType));
            await _lavaNode.GetPlayer(Context.Guild).PlayAsync(searchResponse.Tracks[0]);
        }
    }
}
