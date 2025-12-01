using AoC.Core;

namespace AoC.Tool;

internal static class InputResolver
{
    public static async Task<string?> ResolveInputAsync(
        int year,
        int day,
        string? inputPath,
        bool useSample,
        bool forceRefetch)
    {
        // Explicit path takes precedence
        if (!string.IsNullOrEmpty(inputPath))
        {
            if (!File.Exists(inputPath))
            {
                await Console.Error.WriteLineAsync($"Input file not found: {inputPath}");
                return null;
            }
            
            return await File.ReadAllTextAsync(inputPath);
        }
        
        // Sample input
        if (useSample)
        {
            try
            {
                return InputLoader.LoadSample(year, day);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(
                    $"Could not load sample input for {year} day {day:D2}: {ex.Message}");
                return null;
            }
        }
        
        // Real input default path
        var defaultPath = Path.Combine("inputs", $"{year}", $"{day:00}", "input.txt");
        if (File.Exists(defaultPath) && !forceRefetch)
        {
            return await File.ReadAllTextAsync(defaultPath);
        }
        
        // Either --fetch was requested, or the default input file does not exist
        return await HttpInputFetcher.FetchAndStoreAsync(year, day, defaultPath);
    }
}