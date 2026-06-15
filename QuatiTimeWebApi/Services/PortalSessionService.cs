using ErrorOr;
using System.Collections.Concurrent;

namespace QuatiTimeWebApi.Services;

public class PortalSessionService
{
    private readonly PortalHorasApi.Client _client;
    private readonly ConcurrentDictionary<string, PortalHorasApi.ISession> _sessions = new();

    public PortalSessionService(PortalHorasApi.Client client)
    {
        _client = client;
    }

    public async Task<bool> CreateSession(SessionPayload payload)
    {
        var result = await _client.CreateSession(payload.Username, payload.Password);
        if (result.IsError) return false;

        _sessions[payload.UserId] = result.Value;
        return true;
    }

    private async Task<PortalHorasApi.ISession?> GetOrCreate(SessionPayload payload)
    {
        if (_sessions.TryGetValue(payload.UserId, out var existing))
            return existing;

        return await CreateSessionInternal(payload);
    }

    private async Task<PortalHorasApi.ISession?> CreateSessionInternal(SessionPayload payload)
    {
        var result = await _client.CreateSession(payload.Username, payload.Password);
        if (result.IsError) return null;

        _sessions[payload.UserId] = result.Value;
        return result.Value;
    }

    public async Task<ErrorOr<T>> WithRetry<T>(SessionPayload payload, Func<PortalHorasApi.ISession, Task<ErrorOr<T>>> action)
    {
        var session = await GetOrCreate(payload);
        if (session is null)
            return Error.Unauthorized("Auth", "Login no portal falhou");

        ErrorOr<T> response;
        try
        {
            response = await action(session);
        }
        catch (OperationCanceledException)
        {
            return Error.Failure("Timeout", "O portal não respondeu a tempo. Tente novamente.");
        }

        if (!response.IsError || !response.Errors.Any(e => e.Type == ErrorType.Unauthorized))
            return response;

        _sessions.TryRemove(payload.UserId, out _);
        session = await CreateSessionInternal(payload);
        if (session is null)
            return Error.Unauthorized("Auth", "Re-login no portal falhou");

        try
        {
            return await action(session);
        }
        catch (OperationCanceledException)
        {
            return Error.Failure("Timeout", "O portal não respondeu a tempo. Tente novamente.");
        }
    }

    public void RemoveSession(string userId) => _sessions.TryRemove(userId, out _);
}
