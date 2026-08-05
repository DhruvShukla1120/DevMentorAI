using System.Text.Json;
using DevMentorAI.Models;

namespace DevMentorAI.Services;

public class ProgressService
{
    private readonly string _projectFolder =
    Directory.GetParent(AppContext.BaseDirectory)!
        .Parent!
        .Parent!
        .Parent!
        .FullName;

private string ProgressPath =>
    Path.Combine(_projectFolder, "Resources", "LearningProgress.json");

    public async Task<LearningProgress> GetProgressAsync()
    {
        if (!File.Exists(ProgressPath))
        {
            var progress = new LearningProgress();

            await SaveProgressAsync(progress);

            return progress;
        }

        try
        {
            var json = await File.ReadAllTextAsync(ProgressPath);

            return JsonSerializer.Deserialize<LearningProgress>(json)
                   ?? new LearningProgress();
        }
        catch (JsonException)
        {
            Console.WriteLine("LearningProgress.json is invalid.");
            Console.WriteLine("Creating a new progress file...");

            var progress = new LearningProgress();

            await SaveProgressAsync(progress);

            return progress;
        }
    }

    public async Task SaveProgressAsync(LearningProgress progress)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json =
            JsonSerializer.Serialize(progress, options);

        await File.WriteAllTextAsync(ProgressPath, json);
    }

    public async Task CompleteTodayAsync()
    {
        var progress =
            await GetProgressAsync();

        if (!progress.CompletedDays.Contains(progress.CurrentDay))
        {
            progress.CompletedDays.Add(progress.CurrentDay);
        }

        progress.LastLearningDate = DateTime.Today;

        progress.CurrentDay++;

        await SaveProgressAsync(progress);
    }
}