using System.Text;
using DevMentorAI.Models;

namespace DevMentorAI.Services;

public class PromptService
{
    public async Task<string> BuildPromptAsync(
    LearningTopic todayTopic,
    List<RevisionTopic> revisionTopics,
    List<NewsItem> news)
    {
        var template =
            await File.ReadAllTextAsync("Templates/DailyPrompt.txt");

        var revision = revisionTopics.Any()
            ? string.Join(
                Environment.NewLine,
                revisionTopics.Select(x =>
                    $"• Day {x.Day}: {x.Topic}"))
            : "No revision today.";

        var latestNews = news.Any()
            ? string.Join(
                Environment.NewLine,
                news
                    .Take(7)
                    .Select(x =>
                        $"• {x.Title} ({x.Source})\n  URL: {x.Link}"))
            : "No news available.";

        template = template.Replace(
            "{{TOPIC}}",
            todayTopic.Topic);

        template = template.Replace(
            "{{DIFFICULTY}}",
            todayTopic.Difficulty);

        template = template.Replace(
            "{{REVISION}}",
            revision);

        template = template.Replace(
            "{{NEWS}}",
            latestNews);

        return template;
    }
}