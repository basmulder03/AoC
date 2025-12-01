using System.Net;
using System.Net.Http.Headers;

namespace AoC.Tool;

internal static class HttpInputFetcher
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://adventofcode.com/"),
        DefaultRequestHeaders =
        {
            UserAgent =
            {
                new ProductInfoHeaderValue(new ProductHeaderValue("AoC-Tool", "1.0")),
                new ProductInfoHeaderValue(new ProductHeaderValue("(https://github.com/basmulder03/AoC-Tool)"))
            }
        }
    };

    public static async Task<string?> FetchAndStoreAsync(int year, int day, string targetPath)
    {
        var input = await FetchAsync(year, day);
        if (input is null) return null;

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        await File.WriteAllTextAsync(targetPath, input);
        
        Console.WriteLine($"Saved input to {targetPath}");
        return input;
    }

    private static async Task<string?> FetchAsync(int year, int day)
    {
        var cookie = await CookieManager.GetSessionCookieAsync();

        if (string.IsNullOrWhiteSpace(cookie))
        {
            Console.WriteLine("No session cookie configured.");
            await CookieManager.PromptAndSaveAsync();
            cookie = await CookieManager.GetSessionCookieAsync();
            if (string.IsNullOrWhiteSpace(cookie))
            {
                Console.WriteLine("Session cookie is still not configured, cannot fetch input.");
                return null;
            }
        }
        
        // First attempt to fetch the input
        var input = await TryDownloadAsync(year, day, cookie);
        if (input is not null) return input;
        
        Console.WriteLine("The current session cookie may be invalid or expired.");
        Console.WriteLine("Please enter a new session cookie.");
        await CookieManager.PromptAndSaveAsync();
        
        cookie = await CookieManager.GetSessionCookieAsync();
        if (string.IsNullOrWhiteSpace(cookie))
        {
            Console.WriteLine("Session cookie is still not configured, cannot fetch input.");
            return null;
        }
        
        // Second attempt with the new cookie
        input = await TryDownloadAsync(year, day, cookie);
        if (input is null)
        {
            await Console.Error.WriteLineAsync("Failed to fetch input even after updating the session cookie.");
        }

        return input;
    }

    private static async Task<string?> TryDownloadAsync(int year, int day, string sessionCookie)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{year}/day/{day}/input");
        request.Headers.Add("Cookie", $"session={sessionCookie}");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("aoc-tool/1.0 (+github.com/basmulder03)"));

        using var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            await Console.Error.WriteLineAsync($"HTTP {(int)response.StatusCode} while fetching input for {year} day {day:D2}.");
        }
        
        var text = await response.Content.ReadAsStringAsync();
        
        // Very crude check: if we got HTML instead of raw input, treat it as failure
        if (text.StartsWith("<!DOCTYPE HTML", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("log in", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return text.TrimEnd('\n', '\r');
    }
}