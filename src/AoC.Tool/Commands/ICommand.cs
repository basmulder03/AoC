namespace AoC.Tool.Commands;

public interface ICommand
{
    Task<int> ExecuteAsync(string[] args);
}
