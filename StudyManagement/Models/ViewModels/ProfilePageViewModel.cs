using StudyManagement.Models;

namespace StudyManagement.Models.ViewModels
{
    public class ProfilePageViewModel
    {
        public UserProfile Profile { get; set; } = new();

        public int PendingUpcoming7Days { get; set; }

        public int OverdueSessions { get; set; }

        public int CompletedLast7Days { get; set; }

        public int PomodoroStreakDays { get; set; }

        public StatisticsViewModel Statistics { get; set; } = new();
    }
}
