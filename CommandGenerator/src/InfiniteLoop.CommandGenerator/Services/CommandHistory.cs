namespace InfiniteLoop.CommandGenerator.Services;

public sealed class CommandHistory
{
    private readonly LinkedList<string> _items = new();
    private readonly int _limit;

    public CommandHistory(int limit = 50) => _limit = Math.Max(1, limit);

    public IReadOnlyList<string> Items => _items.ToList();

    public void Add(string command)
    {
        command = command.Trim();
        if (command.Length == 0) return;

        _items.Remove(command);
        _items.AddFirst(command);

        while (_items.Count > _limit)
            _items.RemoveLast();
    }
}
