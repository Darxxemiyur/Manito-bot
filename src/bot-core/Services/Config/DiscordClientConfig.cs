using System;
using Newtonsoft.Json;



namespace Manito.Discord.Config
{
    [Serializable]
    public class DiscordClientConfig
    {
        public string ClientKey { get; set; }
        public DiscordClientConfig()
        {

        }
    }
}