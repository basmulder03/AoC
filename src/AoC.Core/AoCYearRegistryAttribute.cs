namespace AoC.Core;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AoCYearRegistryAttribute(int year, Type registryType) : Attribute
{
    public int Year { get; } = year;
    public Type RegistryType { get; } = registryType;
}
