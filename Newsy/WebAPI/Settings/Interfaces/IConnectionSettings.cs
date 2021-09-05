namespace WebAPI.Settings.Interfaces
{
    public interface IConnectionSettings
    {
        string ConnectionString { get; }
        string DbConnectionString { get; }
    }
}
