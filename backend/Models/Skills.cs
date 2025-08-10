using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Skills
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        public List<string> SkillsList { get; set; } = new();

        // Many-to-many relationships with TimelineItems
        public List<TimelineItem> RelatedTimelineItems { get; set; } = new();
    }
}
