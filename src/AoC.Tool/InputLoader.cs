namespace AoC.Tool;

public sealed class InputLoader
{
    public static string Load(int year, int day, string? basePath = null)
    {
        basePath ??= Directory.GetCurrentDirectory();
        var path = Path.Combine(basePath, "inputs", $"{year}", $"{day:00}", "input.txt");
        return File.ReadAllText(path);
    }

    public static string LoadSample(int year, int day, string? basePath = null)
    {
        basePath ??= Directory.GetCurrentDirectory();
        var path = Path.Combine(basePath, "inputs", $"{year}", $"{day:00}", "sample.txt");
        return File.ReadAllText(path);
    }
}
