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

        // Create year project if it doesn't exist
        var yearProjectResult = await EnsureYearProjectAsync(repoRoot, year, force);
        if (yearProjectResult != 0)
        {
            return yearProjectResult;
        }

        var yearProjectDir = Path.Combine(repoRoot, "solutions", $"AoC.Y{year}");
        var daysDir = Path.Combine(yearProjectDir, "Days");
        var dayFilePath = Path.Combine(daysDir, $"Day{day:00}.cs");
        var inputsYearDir = Path.Combine(yearProjectDir, "inputs");
        var inputsDayDir = Path.Combine(inputsYearDir, $"{day:00}");
        var samplePath = Path.Combine(inputsDayDir, "sample.txt");
        var inputPath = Path.Combine(inputsDayDir, "input.txt");

        Directory.CreateDirectory(daysDir);
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
        Console.WriteLine($"Scaffolded {year} Day {day:00} successfully in AoC.Y{year} project.");
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine("  1. Edit the day implementation in the generated file");
        Console.WriteLine("  2. Add sample input to the sample.txt file");
        Console.WriteLine("  3. Run 'dotnet build' to generate registries");
        Console.WriteLine($"  4. Test with: aoc run {year} {day} --sample");
        
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



    private static async Task<int> EnsureYearProjectAsync(string repoRoot, int year, bool force)
    {
        var solutionsDir = Path.Combine(repoRoot, "solutions");
        var yearProjectDir = Path.Combine(solutionsDir, $"AoC.Y{year}");
        var yearProjectFile = Path.Combine(yearProjectDir, $"AoC.Y{year}.csproj");
        
        Directory.CreateDirectory(solutionsDir);
        
        if (!File.Exists(yearProjectFile) || force)
        {
            Directory.CreateDirectory(yearProjectDir);
            
            var projectContent = GenerateYearProjectFile(year);
            await File.WriteAllTextAsync(yearProjectFile, projectContent, Encoding.UTF8);
            Console.WriteLine($"{(force ? "Overwrote" : "Created")} year project: {yearProjectFile}");
            
            // Add to solution file
            await AddProjectToSolutionAsync(repoRoot, $"solutions/AoC.Y{year}/AoC.Y{year}.csproj");
        }
        
        return 0;
    }

    private static string GenerateYearProjectFile(int year)
    {
        return "<Project Sdk=\"Microsoft.NET.Sdk\">" + Environment.NewLine +
               "  <PropertyGroup>" + Environment.NewLine +
               "    <TargetFramework>net10.0</TargetFramework>" + Environment.NewLine +
               "    <ImplicitUsings>enable</ImplicitUsings>" + Environment.NewLine +
               "    <Nullable>enable</Nullable>" + Environment.NewLine +
               "  </PropertyGroup>" + Environment.NewLine + Environment.NewLine +
               "  <ItemGroup>" + Environment.NewLine +
               "    <ProjectReference Include=\"..\\..\\src\\AoC.Shared\\AoC.Shared.csproj\" />" + Environment.NewLine +
               "    <ProjectReference Include=\"..\\..\\src\\AoC.SourceGenerator\\AoC.SourceGenerator.csproj\" OutputItemType=\"Analyzer\" ReferenceOutputAssembly=\"false\" />" + Environment.NewLine +
               "  </ItemGroup>" + Environment.NewLine +
               "</Project>";
    }

    private static async Task AddProjectToSolutionAsync(string repoRoot, string projectPath)
    {
        var solutionFile = Path.Combine(repoRoot, "AoC.slnx");
        if (!File.Exists(solutionFile))
        {
            return;
        }

        var content = await File.ReadAllTextAsync(solutionFile);
        
        // Check if project is already in the solution
        if (content.Contains(projectPath))
        {
            return;
        }

        // Find the end of src folder and add solutions folder if it doesn't exist
        var solutionsFolder = "  <Folder Name=\"/solutions/\">" + Environment.NewLine + 
                             "    <Project Path=\"solutions/";

        if (!content.Contains("<Folder Name=\"/solutions/\">"))
        {
            // Add solutions folder after src folder
            var srcFolderEnd = content.IndexOf("  </Folder>", content.IndexOf("<Folder Name=\"/src/\">", StringComparison.Ordinal), StringComparison.Ordinal);
            if (srcFolderEnd != -1)
            {
                srcFolderEnd = content.IndexOf("  </Folder>", srcFolderEnd, StringComparison.Ordinal) + "  </Folder>".Length;
                content = content.Insert(srcFolderEnd, Environment.NewLine + solutionsFolder + projectPath.Replace("solutions/", "") + "\" />" + Environment.NewLine + "  </Folder>");
            }
        }
        else
        {
            // Add project to existing solutions folder
            var solutionsFolderEnd = content.IndexOf("  </Folder>", content.IndexOf("<Folder Name=\"/solutions/\">", StringComparison.Ordinal), StringComparison.Ordinal);
            if (solutionsFolderEnd != -1)
            {
                var insertPoint = content.LastIndexOf("    <Project Path=\"", solutionsFolderEnd, StringComparison.Ordinal);
                if (insertPoint != -1)
                {
                    var lineEnd = content.IndexOf(Environment.NewLine, insertPoint, StringComparison.Ordinal);
                    if (lineEnd != -1)
                    {
                        content = content.Insert(lineEnd, Environment.NewLine + "    <Project Path=\"" + projectPath + "\" />");
                    }
                }
                else
                {
                    // No existing projects in solutions folder
                    var folderStart = content.IndexOf('>', content.IndexOf("<Folder Name=\"/solutions/\">", StringComparison.Ordinal)) + 1;
                    content = content.Insert(folderStart, Environment.NewLine + "    <Project Path=\"" + projectPath + "\" />");
                }
            }
        }

        await File.WriteAllTextAsync(solutionFile, content);
        Console.WriteLine($"Added project to solution: {projectPath}");
    }



    private static string GenerateDaySource(int year, int day)
    {
        var ns = $"AoC.Y{year}.Days";
        var className = $"Day{day:00}";

        return $"using AoC.SourceGenerator;" + Environment.NewLine + Environment.NewLine +
               $"namespace {ns};" + Environment.NewLine + Environment.NewLine +
               $"[AoCDay({day})]" + Environment.NewLine +
               $"public sealed class {className} : IAoCDay" + Environment.NewLine +
               "{" + Environment.NewLine +
               "    public string Part1(string input)" + Environment.NewLine +
               "    {" + Environment.NewLine +
               "        // TODO: Part 1 implementation" + Environment.NewLine +
               "        return string.Empty;" + Environment.NewLine +
               "    }" + Environment.NewLine + Environment.NewLine +
               "    public string Part2(string input)" + Environment.NewLine +
               "    {" + Environment.NewLine +
               "        // TODO: Part 2 implementation" + Environment.NewLine +
               "        return string.Empty;" + Environment.NewLine +
               "    }" + Environment.NewLine +
               "}";
    }
}
