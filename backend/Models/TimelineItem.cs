using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public enum TimelineItemType
    {
        Education,
        Experience,
        Project
    }

    public class TimelineItem
    {
        public int Id { get; set; }

        [Required]
        public TimelineItemType Type { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty; // Role for Experience, Degree for Education

        [Required]
        [MaxLength(100)]
        public string Organization { get; set; } = string.Empty; // Company for Experience, Institution for Education

        [MaxLength(100)]
        public string? Subtitle { get; set; } // Company for Experience, Field for Education

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(20)]
        public string? Duration { get; set; } // Alternative to start/end dates

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Additional fields based on type
        [MaxLength(100)]
        public string? Tech { get; set; } // For projects

        public List<string> BulletPoints { get; set; } = new(); // For projects and experience

        public int? GraduationYear { get; set; } // For education

        [MaxLength(10)]
        public string? Grade { get; set; } // For education

        public bool IsCurrentPosition { get; set; } = false; // For experience
    }
}
