using System.Text.RegularExpressions;
using PortalHorasApi.Model;
using QuatiTimeWebApi.Models;

namespace QuatiTimeWebApi.Services;

public class ChatParseService
{
    private static readonly Regex TimePattern = new(
        @"(\d+(?:[.,]\d+)?)\s*h(?:oras?)?|(\d+(?:[.,]\d+)?)\s*(?:hora|horas)|(\d+)h(?:(\d+)m(?:in)?)?",
        RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, int> RelativeDays = new()
    {
        ["hoje"] = 0, ["ontem"] = -1, ["anteontem"] = -2
    };

    public ChatParseResult Parse(string message, IList<Tarefa> tasks)
    {
        var time = ExtractTime(message);
        var date = ExtractDate(message);
        var description = ExtractDescription(message);
        var (taskId, taskName) = MatchTask(message, tasks);

        return new ChatParseResult(taskId, taskName, date, description, time);
    }

    private decimal ExtractTime(string message)
    {
        var match = TimePattern.Match(message);
        if (!match.Success) return 0;

        if (!string.IsNullOrEmpty(match.Groups[3].Value) && !string.IsNullOrEmpty(match.Groups[4].Value))
        {
            var hours = int.Parse(match.Groups[3].Value);
            var minutes = int.Parse(match.Groups[4].Value);
            return hours + Math.Round(minutes / 60m, 2);
        }

        var raw = (match.Groups[1].Value + match.Groups[2].Value).Replace(",", ".");
        return decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var val) ? val : 0;
    }

    private DateTime ExtractDate(string message)
    {
        var lower = message.ToLowerInvariant();
        foreach (var (word, offset) in RelativeDays)
            if (lower.Contains(word))
                return DateTime.Today.AddDays(offset);

        // Try to find dd/MM or dd/MM/yyyy
        var dateMatch = Regex.Match(message, @"\b(\d{1,2})[/\-](\d{1,2})(?:[/\-](\d{4}))?\b");
        if (dateMatch.Success)
        {
            var day = int.Parse(dateMatch.Groups[1].Value);
            var month = int.Parse(dateMatch.Groups[2].Value);
            var year = dateMatch.Groups[3].Success ? int.Parse(dateMatch.Groups[3].Value) : DateTime.Today.Year;
            if (day >= 1 && day <= 31 && month >= 1 && month <= 12)
                return new DateTime(year, month, day);
        }

        return DateTime.Today;
    }

    private string ExtractDescription(string message)
    {
        // Remove time expressions to get the description remainder
        var cleaned = TimePattern.Replace(message, "").Trim();
        cleaned = Regex.Replace(cleaned, @"\b(hoje|ontem|anteontem|\d{1,2}[/\-]\d{1,2}(?:[/\-]\d{4})?)\b", "", RegexOptions.IgnoreCase).Trim();
        cleaned = Regex.Replace(cleaned, @"\s{2,}", " ").Trim();
        // Remove common filler words at start
        cleaned = Regex.Replace(cleaned, @"^(trabalhei|fiz|fiquei|gastei|dediquei)\s+(em\s+|na\s+|no\s+|com\s+)?", "", RegexOptions.IgnoreCase).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? message : cleaned;
    }

    private (int? taskId, string? taskName) MatchTask(string message, IList<Tarefa> tasks)
    {
        if (!tasks.Any()) return (null, null);

        var lower = message.ToLowerInvariant();

        // Exact substring match on task name
        foreach (var task in tasks)
        {
            if (task.Nome != null && lower.Contains(task.Nome.ToLowerInvariant()))
                return (task.Id, task.Nome);
        }

        // Fuzzy: score by how many words of the task name appear in the message
        var best = tasks
            .Where(t => t.Nome != null)
            .Select(t =>
            {
                var words = t.Nome!.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var score = words.Count(w => w.Length > 3 && lower.Contains(w));
                return (task: t, score);
            })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .FirstOrDefault();

        return best.task != null ? (best.task.Id, best.task.Nome) : (null, null);
    }
}
