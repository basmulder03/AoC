namespace AoC.Tool.Commands;

public class AuthCommand : BaseCommand
{
    public override async Task<int> ExecuteAsync(string[] args)
    {
        await CookieManager.PromptAndSaveAsync();
        WriteSuccess("Session cookie updated.");
        return 0;
    }
}
