using System.ComponentModel.DataAnnotations;

namespace StudyManagement.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = "";

        [MaxLength(160)]
        public string Professor { get; set; } = "";

        [Range(0, 60)]
        public int Credits { get; set; }

        [MaxLength(400)]
        public string Description { get; set; } = "";

        [Required, MaxLength(16)]
        public string ColorHex { get; set; } = "#3b82f6";

        public string? IdentityUserId { get; set; }

        public List<Evaluation> Evaluations { get; set; } = new();
    }
}
