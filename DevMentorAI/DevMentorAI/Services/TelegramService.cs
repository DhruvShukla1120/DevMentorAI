using Microsoft.Extensions.Configuration;

namespace DevMentorAI.Services;

public class TelegramService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;

    public TelegramService(HttpClient http,
        IConfiguration configuration)
    {
        _http = http;
        _configuration = configuration;
    }

    public string BotToken =>
        _configuration["TELEGRAM_BOT_TOKEN"] ?? string.Empty;

    public string ChatId =>
        _configuration["TELEGRAM_CHAT_ID"] ?? string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BotToken)
        && !string.IsNullOrWhiteSpace(ChatId);

    private string SendDocumentUrl =>
        $"https://api.telegram.org/bot{BotToken}/sendDocument";

    private string SendMessageUrl =>
        $"https://api.telegram.org/bot{BotToken}/sendMessage";

    public async Task SendPdfAsync(string pdfPath, string caption)
    {
        if (!IsConfigured)
            throw new InvalidOperationException(
                "Telegram is not configured. Set TELEGRAM_BOT_TOKEN and TELEGRAM_CHAT_ID.");

        if (!File.Exists(pdfPath))
            throw new FileNotFoundException(pdfPath);

        using var form = new MultipartFormDataContent();

        form.Add(new StringContent(ChatId), "chat_id");

        await using var fileStream = File.OpenRead(pdfPath);

        var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

        form.Add(fileContent, "document", Path.GetFileName(pdfPath));

        if (!string.IsNullOrWhiteSpace(caption))
        {
            form.Add(new StringContent(caption), "caption");
        }

        var response = await _http.PostAsync(SendDocumentUrl, form);

        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        Console.WriteLine("Telegram : PDF sent successfully.");
        Console.WriteLine(body);
    }

    public async Task SendMessageAsync(string text)
    {
        if (!IsConfigured)
            throw new InvalidOperationException(
                "Telegram is not configured. Set TELEGRAM_BOT_TOKEN and TELEGRAM_CHAT_ID.");

        var payload = new
        {
            chat_id = ChatId,
            text = text,
            parse_mode = "HTML"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);

        var content = new StringContent(
            json,
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _http.PostAsync(SendMessageUrl, content);

        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        Console.WriteLine("Telegram : Message sent successfully.");
        Console.WriteLine(body);
    }
}
