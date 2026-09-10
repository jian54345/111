using System.Text.Json;
using InfiniteLoop.CommandGenerator.Models;

namespace InfiniteLoop.CommandGenerator.Services;

public sealed class SettingsService
{
    private readonly string _path = Path.Combine(
        AppContext.BaseDirectory, "appsettings.local.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path))
                return new AppSettings();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_path, json);
    }
}
