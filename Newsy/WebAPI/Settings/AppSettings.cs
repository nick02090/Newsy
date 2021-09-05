using WebAPI.Settings.Interfaces;

namespace WebAPI.Settings
{
    public class AppSettings : IAppSettings
    {
        public string Secret { get; set; }

        public AppSettings()
        {
        }

        public AppSettings(string secret)
        {
            Secret = secret;
        }
    }
}
