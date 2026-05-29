namespace QuatiTimeWebApi.Models;

public record ChatParseRequest(string Message);

public record ChatParseResult(
    int? TaskId,
    string? TaskName,
    DateTime Date,
    string Description,
    decimal Time
);
