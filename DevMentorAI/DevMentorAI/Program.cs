using DevMentorAI.Models;
using DevMentorAI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddHttpClient();

services.AddTransient<GeminiService>();
services.AddTransient<NewsService>();
services.AddTransient<PromptService>();
services.AddTransient<RoadmapService>();
services.AddTransient<ProgressService>();
services.AddTransient<RevisionService>();
services.AddTransient<PdfService>();
services.AddTransient<MarkdownParserService>();
services.AddTransient<TelegramService>();

var provider = services.BuildServiceProvider();

Console.WriteLine("========================================================");
Console.WriteLine("                DevMentor AI");
Console.WriteLine("========================================================");
Console.WriteLine();

var progressService = provider.GetRequiredService<ProgressService>();
var roadmapService = provider.GetRequiredService<RoadmapService>();
var revisionService = provider.GetRequiredService<RevisionService>();
var newsService = provider.GetRequiredService<NewsService>();
var promptService = provider.GetRequiredService<PromptService>();
var geminiService = provider.GetRequiredService<GeminiService>();
var pdfService = provider.GetRequiredService<PdfService>();
var parser = provider.GetRequiredService<MarkdownParserService>();
var telegramService = provider.GetRequiredService<TelegramService>();

//------------------------------------------------------
// STEP 1 - Load Learning Progress
//------------------------------------------------------

Console.WriteLine("Loading Learning Progress...");

var progress = await progressService.GetProgressAsync();

Console.WriteLine();
Console.WriteLine("============== Progress ==============");

Console.WriteLine($"Current Day        : {progress.CurrentDay}");
Console.WriteLine($"Completed Topics   : {progress.CompletedDays.Count}");

if (progress.LastLearningDate.HasValue)
{
    Console.WriteLine($"Last Learning Date : {progress.LastLearningDate:dd-MMM-yyyy}");
}
else
{
    Console.WriteLine("Last Learning Date : First Time Learning");
}

Console.WriteLine("======================================");
Console.WriteLine();

//------------------------------------------------------
// STEP 2 - Today's Topic
//------------------------------------------------------

Console.WriteLine("Loading Today's Topic...");

LearningTopic? todayTopic =
    await roadmapService.GetTopicAsync(progress.CurrentDay);

if (todayTopic == null)
{
    Console.WriteLine("🎉 Congratulations!");
    Console.WriteLine("You have completed the entire roadmap.");
    return;
}

Console.WriteLine();
Console.WriteLine("=========== Today's Learning ==========");

Console.WriteLine($"Day        : {todayTopic.Day}");
Console.WriteLine($"Topic      : {todayTopic.Topic}");
Console.WriteLine($"Difficulty : {todayTopic.Difficulty}");

Console.WriteLine("======================================");
Console.WriteLine();

//------------------------------------------------------
// STEP 3 - Revision Topics
//------------------------------------------------------

Console.WriteLine("Loading Revision Topics...");

var revisionTopics =
    await revisionService.GetRevisionTopicsAsync(progress.CurrentDay);

Console.WriteLine();

if (revisionTopics.Any())
{
    Console.WriteLine("========== Revision Topics ==========");

    foreach (var topic in revisionTopics)
    {
        Console.WriteLine($"Day {topic.Day}");
        Console.WriteLine($"Topic      : {topic.Topic}");
        Console.WriteLine($"Difficulty : {topic.Difficulty}");
        Console.WriteLine($"Revision   : {topic.DaysAgo} day(s) ago");
        Console.WriteLine("--------------------------------------");
    }
}
else
{
    Console.WriteLine("No revision topics for today.");
}

Console.WriteLine();

//------------------------------------------------------
// STEP 4 - Latest News
//------------------------------------------------------

Console.WriteLine("Fetching Latest Industry News...");

var news = await newsService.GetLatestNewsAsync();

Console.WriteLine($"News Count : {news.Count}");
Console.WriteLine();

foreach (var item in news.Take(10))
{
    Console.WriteLine($"• {item.Title}");
    Console.WriteLine($"  Source : {item.Source}");
    Console.WriteLine();
}

//------------------------------------------------------
// STEP 5 - Build Prompt
//------------------------------------------------------

Console.WriteLine("Building AI Prompt...");

var prompt =
    await promptService.BuildPromptAsync(
        todayTopic,
        revisionTopics,
        news);

//------------------------------------------------------
// STEP 6 - Generate AI Report
//------------------------------------------------------

Console.WriteLine("Generating AI Report...");

var report =
    await geminiService.GenerateAsync(prompt);
var document =
    parser.Parse(
        report,
        todayTopic);

//------------------------------------------------------
// STEP 7 - Save Markdown Files
//------------------------------------------------------

var projectFolder =
    Directory.GetParent(AppContext.BaseDirectory)!
        .Parent!
        .Parent!
        .Parent!
        .FullName;

var outputFolder =
    Path.Combine(projectFolder, "Output");

Directory.CreateDirectory(outputFolder);

var promptFile =
    Path.Combine(outputFolder, "Prompt.md");

var reportFile =
    Path.Combine(outputFolder, "DailyReport.md");

var pdfFile =
    Path.Combine(outputFolder, "DailyReport.pdf");

await File.WriteAllTextAsync(promptFile, prompt);

await File.WriteAllTextAsync(reportFile, report);

//------------------------------------------------------
// STEP 8 - Generate PDF
//------------------------------------------------------

Console.WriteLine("Generating PDF...");

await pdfService.GeneratePdfAsync(
    document,
    pdfFile);

//------------------------------------------------------
// STEP 9 - Deliver via Telegram
//------------------------------------------------------

Console.WriteLine();
Console.WriteLine("Sending Report to Telegram...");

var caption =
    $"<b>DevMentor AI - Day {todayTopic.Day}</b>\n" +
    $"\n" +
    $"Topic: {todayTopic.Topic}\n" +
    $"Difficulty: {todayTopic.Difficulty}\n" +
    $"Revision Topics: {revisionTopics.Count}\n" +
    $"News Articles: {news.Count}\n" +
    $"\n" +
    $"Generated for Dhruv Shukla by DevMentor AI.";

if (!telegramService.IsConfigured)
{
    Console.WriteLine("Telegram : Not configured. Skipping delivery.");
    Console.WriteLine("Telegram : Set TELEGRAM_BOT_TOKEN and TELEGRAM_CHAT_ID.");
}
else
{
    await telegramService.SendPdfAsync(
        pdfFile,
        caption);
}

//------------------------------------------------------
// STEP 10 - Complete Today
//------------------------------------------------------

Console.WriteLine();
Console.WriteLine("Updating Learning Progress...");

await progressService.CompleteTodayAsync();

Console.WriteLine("Progress updated successfully.");

//------------------------------------------------------
// STEP 11 - Summary
//------------------------------------------------------

Console.WriteLine();
Console.WriteLine("========================================================");
Console.WriteLine("          DEVMENTOR AI REPORT GENERATED");
Console.WriteLine("========================================================");

Console.WriteLine($"Today's Topic  : {todayTopic.Topic}");
Console.WriteLine($"Difficulty     : {todayTopic.Difficulty}");
Console.WriteLine($"Revision Count : {revisionTopics.Count}");
Console.WriteLine($"News Articles  : {news.Count}");
Console.WriteLine($"Telegram       : {(telegramService.IsConfigured ? "Delivered" : "Skipped (not configured)")}");

Console.WriteLine();

Console.WriteLine("Generated Files");
Console.WriteLine("----------------------------------------");
Console.WriteLine($"Prompt    : {promptFile}");
Console.WriteLine($"Markdown  : {reportFile}");
Console.WriteLine($"PDF       : {pdfFile}");

Console.WriteLine();

Console.WriteLine("Application Completed Successfully.");