using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class CV
    {
        public int Id { get; set; }

        public PersonalInfo PersonalInfo { get; set; } = new();

        public List<TimelineItem> TimelineItems { get; set; } = new();

        public List<Skills> Skills { get; set; } = new();

        [MaxLength(10000)]
        public string? RawContent { get; set; } // Original document text

        [MaxLength(10000)]
        public string? ParsedContent { get; set; } // Structured parsed data as JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PersonalInfo
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(255)]
        public string? LinkedIn { get; set; }

        [MaxLength(500)]
        public string? Summary { get; set; }
    }
}
