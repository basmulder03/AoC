using System.Diagnostics;
using AoC.Core;

namespace AoC.Tool.Services;

public class ExecutionService
{
    public int RunOnce(IAoCDay dayImpl, int year, int day, string input, int? part, bool timingEnabled)
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
    }

    public int RunBenchmark(IAoCDay dayImpl, int year, int day, string input, int? part, int iterations)
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

    private static (string result, double ms) RunTimed(Func<string, string> partFunc, string input, bool timing)
    {
        if (!timing)
            return (partFunc(input), 0);

        var sw = Stopwatch.StartNew();
        var result = partFunc(input);
        sw.Stop();
        return (result, sw.Elapsed.TotalMilliseconds);
    }
}
