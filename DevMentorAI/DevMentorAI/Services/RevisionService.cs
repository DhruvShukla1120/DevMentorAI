using DevMentorAI.Models;

namespace DevMentorAI.Services;

public class RevisionService
{
    private readonly RoadmapService _roadmapService;

    public RevisionService(RoadmapService roadmapService)
    {
        _roadmapService = roadmapService;
    }

    public async Task<List<RevisionTopic>> GetRevisionTopicsAsync(int currentDay)
    {
        var revisionDays = new List<int>
        {
            currentDay - 1,
            currentDay - 3,
            currentDay - 7
        };

        revisionDays = revisionDays
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var revisionTopics = new List<RevisionTopic>();

        foreach (var day in revisionDays)
        {
            var topic = await _roadmapService.GetTopicAsync(day);

            if (topic != null)
            {
                revisionTopics.Add(new RevisionTopic
                {
                    Day = topic.Day,
                    Topic = topic.Topic,
                    Difficulty = topic.Difficulty,
                    DaysAgo = currentDay - topic.Day
                });
            }
        }

        return revisionTopics;
    }
}