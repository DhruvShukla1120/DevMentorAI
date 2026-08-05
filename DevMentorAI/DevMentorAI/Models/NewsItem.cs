namespace DevMentorAI.Models;

public class NewsItem
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public DateTime PublishedOn { get; set; }
}
