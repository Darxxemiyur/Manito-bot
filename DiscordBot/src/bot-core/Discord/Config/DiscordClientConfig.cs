using System;

using Newtonsoft.Json;



namespace Manito.Discord.Config
{
	[Serializable]
	public class DiscordClientConfig
	{
		public string ClientKey {
			get;
		}
		public DiscordClientConfig()
		{
#if DEBUG
			//DEBUG
			ClientKey = "OTU4MDk4NDIzMzgxMzY0NzQ2.YkIYsA.P-D1NMIwuFwpiveg5TJXVHAcUUM";
#else
			//RELEASE
			ClientKey = "OTgzMzkxMTgwMjYxODUxMTg2.GL1uT4.2p5AxTukTGDyFhfB0gNmgmUd3v1TzuP9joDaYo";
#endif
		}
	}
}