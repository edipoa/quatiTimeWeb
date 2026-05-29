using Microsoft.Data.Sqlite;

namespace QuatiTimeWebApi.Data;

public class Database
{
    private readonly string _connectionString;

    public Database(IConfiguration configuration)
    {
        var path = configuration["Database:Path"] ?? "quatitimeweb.db";
        _connectionString = $"Data Source={path}";
    }

    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public async Task InitializeAsync()
    {
        await using var conn = CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS records (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id     TEXT    NOT NULL,
                task_id     INTEGER NOT NULL,
                date        TEXT    NOT NULL,
                description TEXT    NOT NULL,
                time        REAL    NOT NULL,
                synchronized INTEGER NOT NULL DEFAULT 0
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }
}
