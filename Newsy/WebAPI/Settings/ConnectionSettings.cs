using WebAPI.Settings.Interfaces;

namespace WebAPI.Settings
{
    public class ConnectionSettings : IConnectionSettings
    {
        public string ConnectionString { get; set; }
        public string DbConnectionString { get; set; }

        public ConnectionSettings()
        {
        }

        public ConnectionSettings(string connectionString)
        {
            ConnectionString = connectionString;
        }


    }
}
