namespace QuatiTimeWebApi.Models;

public record RecordDto(
    int Id,
    int TaskId,
    string TaskName,
    string Project,
    DateTime Date,
    string Description,
    decimal Time,
    bool Synchronized
);
