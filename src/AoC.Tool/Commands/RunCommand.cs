using AoC.Tool.Parsers;
using AoC.Tool.Services;

namespace AoC.Tool.Commands;

public class RunCommand : BaseCommand
{
    public override async Task<int> ExecuteAsync(string[] args)
    {
        var options = ArgumentParser.ParseRunOptions(args);
        if (options == null)
        {
            WriteError("Usage: aoc <year> <day> [options]");
            PrintUsage();
            return 1;
        }

        if (!AoCRegistry.TryGetDay(options.Year, options.Day, out var dayImpl) || dayImpl is null)
        {
            WriteError($"No implementation found for {options.Year} day {options.Day:D2}");
            return 2;
        }

        // Resolve input
        var input = await InputResolver.ResolveInputAsync(
            options.Year, options.Day,
            inputPath: options.InputPath,
            useSample: options.UseSample,
            forceRefetch: options.ForceFetch);

        // if (input is not null)
        //     return options.Benchmark
        //         ? ExecutionService.RunBenchmark(dayImpl, options.Year, options.Day, input, options.Part,
        //             options.BenchmarkCount)
        //         : ExecutionService.RunOnce(dayImpl, input, options.Part, options.TimingEnabled);
        // WriteError("Could not obtain input. See messages above.");
        return 3;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
                          Options:
                            --part 1|2          Run only a single part
                            --sample            Use sample input file
                            --input <path>      Use explicit input file
                            --fetch             Force re-fetch puzzle input
                            --benchmark [N]     Run benchmark with N iterations (default 100)
                            --no-timing         Disable per-run timing output
                          """);
    }
}
