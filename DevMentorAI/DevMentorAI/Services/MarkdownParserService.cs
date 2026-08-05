using System.Text;
using System.Text.RegularExpressions;
using DevMentorAI.Models;

namespace DevMentorAI.Services;

public class MarkdownParserService
{
    public ReportDocument Parse(
        string markdown,
        LearningTopic topic)
    {
        var document = new ReportDocument
        {
            Title = $"Day {topic.Day} - {topic.Topic}",
            Topic = topic.Topic,
            Difficulty = topic.Difficulty,
            Day = topic.Day,
            GeneratedOn = DateTime.Now,
            ReadingMinutes = 10
        };

        ReportSection? current = null;
        var codeBuffer = new StringBuilder();
        var insideCode = false;

        foreach (var raw in markdown.Split('\n'))
        {
            var line = raw.TrimEnd();

            if (line.StartsWith("```"))
            {
                if (insideCode)
                {
                    current?.Elements.Add(new ReportElement
                    {
                        Type = ReportElementType.Code,
                        Content = codeBuffer.ToString().TrimEnd()
                    });

                    codeBuffer.Clear();
                }

                insideCode = !insideCode;

                continue;
            }

            if (insideCode)
            {
                codeBuffer.AppendLine(line);

                continue;
            }

            if (line.StartsWith("# "))
            {
                current = new ReportSection
                {
                    Title = CleanInline(line.Substring(2))
                };

                document.Sections.Add(current);

                continue;
            }

            if (current == null)
                continue;

            if (string.IsNullOrWhiteSpace(line))
            {
                current.Elements.Add(new ReportElement
                {
                    Type = ReportElementType.Empty
                });

                continue;
            }

            var trimmed = line.TrimStart();

            if (trimmed.StartsWith("### "))
            {
                current.Elements.Add(new ReportElement
                {
                    Type = ReportElementType.Heading3,
                    Content = CleanInline(trimmed.Substring(4))
                });

                continue;
            }

            if (trimmed.StartsWith("## "))
            {
                current.Elements.Add(new ReportElement
                {
                    Type = ReportElementType.Heading2,
                    Content = CleanInline(trimmed.Substring(3))
                });

                continue;
            }

            var indent = (line.Length - line.TrimStart().Length) / 2;

            var link = Regex.Match(trimmed, @"\[([^\]]+)\]\(([^)]+)\)");
            if (link.Success && string.IsNullOrWhiteSpace(link.Groups[1].Value) == false)
            {
                current.Elements.Add(new ReportElement
                {
                    Type = ReportElementType.Link,
                    Content = link.Groups[1].Value,
                    Url = link.Groups[2].Value,
                    Indent = indent
                });

                continue;
            }

            var bareUrl = Regex.Match(trimmed, @"^https?://[\S]+$");
            if (bareUrl.Success)
            {
                current.Elements.Add(new ReportElement
                {
                    Type = ReportElementType.Link,
                    Content = trimmed,
                    Url = trimmed,
                    Indent = indent
                });

                continue;
            }

            if (trimmed.StartsWith("- ")
                || trimmed.StartsWith("* ")
                || trimmed.StartsWith("+ "))
            {
                current.Elements.Add(new ReportElement
                {
                    Type = ReportElementType.Bullet,
                    Content = CleanInline(trimmed.Substring(2)),
                    Indent = indent
                });

                continue;
            }

            var numbered = Regex.Match(trimmed, @"^\d+[\.\)]\s*(.*)$");
            if (numbered.Success)
            {
                current.Elements.Add(new ReportElement
                {
                    Type = ReportElementType.Numbered,
                    Content = CleanInline(numbered.Groups[1].Value),
                    Indent = indent
                });

                continue;
            }

            current.Elements.Add(new ReportElement
            {
                Type = ReportElementType.Paragraph,
                Content = CleanInline(line)
            });
        }

        return document;
    }

    private static string CleanInline(string text)
    {
        text = Regex.Replace(text, @"`([^`]+)`", "$1");
        text = Regex.Replace(text, @"\*\*([^*]+)\*\*", "$1");
        text = Regex.Replace(text, @"\*([^*]+)\*", "$1");
        return text;
    }
}
