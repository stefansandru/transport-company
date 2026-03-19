using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace persistance;

public class DatabaseConnection
{
    private readonly string connectionString;
    private readonly ILogger<DatabaseConnection> _logger;
    private IDbConnection instance = null;

    public DatabaseConnection(ILogger<DatabaseConnection> logger)
    {
        _logger = logger;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        var rawConnectionString = configuration.GetConnectionString("DefaultConnection");
        var dataSourcePrefix = "Data Source=";
        if (rawConnectionString.StartsWith(dataSourcePrefix))
        {
            var dbPath = rawConnectionString.Substring(dataSourcePrefix.Length);
            if (!Path.IsPathRooted(dbPath))
            {
                dbPath = Path.Combine(AppContext.BaseDirectory, dbPath);
                rawConnectionString = $"{dataSourcePrefix}{dbPath}";
            }
        }
        connectionString = rawConnectionString;

        var dbPathToCheck = rawConnectionString.StartsWith(dataSourcePrefix)
            ? rawConnectionString.Substring(dataSourcePrefix.Length)
            : rawConnectionString;
        if (!File.Exists(dbPathToCheck))
        {
            _logger.LogError($"Database file does not exist at: {dbPathToCheck}");
        }
        else
        {
        }
    }

    private IDbConnection GetNewConnection()
    {
        IDbConnection connection = null;
        try
        {
            connection = new SqliteConnection(connectionString);
            connection.Open();
        }
        catch (SqliteException e)
        {
            _logger.LogError(e, "Error getting connection");
        }

        return connection;
    }

    public IDbConnection GetConnection()
    {
        try
        {
            if (instance == null || instance.State == ConnectionState.Closed)
            {
                instance = GetNewConnection();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with database connection");
        }
        return instance;
    }
}