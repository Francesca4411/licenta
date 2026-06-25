namespace StudyManagement.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string WelcomeName { get; set; } = "Student";
        public string TodayLabel { get; set; } = "";

        public int ActiveSubjects { get; set; }
        public int UpcomingExams { get; set; }
        public int SessionsToday { get; set; }
        public string HoursThisWeekLabel { get; set; } = "0h 0m";
        public int WeeklyGoalHours { get; set; } = 12;
        public int WeeklyActualMinutes { get; set; }
        public int WeeklyGoalProgressPercent { get; set; }

        public List<DashboardRecommendationRow> Recommendations { get; set; } = new();
        public List<DashboardEvaluationRow> UpcomingEvaluations { get; set; } = new();
    }

    public class DashboardRecommendationRow
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsStrong { get; set; } = false;
        public int Priority { get; set; } = 1;
    }

    public class DashboardEvaluationRow
    {
        public string SubjectName { get; set; } = "";
        public string SubjectColorHex { get; set; } = "#7c3aed";
        public string EvaluationTitle { get; set; } = "";
        public string DateLabel { get; set; } = "";
        public int DaysLeft { get; set; }
        public string PillClass { get; set; } = "";
    }
}
