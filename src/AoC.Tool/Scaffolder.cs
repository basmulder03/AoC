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

        if (!EnsureYearProject(repoRoot, year))
        {
            return 1;
        }

        var projectDir = Path.Combine(repoRoot, "src", $"AoC.Y{year}");
        var solutionDir = Path.Combine(repoRoot, "solutions", $"AoC.Y{year}");
        var dayFilePath = Path.Combine(solutionDir, $"Day{day:00}.cs");
        var inputsDayDir = Path.Combine(solutionDir, "inputs", $"{year}", $"{day:00}");
        var samplePath = Path.Combine(inputsDayDir, "sample.txt");
        var inputPath = Path.Combine(inputsDayDir, "input.txt");

        Directory.CreateDirectory(projectDir);
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
        Console.WriteLine($"Scaffolded {year} Day {day:00} successfully.");
        return 0;
    }

    private static bool EnsureYearProject(string repoRoot, int year)
    {
        var projDir = Path.Combine(repoRoot, "solutions", $"AoC.Y{year}");
        var projPath = Path.Combine(projDir, $"AoC.Y{year}.csproj");
        if (File.Exists(projPath))
        {
            return true;
        }
        
        Console.WriteLine($"Creating year project AoC.Y{year}");

        Directory.CreateDirectory(projDir);

        if (!RunDotnet(repoRoot, $"new classlib -n AoC.Y{year} -o solutions/AoC.Y{year}"))
        {
            return false;
        }

        var slnPath = GetSolutionFilePath();
        if (File.Exists(slnPath))
        {
            if (!RunDotnet(repoRoot, $"sln {GetSolutionFileName()} add solutions/AoC.Y{year}/AoC.Y{year}.csproj"))
            {
                Console.WriteLine($"Warning: Could not add project to solution (file may be in use). You can manually add it later.\nTo add it, run:\n  dotnet sln {GetSolutionFileName()} add solutions/AoC.Y{year}/AoC.Y{year}.csproj");
            }
        }

        RunDotnet(repoRoot, $"add solutions/AoC.Y{year}/AoC.Y{year}.csproj reference src/AoC.Core/AoC.Core.csproj");
        RunDotnet(repoRoot, $"add solutions/AoC.Y{year}/AoC.Y{year}.csproj reference src/AoC.Shared/AoC.Shared.csproj");
        RunDotnet(repoRoot, $"add src/AoC.Tool/AoC.Tool.csproj reference solutions/AoC.Y{year}/AoC.Y{year}.csproj");
        
        // Remove the default Class1.cs
        var class1Path = Path.Combine(projDir, "Class1.cs");
        if (File.Exists(class1Path))
        {
            File.Delete(class1Path);
        }
        
        Console.WriteLine($"Created and wired project AoC.Y{year}");
        return true;
    }

    private static bool RunDotnet(string workingDir, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var proc = Process.Start(psi);
        if (proc is null)
        {
            Console.Error.WriteLine("Failed to start dotnet process.");
            return false;
        }

        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        
        if (!string.IsNullOrWhiteSpace(stdout)) Console.Write(stdout);
        if (!string.IsNullOrWhiteSpace(stderr)) Console.Error.Write(stderr);

        if (proc.ExitCode == 0) return true;
        Console.Error.WriteLine($"dotnet {arguments} exited with {proc.ExitCode}.");
        return false;
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
        var ns = $"AoC.Y{year}";
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

    private static string GetSolutionFilePath()
    {
        var repoRoot = FindRepoRoot();
        return repoRoot is null 
            ? throw new InvalidOperationException("Cannot find repo root.") 
            : Path.Combine(repoRoot, GetSolutionFileName());
    }

    private static string GetSolutionFileName()
    {
        return File.Exists("AoC.slnx") ? "AoC.slnx" : "AoC.sln";
    }
}