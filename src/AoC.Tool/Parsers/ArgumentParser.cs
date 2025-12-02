using AoC.Tool.Models;

namespace AoC.Tool.Parsers;

public static class ArgumentParser
{
    public static RunOptions? ParseRunOptions(string[] args)
    {
        if (args.Length < 2 ||
            !int.TryParse(args[0], out var year) ||
            !int.TryParse(args[1], out var day))
        {
            return null;
        }

        var options = new RunOptions
        {
            Year = year,
            Day = day
        };

        for (var i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--part":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var part))
                    {
                        options.Part = part;
                        i++;
                    }
                    break;

                case "--input":
                    if (i + 1 < args.Length)
                    {
                        options.InputPath = args[i + 1];
                        i++;
                    }
                    break;

                case "--sample":
                    options.UseSample = true;
                    break;

                case "--no-timing":
                    options.TimingEnabled = false;
                    break;

                case "--fetch":
                    options.ForceFetch = true;
                    break;

                case "--benchmark":
                    options.Benchmark = true;
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var count) && count > 0)
                    {
                        options.BenchmarkCount = count;
                        i++;
                    }
                    break;
            }
        }

        return options;
    }
}
