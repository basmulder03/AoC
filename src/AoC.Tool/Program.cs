using System.Diagnostics;
using AoC.Core;
using AoC.Tool;

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

// Subcommand: auth / refresh-cookie
if (IsAuthCommand(args[0]))
{
    await CookieManager.PromptAndSaveAsync();
    Console.WriteLine("Session cookie updated.");
    return 0;
}

// Positional: year day
if (args.Length < 2 ||
    !int.TryParse(args[0], out var year) ||
    !int.TryParse(args[1], out var day))
{
    PrintUsage();
    return 1;
}

int? part = null;
string? inputPath = null;
var useSample = false;
var timingEnabled = true;
var forceFetch = false;
var benchmark = false;
var benchmarkCount = 100;

for (var i = 2; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--part":
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out var p))
                part = p;
            i++;
            break;

        case "--input":
            if (i + 1 < args.Length)
                inputPath = args[i + 1];
            i++;
            break;

        case "--sample":
            useSample = true;
            break;

        case "--no-timing":
            timingEnabled = false;
            break;

        case "--fetch":
            forceFetch = true;
            break;

        case "--benchmark":
            benchmark = true;
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out var n) && n > 0)
            {
                benchmarkCount = n;
                i++;
            }

            break;
    }
}

if (!AoCRegistry.TryGet(year, day, out var dayImpl) || dayImpl is null)
{
    await Console.Error.WriteLineAsync($"No implementation found for {year} day {day:D2}");
    return 2;
}

// Resolve input
var input = await InputResolver.ResolveInputAsync(
    year, day,
    inputPath: inputPath,
    useSample: useSample,
    forceFetch: forceFetch);

if (input is null)
{
    await Console.Error.WriteLineAsync("Could not obtain input. See messages above.");
    return 3;
}

return benchmark ? RunBenchmark(dayImpl, year, day, input, part, benchmarkCount) : RunOnce(dayImpl, year, day, input, part, timingEnabled);

static bool IsAuthCommand(string arg) =>
    string.Equals(arg, "auth", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(arg, "refresh-cookie", StringComparison.OrdinalIgnoreCase);

static void PrintUsage()
{
    Console.WriteLine("""
                      Usage:
                        aoc <year> <day> [options]
                        aoc auth | refresh-cookie

                      Options:
                        --part 1|2          Run only a single part
                        --sample            Use sample input file
                        --input <path>      Use explicit input file
                        --fetch             Force re-fetch puzzle input
                        --benchmark [N]     Run benchmark with N iterations (default 100)
                        --no-timing         Disable per-run timing output
                      """);
}

static int RunOnce(IAoCDay dayImpl, int year, int day, string input, int? part, bool timingEnabled)
{
    if (part is null or 1)
    {
        var (result1, ms1) = RunTimed(dayImpl.Part1, input, timingEnabled);
        Console.Write("Part 1: ");
        Console.Write(result1);
        if (timingEnabled) Console.Write($"  ({ms1:F3} ms)");
        Console.WriteLine();
    }

    if (part is not (null or 2)) return 0;
    var (result2, ms2) = RunTimed(dayImpl.Part2, input, timingEnabled);
    Console.Write("Part 2: ");
    Console.Write(result2);
    if (timingEnabled) Console.Write($"  ({ms2:F3} ms)");
    Console.WriteLine();

    return 0;

    static (string result, double ms) RunTimed(Func<string, string> partFunc, string input, bool timing)
    {
        if (!timing)
            return (partFunc(input), 0);

        var sw = Stopwatch.StartNew();
        var result = partFunc(input);
        sw.Stop();
        return (result, sw.Elapsed.TotalMilliseconds);
    }
}

static int RunBenchmark(IAoCDay dayImpl, int year, int day, string input, int? part, int iterations)
{
    Console.WriteLine($"Benchmarking {year} day {day:D2}, {iterations} iterations…");

    if (part is null)
    {
        Console.WriteLine("Benchmarking requires --part 1 or 2.");
        return 4;
    }

    Func<string, string> partFunc = part == 1 ? dayImpl.Part1 : dayImpl.Part2;

    // Optional warmup – cheap even in NativeAOT
    _ = partFunc(input);

    var times = new double[iterations];
    var sw = Stopwatch.StartNew();

    for (int i = 0; i < iterations; i++)
    {
        sw.Restart();
        _ = partFunc(input);
        sw.Stop();
        times[i] = sw.Elapsed.TotalMilliseconds;
    }

    var min = times.Min();
    var max = times.Max();
    var avg = times.Average();

    Console.WriteLine($"Part {part} benchmark:");
    Console.WriteLine($"  min : {min:F3} ms");
    Console.WriteLine($"  avg : {avg:F3} ms");
    Console.WriteLine($"  max : {max:F3} ms");

    return 0;
}
