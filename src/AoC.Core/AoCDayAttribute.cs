namespace AoC.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AoCDayAttribute(int year, int day) : Attribute
{
    public int Year { get; } = year;
    public int Day { get; } = day;
}