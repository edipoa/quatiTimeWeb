using PortalHorasApi.Model;
using System.Collections.Concurrent;

namespace QuatiTimeWebApi.Services;

public class TaskCacheService
{
    private readonly ConcurrentDictionary<string, (IList<Tarefa> Tasks, DateTime FetchedAt)> _cache = new();
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(15);

    public bool TryGet(string userId, out IList<Tarefa> tasks)
    {
        if (_cache.TryGetValue(userId, out var entry) && DateTime.UtcNow - entry.FetchedAt < _ttl)
        {
            tasks = entry.Tasks;
            return true;
        }
        tasks = [];
        return false;
    }

    public void Set(string userId, IList<Tarefa> tasks) =>
        _cache[userId] = (tasks, DateTime.UtcNow);
}
