using Microsoft.EntityFrameworkCore;
using StudyManagement.Data;
using StudyManagement.Helpers;
using StudyManagement.Models;
using StudyManagement.Models.ViewModels;

namespace StudyManagement.Services;

public class StudyAnalyticsService
{
    private readonly AppDbContext _db;

    public StudyAnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> ComputePomodoroStreakDaysAsync()
    {
        var today = DateTime.UtcNow.Date;
        var d = today;

        var hasToday = await _db.PomodoroSessions
            .AnyAsync(p => p.CompletedAtUtc >= d && p.CompletedAtUtc < d.AddDays(1));

        if (!hasToday)
            d = d.AddDays(-1);

        var streak = 0;
        while (await _db.PomodoroSessions.AnyAsync(p => p.CompletedAtUtc >= d && p.CompletedAtUtc < d.AddDays(1)))
        {
            streak++;
            d = d.AddDays(-1);
        }

        return streak;
    }

    public async Task<StatisticsViewModel> BuildStatisticsViewModelAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var studyRows = await _db.StudySessions
            .AsNoTracking()
            .Where(s => s.StartAt >= monthStart && s.StartAt < monthEnd)
            .ToListAsync();

        var studyMinutesMonth = studyRows.Sum(s => s.DurationMinutes);
        var pomoMinutesMonth = await _db.PomodoroSessions
            .AsNoTracking()
            .Where(p => p.CompletedAtUtc >= monthStart && p.CompletedAtUtc < monthEnd)
            .SumAsync(p => (int?)p.DurationMinutes) ?? 0;

        var totalMinutesMonth = studyMinutesMonth + pomoMinutesMonth;
        var hoursThisMonthLabel = StudyUiFormat.FormatHoursMinutes(totalMinutesMonth);

        var completedInMonth = studyRows.Count(s => s.IsCompleted);
        var totalStudySessionsInMonth = studyRows.Count;
        var completionRate = totalStudySessionsInMonth == 0
            ? 0
            : (int)Math.Round(100.0 * completedInMonth / totalStudySessionsInMonth);

        var pomodorosAll = await _db.PomodoroSessions.CountAsync();
        var completedStudyAll = await _db.StudySessions.CountAsync(s => s.IsCompleted);
        var totalPomoMinAll = await _db.PomodoroSessions.SumAsync(p => (int?)p.DurationMinutes) ?? 0;
        var productivityScore = Math.Min(9999, pomodorosAll * 12 + completedStudyAll * 8 + totalPomoMinAll / 2);

        var allSessions = await _db.StudySessions
            .AsNoTracking()
            .Include(s => s.Subject)
            .ToListAsync();

        var bySubject = allSessions
            .GroupBy(s => s.Subject?.Name ?? "Unknown")
            .Select(g => new SubjectHoursRow
            {
                SubjectName = g.Key,
                Minutes = g.Sum(x => x.DurationMinutes)
            })
            .OrderByDescending(x => x.Minutes)
            .Take(8)
            .ToList();

        var maxSubj = bySubject.Count == 0 ? 0 : bySubject.Max(x => x.Minutes);
        foreach (var row in bySubject)
            row.BarPercent = maxSubj == 0 ? 0 : (int)Math.Round(100.0 * row.Minutes / maxSubj);

        var byActivity = allSessions
            .GroupBy(s => string.IsNullOrWhiteSpace(s.ActivityLabel) ? "other" : s.ActivityLabel.Trim().ToLowerInvariant())
            .Select(g => new ActivityShareRow
            {
                Label = g.Key,
                Minutes = g.Sum(x => x.DurationMinutes)
            })
            .OrderByDescending(x => x.Minutes)
            .ToList();

        var totalActMin = byActivity.Sum(x => x.Minutes);
        foreach (var row in byActivity)
            row.SharePercent = totalActMin == 0 ? 0 : (int)Math.Round(100.0 * row.Minutes / totalActMin);

        return new StatisticsViewModel
        {
            HoursThisMonthLabel = hoursThisMonthLabel,
            CompletedSessionsThisMonth = completedInMonth,
            CompletionRatePercent = completionRate,
            ProductivityScore = productivityScore,
            HoursBySubject = bySubject,
            ActivityDistribution = byActivity
        };
    }

    public async Task<PomodoroViewModel> GetPomodoroStatsAsync()
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        var pomodorosToday = await _db.PomodoroSessions
            .CountAsync(p => p.CompletedAtUtc >= todayUtc && p.CompletedAtUtc < tomorrowUtc);

        var totalFocusMinutes = await _db.PomodoroSessions
            .SumAsync(p => (int?)p.DurationMinutes) ?? 0;

        var streak = await ComputePomodoroStreakDaysAsync();

        return new PomodoroViewModel
        {
            PomodorosToday = pomodorosToday,
            CurrentStreakDays = streak,
            TotalFocusMinutes = totalFocusMinutes
        };
    }

    public async Task<ProfilePageViewModel> BuildProfilePageViewModelAsync(UserProfile profile)
    {
        var now = DateTime.Now;
        var today = DateTime.Today;
        var nextWeek = today.AddDays(7);
        var last7Start = today.AddDays(-6);
        var endOfToday = today.AddDays(1);

        var pendingUpcoming = await _db.StudySessions.CountAsync(s =>
            !s.IsCompleted &&
            s.StartAt >= today &&
            s.StartAt < nextWeek);

        var overdue = await _db.StudySessions.CountAsync(s =>
            !s.IsCompleted &&
            s.StartAt < now);

        var completedLast7 = await _db.StudySessions.CountAsync(s =>
            s.IsCompleted &&
            (s.ActualStartAt ?? s.StartAt) >= last7Start &&
            (s.ActualStartAt ?? s.StartAt) < endOfToday);

        var streak = await ComputePomodoroStreakDaysAsync();

        var statistics = await BuildStatisticsViewModelAsync();

        return new ProfilePageViewModel
        {
            Profile = profile,
            PendingUpcoming7Days = pendingUpcoming,
            OverdueSessions = overdue,
            CompletedLast7Days = completedLast7,
            PomodoroStreakDays = streak,
            Statistics = statistics
        };
    }
}
