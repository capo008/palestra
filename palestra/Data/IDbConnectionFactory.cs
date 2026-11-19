using MySqlConnector;

namespace palestra.Data;

public interface IDbConnectionFactory
{
    /// <summary>Apre una nuova connessione al database (da chiudere con "await using").</summary>
    Task<MySqlConnection> OpenAsync();
}

public sealed class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Palestra")
            ?? throw new InvalidOperationException(
                "Manca la connection string 'ConnectionStrings:Palestra' (appsettings.json, user-secrets o variabile d'ambiente).");
    }

    public async Task<MySqlConnection> OpenAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
