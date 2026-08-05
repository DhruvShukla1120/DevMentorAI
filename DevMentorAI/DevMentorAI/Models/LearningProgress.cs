using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMentorAI.Models
{
    public class LearningProgress
    {
        public int CurrentDay { get; set; } = 1;

        public DateTime? LastLearningDate { get; set; }

        public List<int> CompletedDays { get; set; } = new();

        public List<int> RevisionQueue { get; set; } = new();
    }
}
