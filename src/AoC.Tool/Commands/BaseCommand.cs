namespace AoC.Tool.Commands;

public abstract class BaseCommand : ICommand
{
    public abstract Task<int> ExecuteAsync(string[] args);
    
    protected static void WriteError(string message)
    {
        Console.Error.WriteLine(message);
    }
    
    protected static void WriteSuccess(string message)
    {
        Console.WriteLine(message);
    }
}
