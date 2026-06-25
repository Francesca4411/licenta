using System.ComponentModel.DataAnnotations;

namespace StudyManagement.Models
{
    public class PomodoroSession
    {
        public int Id { get; set; }

        public DateTime CompletedAtUtc { get; set; }

        [Range(1, 120)]
        public int DurationMinutes { get; set; } = 25;

        public string? IdentityUserId { get; set; }
    }
}
