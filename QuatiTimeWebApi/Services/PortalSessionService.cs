using ErrorOr;
using PortalHorasApi;
using PortalHorasApi.Model;
using System.Collections.Concurrent;

namespace QuatiTimeWebApi.Services;

public class PortalSessionService
{
    private readonly Client _client;
    private readonly ConcurrentDictionary<string, ISession> _sessions = new();

    public PortalSessionService(Client client)
    {
        _client = client;
    }

    public async Task<ErrorOr<ISession>> GetOrCreateSession(SessionPayload payload)
    {
        if (_sessions.TryGetValue(payload.UserId, out var existing))
            return existing.ToErrorOr();

        return await CreateSession(payload);
    }

    public async Task<ErrorOr<ISession>> CreateSession(SessionPayload payload)
    {
        var result = await _client.CreateSession(payload.Username, payload.Password);
        if (result.IsError) return result.Errors;

        _sessions[payload.UserId] = result.Value;
        return result.Value;
    }

    public async Task<ErrorOr<T>> WithRetry<T>(SessionPayload payload, Func<ISession, Task<ErrorOr<T>>> action)
    {
        var sessionResult = await GetOrCreateSession(payload);
        if (sessionResult.IsError) return sessionResult.Errors;

        var response = await action(sessionResult.Value);

        if (!response.IsError || !response.Errors.Any(e => e.Type == ErrorType.Unauthorized))
            return response;

        _sessions.TryRemove(payload.UserId, out _);
        var renewed = await CreateSession(payload);
        if (renewed.IsError) return renewed.Errors;

        return await action(renewed.Value);
    }

    public void RemoveSession(string userId) => _sessions.TryRemove(userId, out _);
}
