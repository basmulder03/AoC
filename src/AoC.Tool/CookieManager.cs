using System.Text.Json;
using System.Text.Json.Serialization;

namespace AoC.Tool;

[JsonSerializable(typeof(AoCConfig))]
internal partial class AoCConfigContext : JsonSerializerContext
{
}

internal static class CookieManager
{
    private const string AppFolderName = "AoC.Tool";
    private const string ConfigFileName = "config.json";

    public static async Task<string?> GetSessionCookieAsync()
    {
        var cfg = await LoadAsync();
        return cfg?.SessionCookie;
    }

    public static async Task SetSessionCookieAsync(string sessionCookie)
    {
        var cfg = await LoadAsync() ?? new AoCConfig();
        cfg.SessionCookie = sessionCookie;
        await SaveAsync(cfg);
    }

    public static async Task PromptAndSaveAsync()
    {
        Console.WriteLine("Enter your session cookie (from https://adventofcode.com):");
        Console.WriteLine("You can find it in your browser's cookies for the https://adventofcode.com domain.");
        Console.WriteLine("Only paste the cookie value (the long hex string), not 'session=...'");
        Console.Write("Session cookie = ");

        var value = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine("No value entered, cookie not updated.");
            return;
        }

        await SetSessionCookieAsync(value.Trim());
        Console.WriteLine("Session cookie updated.");
    }

    private static async Task<AoCConfig?> LoadAsync()
    {
        var path = GetConfigPath();
        if (!File.Exists(path))
        {
            return new AoCConfig();
        }

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<AoCConfig>(json, AoCConfigContext.Default.AoCConfig);
    }

    private static async Task SaveAsync(AoCConfig cfg)
    {
        var path = GetConfigPath();
        var directory = Path.GetDirectoryName(path);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(cfg, AoCConfigContext.Default.AoCConfig);
        await File.WriteAllTextAsync(path, json);
    }

    private static string GetConfigPath()
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(baseDir, AppFolderName);
        return Path.Combine(dir, ConfigFileName);
    }
}