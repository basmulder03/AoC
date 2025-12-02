namespace AoC.Tool.Commands;

public class ScaffoldCommand : BaseCommand
{
    public override async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 ||
            !int.TryParse(args[0], out var year) ||
            !int.TryParse(args[1], out var day))
        {
            WriteError("Usage: aoc scaffold <year> <day> [--force]");
            return 1;
        }

        var force = args.Contains("--force");
        return await Scaffolder.ScaffoldDayAsync(year, day, force);
    }
}
