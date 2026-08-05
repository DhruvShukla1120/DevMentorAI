using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMentorAI.Models
{
    public class LearningTopic
    {
        public int Day { get; set; }

        public string Topic { get; set; } = string.Empty;

        public string Difficulty { get; set; } = string.Empty;
    }
}
