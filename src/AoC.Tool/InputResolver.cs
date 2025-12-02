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
                var samplePath = FindInputPath(year, day, "sample.txt");
                if (samplePath != null)
                {
                    return await File.ReadAllTextAsync(samplePath);
                }
                
                await Console.Error.WriteLineAsync(
                    $"Could not find sample input for {year} day {day:D2}");
                return null;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(
                    $"Could not load sample input for {year} day {day:D2}: {ex.Message}");
                return null;
            }
        }
        
        // Real input default path
        var realInputPath = FindInputPath(year, day, "input.txt");
        if (realInputPath != null && File.Exists(realInputPath) && !forceRefetch)
        {
            return await File.ReadAllTextAsync(realInputPath);
        }
        
        // Either --fetch was requested, or the default input file does not exist
        // For fetching, we still use the year project directory if it exists
        var defaultPath = realInputPath ?? Path.Combine("inputs", $"{year}", $"{day:00}", "input.txt");
        return await HttpInputFetcher.FetchAndStoreAsync(year, day, defaultPath);
    }

    private static string? FindInputPath(int year, int day, string fileName)
    {
        // Try year-specific project first (new structure)
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepoRoot(currentDir);
        
        if (repoRoot != null)
        {
            var yearProjectPath = Path.Combine(repoRoot, "solutions", $"AoC.Y{year}", "inputs", $"{day:00}", fileName);
            if (File.Exists(yearProjectPath))
            {
                return yearProjectPath;
            }
        }
        

        return null;
    }

    private static string? FindRepoRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "AoC.slnx")) ||
                Directory.Exists(Path.Combine(current.FullName, "src")))
            {
                return current.FullName;
            }
            
            current = current.Parent;
        }
        
        return null;
    }
}
