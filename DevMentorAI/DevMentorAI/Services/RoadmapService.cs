using System.Text.Json;
using DevMentorAI.Models;

namespace DevMentorAI.Services;

public class RoadmapService
{
    private readonly string _projectFolder =
    Directory.GetParent(AppContext.BaseDirectory)!
        .Parent!
        .Parent!
        .Parent!
        .FullName;

    private string RoadmapPath =>
        Path.Combine(_projectFolder, "Resources", "LearningRoadmap.json");

    public async Task<LearningTopic?> GetTopicAsync(int day)
    {
        if (!File.Exists(RoadmapPath))
            throw new FileNotFoundException(RoadmapPath);

        var roadmapJson =
            await File.ReadAllTextAsync(RoadmapPath);

        var roadmap =
            JsonSerializer.Deserialize<List<RoadmapModule>>(roadmapJson)
            ?? new();

        var allTopics =
            roadmap
                .OrderBy(x => x.Order)
                .SelectMany(x => x.Topics)
                .OrderBy(x => x.Day)
                .ToList();

        return allTopics.FirstOrDefault(x => x.Day == day);
    }
}