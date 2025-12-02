using System.Diagnostics;
using System.Text;

namespace AoC.Tool;

internal static class Scaffolder
{
    public static async Task<int> ScaffoldDayAsync(int year, int day, bool force)
    {
        var repoRoot = FindRepoRoot();
        if (repoRoot is null)
        {
            await Console.Error.WriteLineAsync("Run from inside the repo (where AoC.sln(x) or src/ exists).");
            return 1;
        }

        var toolProjectDir = Path.Combine(repoRoot, "src", "AoC.Tool");
        var yearDir = Path.Combine(toolProjectDir, "Days", $"Y{year}");
        var dayFilePath = Path.Combine(yearDir, $"Day{day:00}.cs");
        var inputsYearDir = Path.Combine(toolProjectDir, "inputs", $"{year}");
        var inputsDayDir = Path.Combine(inputsYearDir, $"{day:00}");
        var samplePath = Path.Combine(inputsDayDir, "sample.txt");
        var inputPath = Path.Combine(inputsDayDir, "input.txt");

        Directory.CreateDirectory(yearDir);
        Directory.CreateDirectory(inputsDayDir);

        // Day class
        if (File.Exists(dayFilePath) && !force)
        {
            Console.WriteLine($"Day file {dayFilePath} already exists. Use --force to overwrite.");
        }
        else
        {
            var source = GenerateDaySource(year, day);
            await File.WriteAllTextAsync(dayFilePath, source, Encoding.UTF8);
            Console.WriteLine($"{(force ? "Overwrote" : "Created")} day file: {dayFilePath}");
        }

        // inputs/{year}/{day:00}/sample.txt
        if (!File.Exists(samplePath) || force)
        {
            await File.WriteAllTextAsync(samplePath, "# Put sample input here\n", Encoding.UTF8);
            Console.WriteLine($"{(force ? "Overwrote" : "Created")} sample input: {samplePath}");
        }

        // inputs/{year}/{day:00}/input.txt
        if (!File.Exists(inputPath) || force)
        {
            await File.WriteAllTextAsync(inputPath, "# Real input will be fetched or pasted later\n", Encoding.UTF8);
            Console.WriteLine($"{(force ? "Overwrote" : "Created")} real input: {inputPath}");
        }

        Console.WriteLine();
        Console.WriteLine($"Scaffolded {year} Day {day:00} successfully in AoC.Tool project.");
        return 0;
    }

    private static string? FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrWhiteSpace(dir))
        {
            if (File.Exists(Path.Combine(dir, "AoC.slnx")) ||
                Directory.Exists(Path.Combine(dir, "src")))
            {
                return dir;
            }

            var parent = Directory.GetParent(dir);
            if (parent is null) break;
            dir = parent.FullName;
        }

        return null;
    }

    private static string GenerateDaySource(int year, int day)
    {
        var ns = $"AoC.Tool.Days.Y{year}";
        var className = $"Day{day:00}";

        return $$"""
                using AoC.Core;

                namespace {{ns}};

                [AoCDay({{year}}, {{day}})]
                public sealed class {{className}} : IAoCDay
                {
                    public string Part1(string input)
                    {
                        // TODO: Part 1 implementation
                        return string.Empty;
                    }

                    public string Part2(string input)
                    {
                        // TODO: Part 2 implementation
                        return string.Empty;
                    }
                }
                """;
    }
}
