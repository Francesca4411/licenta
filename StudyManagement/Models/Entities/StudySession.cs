using System.ComponentModel.DataAnnotations;

namespace StudyManagement.Models
{
    public class StudySession
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public DateTime StartAt { get; set; }

        public int DurationMinutes { get; set; }

        [MaxLength(40)]
        public string ActivityLabel { get; set; } = "review";

        public bool IsCompleted { get; set; }

        public int? ActualDurationMinutes { get; set; }

        public DateTime? ActualStartAt { get; set; }

        [MaxLength(40)]
        public string DifficultyLabel { get; set; } = "";

        [MaxLength(600)]
        public string? Notes { get; set; }

        public string? IdentityUserId { get; set; }
    }
}
