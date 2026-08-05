using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMentorAI.Models
{
    public class RoadmapModule
    {
        public string Module { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<LearningTopic> Topics { get; set; } = new();
    }
}
