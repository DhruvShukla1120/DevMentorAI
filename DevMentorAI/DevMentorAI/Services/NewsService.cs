using CodeHollow.FeedReader;
using DevMentorAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMentorAI.Services
{
    public class NewsService
    {
        private readonly List<string> _rssFeeds =
        [
            "https://devblogs.microsoft.com/dotnet/feed/",
        "https://devblogs.microsoft.com/visualstudio/feed/",
        "https://devblogs.microsoft.com/azure-sdk/feed/",
        "https://github.blog/feed/",
        "https://www.infoq.com/feed/"
        ];

        public async Task<List<NewsItem>> GetLatestNewsAsync()
        {
            List<NewsItem> news = [];

            foreach (var rss in _rssFeeds)
            {
                try
                {
                    var feed = await FeedReader.ReadAsync(rss);

                    foreach (var item in feed.Items.Take(8))
                    {
                        news.Add(new NewsItem
                        {
                            Title = item.Title ?? "",
                            Description = item.Description ?? "",
                            Link = item.Link ?? "",
                            PublishedOn = item.PublishingDate ?? DateTime.MinValue,
                            Source = feed.Title ?? "Unknown"
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RSS Error : {rss}");
                    Console.WriteLine(ex.Message);
                }
            }

            return news
                .Where(x => x.Link.StartsWith("http://")
                    || x.Link.StartsWith("https://"))
                .OrderByDescending(x => x.PublishedOn)
                .ToList();
        }
    }
}
