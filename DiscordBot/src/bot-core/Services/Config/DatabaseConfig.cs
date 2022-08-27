using System;

using Newtonsoft.Json;



namespace Manito.Discord.Config
{
	[Serializable]
	public class DatabaseConfig
	{
		public string Address {
			get;
		}
		public string Port {
			get;
		}
		public string Login {
			get;
		}
		public string Password {
			get;
		}
		public string Database {
			get;
		}
		public DatabaseConfig()
		{
			Address = "localhost";
			Port = "5432";
#if DEBUG
			Login = "postgres";
			Password = "postgres";
#else
			Login = "ManitoStuff";
			Password = "6A8C6D7C3188AB4A203A51149FDB66F098B925D57D75F15CB902FB82F68F1436";
#endif
			Database = "Manito";
		}
		private string LoginS => $"Username={Login};Password={Password}";
		private string ConnectS => $"Host={Address};Port={Port}";

		private string LoginString => $"{LoginS};{ConnectS};Database={Database}";
		private string OptionsString => "Minimum Pool Size=2";
		public string ConnectionString => $"{LoginString};{OptionsString}";
	}
}