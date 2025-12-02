namespace AoC.Tool.Models;

public class RunOptions
{
    public int Year { get; set; }
    public int Day { get; set; }
    public int? Part { get; set; }
    public string? InputPath { get; set; }
    public bool UseSample { get; set; }
    public bool TimingEnabled { get; set; } = true;
    public bool ForceFetch { get; set; }
    public bool Benchmark { get; set; }
    public int BenchmarkCount { get; set; } = 100;
}
