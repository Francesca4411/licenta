using System.ComponentModel.DataAnnotations;

namespace StudyManagement.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string? IdentityUserId { get; set; }

        [Display(Name = "Full name")]
        [MaxLength(120)]
        public string FullName { get; set; } = "";

        [Display(Name = "Email")]
        [MaxLength(120)]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Display(Name = "Faculty / school")]
        [MaxLength(120)]
        public string Faculty { get; set; } = "";

        [Display(Name = "Academic year")]
        [MaxLength(40)]
        public string AcademicYear { get; set; } = "";

        [MaxLength(512)]
        public string? AvatarPhotoPath { get; set; }

        [Display(Name = "Weekly study goal (hours)")]
        [Range(1, 80)]
        public int WeeklyGoalHours { get; set; } = 12;
    }
}
