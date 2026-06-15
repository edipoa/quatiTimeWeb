namespace QuatiTimeWebApi.Models;

public record CreateRecordRequest(int TaskId, DateTime Date, string Description, decimal Time);
public record UpdateRecordRequest(int TaskId, DateTime Date, string Description, decimal Time);
