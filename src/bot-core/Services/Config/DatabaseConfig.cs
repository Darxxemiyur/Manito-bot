using System;
using Newtonsoft.Json;



namespace Manito.Discord.Config
{
    [Serializable]
    public class DatabaseConfig
    {
        public string Address { get; set; }
        public string Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public DatabaseConfig()
        {
            Address = "localhost";
            Port = "5432";
            Login = "postgres";
            Password = "postgres";
            Database = "Manito";
        }
        private string LoginString => $"Username={Login};Password={Password};Host={Address};Port={Port};Database={Database}";
        private string OptionsString => "Minimum Pool Size=2";
        public string ConnectionString => $"{LoginString};{OptionsString}";
    }
}