using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace Manito.Discord.Client
{

    public class MyDiscordClient
    {
        private MyService _collection;
        private EventBuffer _eventBuffer;
        public EventBuffer EventsBuffer => _eventBuffer;
        private ApplicationCommands _appCommands;
        public ApplicationCommands AppCommands => _appCommands;
        private EventInline _eventInliner;
        public EventInline EventInliner => _eventInliner;
        private DiscordClient _client;
        public DiscordClient Client => _client;
        private ActivitiesTools _activitiesTools;
        public ActivitiesTools ActivityTools => _activitiesTools;
        private ExecThread _executionThread;
        public ExecThread ExecutionThread => _executionThread;
        public MyDiscordClient(MyService collection)
        {
            var config = new DiscordConfiguration();
            config.Token = "OTU4MDk4NDIzMzgxMzY0NzQ2.YkIYsA.P-D1NMIwuFwpiveg5TJXVHAcUUM";
            _client = new DiscordClient(config);
            _appCommands = new ApplicationCommands(collection);
            _collection = collection;
            _client.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,

                Timeout = TimeSpan.FromSeconds(30)
            });

            _eventInliner = new EventInline(new EventBuffer(_client));

            _activitiesTools = new ActivitiesTools(_eventInliner);

            _eventBuffer = new EventBuffer(_eventInliner);

            _executionThread = new ExecThread();
        }

        public async Task Start()
        {
            await _client.ConnectAsync();
            await _client.InitializeAsync();
        }
        private IEnumerable<Task> GetRunners()
        {
            yield return _eventInliner.Run();
            yield return _eventBuffer.EventLoops();
            yield return _executionThread.Run();
        }
        public Task StartLongTerm() => Task.WhenAll(GetRunners());
    }
}
