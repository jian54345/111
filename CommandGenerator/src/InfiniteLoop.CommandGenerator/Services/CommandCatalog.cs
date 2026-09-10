using InfiniteLoop.CommandGenerator.Models;

namespace InfiniteLoop.CommandGenerator.Services;

public static class CommandCatalog
{
    public static IReadOnlyList<CommandDefinition> All { get; } =
    [
        // Character
        new()
        {
            Category = "Character",
            Name = "添加全部角色",
            Command = "/character add all",
            Description = "Add all characters."
        },
        new()
        {
            Category = "Character",
            Name = "全部 Max",
            Command = "/character modify all max",
            Description = "Apply the current server-side character max preset."
        },
        new()
        {
            Category = "Character",
            Name = "全部 Min",
            Command = "/character modify all min",
            Description = "Restore the configured minimum character progression."
        },
        new()
        {
            Category = "Character",
            Name = "全部 Reset",
            Command = "/character reset all",
            Description = "Reset all characters to the server-defined initial state.",
            Dangerous = true
        },
        new()
        {
            Category = "Character",
            Name = "全部等级",
            Command = "/character modify all level {level}",
            Description = "Set all character levels.",
            Parameter = "level",
            ParameterLabel = "等级"
        },

        // Equip
        new()
        {
            Category = "Equip",
            Name = "全部等级",
            Command = "/equip modify all level {level}",
            Description = "Set all equipment levels.",
            Parameter = "level",
            ParameterLabel = "等级"
        },
        new()
        {
            Category = "Equip",
            Name = "全部 Max",
            Command = "/equip modify all level max",
            Description = "Use the configured equipment maximum."
        },
        new()
        {
            Category = "Equip",
            Name = "全部 Reset",
            Command = "/equip reset all",
            Description = "Reset all equipment to the server-defined initial state.",
            Dangerous = true
        },

        // Level
        new()
        {
            Category = "Level",
            Name = "设置等级",
            Command = "/level {level}",
            Description = "Set commandant/player level.",
            Parameter = "level",
            ParameterLabel = "等级"
        },
        new()
        {
            Category = "Level",
            Name = "Max",
            Command = "/level max",
            Description = "Set commandant level to the server maximum."
        },
        new()
        {
            Category = "Level",
            Name = "Reset",
            Command = "/level reset",
            Description = "Reset commandant level to the server-defined initial state.",
            Dangerous = true
        },

        // Guide
        new()
        {
            Category = "Guide",
            Name = "全部完成",
            Command = "/guide all",
            Description = "Complete all applicable guides."
        },

        // System
        new()
        {
            Category = "System",
            Name = "Save",
            Command = "/save",
            Description = "Persist current session data."
        }
    ];

    public static IReadOnlyList<CommandDefinition> GetCategory(string category) =>
        All.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

    public static IReadOnlyList<string> Categories =>
        All.Select(x => x.Category).Distinct().ToList();
}
