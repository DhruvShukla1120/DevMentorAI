using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace DevMentorAI.Services;

public class GeminiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;

    public GeminiService(HttpClient http,
        IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var apiKey = _configuration["GEMINI_API_KEY"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("Gemini API Key not found.");

        var requestBody = JsonSerializer.Serialize(new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        });

        var models = GetOrderedModels();

        string? lastError = null;

        foreach (var model in models)
        {
            try
            {
                return await TryGenerateWithRetryAsync(apiKey, model.Name, requestBody, model.MaxRetries);
            }
            catch (Exception ex)
            {
                lastError = ex.Message;

                Console.WriteLine($"Gemini  : Model '{model.Name}' failed. {ex.Message}");

                Console.WriteLine("Gemini  : Falling back to next model...");

                continue;
            }
        }

        throw new Exception(
            $"All Gemini models failed.\n\nLast error:\n{lastError}");
    }

    private async Task<string> TryGenerateWithRetryAsync(
        string apiKey,
        string model,
        string requestBody,
        int maxRetries)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            var content = new StringContent(
                requestBody,
                Encoding.UTF8,
                "application/json");

            var response = await _http.PostAsync(url, content);

            var responseBody = await response.Content.ReadAsStringAsync();

            var statusCode = (int)response.StatusCode;

            // Transient / overloaded / rate-limited -> retry with backoff
            if (statusCode == 429 || statusCode == 500 || statusCode == 503)
            {
                Console.WriteLine(
                    $"Gemini  : {model} returned {statusCode}. Retry {attempt}/{maxRetries}...");

                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5 * attempt + 5));
                }
                else
                {
                    throw new Exception(
                        $"Model '{model}' returned {statusCode} after {maxRetries} attempts.\n\n{responseBody}");
                }

                continue;
            }

            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(responseBody);

            if (!document.RootElement.TryGetProperty("candidates", out var candidates))
            {
                throw new Exception($"Gemini returned an invalid response.\n\n{responseBody}");
            }

            var markdown = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return markdown ?? string.Empty;
        }

        throw new Exception($"Model '{model}' failed unexpectedly.");
    }

    private List<(string Name, int MaxRetries)> GetOrderedModels()
    {
        var models = new List<(string, int)>();

        var primary = _configuration["Gemini:Model"];

        if (!string.IsNullOrWhiteSpace(primary))
        {
            models.Add((primary, 3));
        }

        foreach (var fallback in _configuration
            .GetSection("Gemini:FallbackModels")
            .GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => x != primary))
        {
            models.Add((fallback!, 2));
        }

        return models.Count > 0
            ? models
            : new List<(string, int)>
            {
                ("gemini-3.6-flash", 3)
            };
    }

    public async Task GetModelsAsync()
    {
        var apiKey = _configuration["GEMINI_API_KEY"];

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";

        var response = await _http.GetAsync(url);

        var json = await response.Content.ReadAsStringAsync();

        Console.WriteLine(json);
    }
}