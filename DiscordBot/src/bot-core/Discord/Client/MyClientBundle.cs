using DisCatSharp;
using DisCatSharp.Entities;

using Manito.Discord.Config;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Manito.Discord.Client
{
	public class MyClientBundle
	{
		private MyDomain _collection;
		public MyDomain Domain => _collection;
		private EventBuffer _eventBuffer;
		public EventBuffer EventsBuffer => _eventBuffer;
		private ApplicationCommands _appCommands;
		public ApplicationCommands AppCommands => _appCommands;
		private EventInline _eventInliner;
		public EventInline EventInliner => _eventInliner;
		private MyDiscordClient _mclient;
		private DiscordClient _client;
		public DiscordClient Client => _client;
#if DEBUG
		public Task<DiscordGuild> ManitoGuild => _client.GetGuildAsync(958095775324336198, true);
#elif !DEBUG
		public Task<DiscordGuild> ManitoGuild => _client.GetGuildAsync(915355370673811486, true);
#endif
		private ActivitiesTools _activitiesTools;
		public ActivitiesTools ActivityTools => _activitiesTools;

		public MyClientBundle(MyDomain collection, RootConfig rconfig)
		{
			_collection = collection;
			//TODO: Make safe for use client impementation.
			_mclient = new(this);

			var config = new DiscordConfiguration {
				Token = rconfig.ClientCfg.ClientKey,
				Intents = DiscordIntents.All
			};

			_client = new DiscordClient(config);

			_appCommands = new ApplicationCommands(collection);

			_eventInliner = new EventInline(new EventBuffer(_client));

			_activitiesTools = new ActivitiesTools(_eventInliner);

			_eventBuffer = new EventBuffer(_eventInliner);
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
		}

		public Task StartLongTerm() => Task.WhenAll(GetRunners());
	}
}