using AoC.Tool.Commands;

namespace AoC.Tool.Services;

public class CommandRouter
{
    private readonly Dictionary<string, ICommand> _commands;

    public CommandRouter()
    {
        _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
        {
            { "auth", new AuthCommand() },
            { "refresh-cookie", new AuthCommand() },
            { "scaffold", new ScaffoldCommand() },
            { "run", new RunCommand() }
        };
    }

    public async Task<int> RouteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        // Check if it's a named command
        if (_commands.TryGetValue(args[0], out var command))
        {
            var commandArgs = args.Skip(1).ToArray();
            return await command.ExecuteAsync(commandArgs);
        }

        // Default to run command if first arg looks like a year
        if (int.TryParse(args[0], out _))
        {
            var runCommand = new RunCommand();
            return await runCommand.ExecuteAsync(args);
        }

        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
                          Usage:
                            aoc <year> <day> [options]              Run a solution
                            aoc run <year> <day> [options]          Run a solution (explicit)
                            aoc scaffold <year> <day> [--force]     Create day template
                            aoc auth | refresh-cookie               Update session cookie

                          Run Options:
                            --part 1|2          Run only a single part
                            --sample            Use sample input file
                            --input <path>      Use explicit input file
                            --fetch             Force re-fetch puzzle input
                            --benchmark [N]     Run benchmark with N iterations (default 100)
                            --no-timing         Disable per-run timing output

                          Scaffold Options:
                            --force             Overwrite existing files
                          """);
    }
}
