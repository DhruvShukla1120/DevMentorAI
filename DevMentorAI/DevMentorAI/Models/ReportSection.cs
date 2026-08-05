namespace DevMentorAI.Models;

public class ReportSection
{
    public string Title { get; set; } = "";

    public List<ReportElement> Elements { get; set; } = new();
}

public class ReportElement
{
    public ReportElementType Type { get; set; }

    public string Content { get; set; } = "";

    public string? Url { get; set; }

    public int Indent { get; set; }
}

public enum ReportElementType
{
    Heading1,
    Heading2,
    Heading3,
    Paragraph,
    Bullet,
    Numbered,
    Code,
    Link,
    Empty
}
