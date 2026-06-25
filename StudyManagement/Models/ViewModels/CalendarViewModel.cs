namespace StudyManagement.Models.ViewModels
{
    public class CalendarSessionDto
    {
        public int Id { get; set; }
        public int DayIndex { get; set; }

        public double TopPx { get; set; }
        public double HeightPx { get; set; }
        public double LeftPct { get; set; }
        public double WidthPct { get; set; }

        public string SubjectName { get; set; } = "";
        public string SubjectColorHex { get; set; } = "#7c3aed";
        public string SubjectColorBg { get; set; } = "rgba(124,58,237,0.18)";

        public string ActivityLabel { get; set; } = "review";
        public int DurationMinutes { get; set; }
    }

    public class CalendarViewModel
    {
        public DateTime WeekStart { get; set; }
        public string WeekLabel { get; set; } = "";

        public int HourStart { get; set; } = 8;
        public int HourEndExclusive { get; set; } = 22;
        public int HourHeightPx { get; set; } = 56;

        public List<CalendarSessionDto> Sessions { get; set; } = new();
    }
}
