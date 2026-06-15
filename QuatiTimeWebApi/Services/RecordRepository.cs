using Microsoft.Data.Sqlite;
using PortalHorasApi.Model;
using QuatiTimeWebApi.Data;
using QuatiTimeWebApi.Models;

namespace QuatiTimeWebApi.Services;

public record PendingRecord(int Id, int TaskId, DateTime Date, string Description, decimal Time);

public class RecordRepository
{
    private readonly Database _db;

    public RecordRepository(Database db)
    {
        _db = db;
    }

    public async Task<List<RecordDto>> GetAllAsync(string userId, IList<Tarefa> tasks)
    {
        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, task_id, date, description, time, synchronized FROM records WHERE user_id = @userId ORDER BY id DESC";
        cmd.Parameters.AddWithValue("@userId", userId);

        var records = new List<RecordDto>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var taskId = reader.GetInt32(1);
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            records.Add(new RecordDto(
                reader.GetInt32(0),
                taskId,
                task?.Nome ?? "Não localizado",
                task?.Projeto ?? "Não localizado",
                DateTime.Parse(reader.GetString(2)),
                reader.GetString(3),
                (decimal)reader.GetDouble(4),
                reader.GetInt32(5) == 1
            ));
        }
        return records;
    }

    public async Task<int> CreateAsync(string userId, CreateRecordRequest req)
    {
        await using var conn = _db.CreateConnection();

        // INSERT separado do SELECT — Microsoft.Data.Sqlite não suporta multi-statement
        await using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO records (user_id, task_id, date, description, time, synchronized) VALUES (@userId, @taskId, @date, @desc, @time, 0)";
        insert.Parameters.AddWithValue("@userId", userId);
        insert.Parameters.AddWithValue("@taskId", req.TaskId);
        insert.Parameters.AddWithValue("@date", req.Date.ToString("yyyy-MM-dd"));
        insert.Parameters.AddWithValue("@desc", req.Description);
        insert.Parameters.AddWithValue("@time", (double)req.Time);
        await insert.ExecuteNonQueryAsync();

        await using var lastId = conn.CreateCommand();
        lastId.CommandText = "SELECT last_insert_rowid()";
        return Convert.ToInt32(await lastId.ExecuteScalarAsync());
    }

    public async Task UpdateAsync(string userId, int id, UpdateRecordRequest req)
    {
        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE records SET task_id = @taskId, date = @date, description = @desc, time = @time
            WHERE id = @id AND user_id = @userId
            """;
        cmd.Parameters.AddWithValue("@taskId", req.TaskId);
        cmd.Parameters.AddWithValue("@date", req.Date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@desc", req.Description);
        cmd.Parameters.AddWithValue("@time", (double)req.Time);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string userId, int id)
    {
        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM records WHERE id = @id AND user_id = @userId";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteSynchronizedAsync(string userId)
    {
        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM records WHERE user_id = @userId AND synchronized = 1";
        cmd.Parameters.AddWithValue("@userId", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task MarkSynchronizedAsync(string userId, int id)
    {
        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE records SET synchronized = 1 WHERE id = @id AND user_id = @userId";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<PendingRecord>> GetPendingAsync(string userId)
    {
        await using var conn = _db.CreateConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, task_id, date, description, time FROM records WHERE user_id = @userId AND synchronized = 0";
        cmd.Parameters.AddWithValue("@userId", userId);

        var result = new List<PendingRecord>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(new PendingRecord(
                reader.GetInt32(0),
                reader.GetInt32(1),
                DateTime.Parse(reader.GetString(2)),
                reader.GetString(3),
                (decimal)reader.GetDouble(4)
            ));

        return result;
    }
}
