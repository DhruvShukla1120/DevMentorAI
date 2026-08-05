using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMentorAI.Models
{
    public class ReportDocument
    {
        public List<ReportSection> Sections { get; set; } = new();

        public string Title { get; set; } = "";

        public DateTime GeneratedOn { get; set; }

        public string Topic { get; set; } = "";

        public string Difficulty { get; set; } = "";

        public int Day { get; set; }

        public int ReadingMinutes { get; set; } = 10;
    }
}
