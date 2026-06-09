namespace AuthService.Services;

public class BlockingService
{
    private readonly Dictionary<string, DateTime> _blocked = new();
    private readonly object _lock = new();

    public void Block(string username, int minutes = 5)
    {
        lock (_lock)
        {
            _blocked[username] =
                DateTime.UtcNow.AddMinutes(minutes);
            Console.WriteLine(
                $"[Block] {username} u bllokua {minutes} min");
        }
    }

    public bool IsBlocked(string username)
    {
        lock (_lock)
        {
            if (!_blocked.TryGetValue(username, out var until))
                return false;
            if (DateTime.UtcNow > until)
            {
                _blocked.Remove(username);
                return false;
            }
            return true;
        }
    }

    public DateTime? GetBlockedUntil(string username)
    {
        lock (_lock)
        {
            return _blocked.TryGetValue(username, out var t)
                ? t : null;
        }
    }

    public List<object> GetAllBlocked()
    {
        lock (_lock)
        {
            return _blocked
                .Where(x => x.Value > DateTime.UtcNow)
                .Select(x => (object)new
                {
                    username = x.Key,
                    until = x.Value
                })
                .ToList();
        }
    }
}