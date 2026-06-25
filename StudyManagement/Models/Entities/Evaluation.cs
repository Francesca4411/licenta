using System.ComponentModel.DataAnnotations;

namespace StudyManagement.Models
{
    public class Evaluation
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        [Required, MaxLength(120)]
        public string Title { get; set; } = "";

        [Required, MaxLength(40)]
        public string DateLabel { get; set; } = "";

        [Range(0, 100)]
        public int WeightPercent { get; set; }
    }
}
