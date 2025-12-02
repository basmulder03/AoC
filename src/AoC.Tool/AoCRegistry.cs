using AoC.Tool.Interfaces;
using AoC.Y2025;

namespace AoC.Tool;

public static class AoCRegistry
{
    private static readonly IReadOnlyDictionary<int, IAoCYearRegistry> Registries = new Dictionary<int, IAoCYearRegistry>
    {
        [2025] = new AoCYear2025Registry(),
    };

    public static bool TryGetDay(int year, int day, out IAoCDay? dayInstance)
    {
        return !Registries.TryGetValue(year, out var registry) 
            ? throw new ArgumentException($"Year {year} is not supported.", nameof(year)) 
            : registry.TryGetDay(day, out dayInstance);
    }
}