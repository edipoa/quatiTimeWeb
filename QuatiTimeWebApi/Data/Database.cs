using Microsoft.Data.Sqlite;

namespace QuatiTimeWebApi.Data;

public class Database
{
    private readonly string _connectionString;
    public readonly string FilePath;

    public Database(IConfiguration configuration)
    {
        var configured = configuration["Database:Path"] ?? "quatitimeweb.db";

        // Usa caminho absoluto se já for absoluto; caso contrário ancora no diretório
        // do executável para ser consistente entre `dotnet run` e IDE.
        FilePath = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);

        _connectionString = $"Data Source={FilePath}";
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
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id      TEXT    NOT NULL,
                task_id      INTEGER NOT NULL,
                date         TEXT    NOT NULL,
                description  TEXT    NOT NULL,
                time         REAL    NOT NULL,
                synchronized INTEGER NOT NULL DEFAULT 0
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }
}
