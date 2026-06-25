namespace StudyManagement.Models.ViewModels
{
    public class StatisticsViewModel
    {
        public string HoursThisMonthLabel { get; set; } = "0h 0m";

        public int CompletedSessionsThisMonth { get; set; }

        public int CompletionRatePercent { get; set; }

        public int ProductivityScore { get; set; }

        public List<SubjectHoursRow> HoursBySubject { get; set; } = new();
        public List<ActivityShareRow> ActivityDistribution { get; set; } = new();
    }

    public class SubjectHoursRow
    {
        public string SubjectName { get; set; } = "";
        public int Minutes { get; set; }
        public int BarPercent { get; set; }
    }

    public class ActivityShareRow
    {
        public string Label { get; set; } = "";
        public int Minutes { get; set; }
        public int SharePercent { get; set; }
    }
}
