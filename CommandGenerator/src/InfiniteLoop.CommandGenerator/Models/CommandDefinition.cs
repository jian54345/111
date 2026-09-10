namespace InfiniteLoop.CommandGenerator.Models;

public sealed class CommandDefinition
{
    public string Category { get; init; } = "";
    public string Name { get; init; } = "";
    public string Command { get; init; } = "";
    public string Description { get; init; } = "";
    public string? Parameter { get; init; }
    public string? ParameterLabel { get; init; }
    public bool Dangerous { get; init; }

    public string Build(string? parameter)
    {
        if (string.IsNullOrWhiteSpace(Parameter))
            return Command;

        return Command.Replace("{" + Parameter + "}", parameter?.Trim() ?? "", StringComparison.Ordinal);
    }

    public override string ToString() => Name;
}
