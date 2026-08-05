using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevMentorAI.Services
{
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

            var model = _configuration["Gemini:Model"];

            //var prompt = await File.ReadAllTextAsync("Templates/DailyPrompt.txt");

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var request = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new
                    {
                        text = prompt
                    }
                }
            }
        }
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _http.PostAsync(url, content);

                var responseBody = await response.Content.ReadAsStringAsync();

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
}
