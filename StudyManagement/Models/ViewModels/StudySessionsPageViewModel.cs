namespace StudyManagement.Models.ViewModels
{
    public class StudySessionsPageViewModel
    {
        public string Tab { get; set; } = "planned";

        public List<StudySessionRowDto> Sessions { get; set; } = new();

        public List<SubjectChoiceDto> SubjectOptions { get; set; } = new();
    }

    public class StudySessionRowDto
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = "";
        public string SubjectColorHex { get; set; } = "#7c3aed";
        public string SubjectColorBg { get; set; } = "rgba(124,58,237,0.18)";

        public DateTime StartAt { get; set; }
        public int DurationMinutes { get; set; }
        public string ActivityLabel { get; set; } = "review";

        public bool IsCompleted { get; set; }
        public int? ActualDurationMinutes { get; set; }
        public DateTime? ActualStartAt { get; set; }
        public string DifficultyLabel { get; set; } = "";
        public string? Notes { get; set; }
    }

    public class SubjectChoiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ColorHex { get; set; } = "#7c3aed";
    }
}
