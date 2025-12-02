namespace AoC.Core;

public interface IAoCYearRegistry
{
    int Year { get; }
    IReadOnlyDictionary<int, IAoCDay> Days { get; }
    bool TryGetDay(int day, out IAoCDay? dayInstance);
}
