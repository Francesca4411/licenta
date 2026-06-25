using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyManagement.Data;
using StudyManagement.Models;
using StudyManagement.Models.ViewModels;
using StudyManagement.Services;
using System.Diagnostics;
using System.Globalization;

namespace StudyManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserProfileService _profiles;

        public HomeController(
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            UserProfileService profiles)
        {
            _db = db;
            _userManager = userManager;
            _profiles = profiles;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
                return View("Landing");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var today = DateTime.Today;
            var weekStart = GetWeekStartMonday(today);
            var weekEnd = weekStart.AddDays(7);

            var profile = await _profiles.GetOrCreateAsync(user.Id, user.Email);
            var displayName = UserProfileService.GetDisplayName(profile);
            var weeklyGoalHours = profile is { WeeklyGoalHours: >= 1 and <= 80 }
                ? profile.WeeklyGoalHours
                : 12;

            var subjects = await _db.Subjects.AsNoTracking().ToListAsync();
            var activeSubjects = subjects.Count;

            var sessionsToday = await _db.StudySessions
                .AsNoTracking()
                .CountAsync(s => s.StartAt.Date == today);

            var plannedWeekMinutes = await _db.StudySessions
                .AsNoTracking()
                .Where(s => s.StartAt >= weekStart && s.StartAt < weekEnd)
                .SumAsync(s => (int?)s.DurationMinutes) ?? 0;

            var weekStartUtc = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day, 0, 0, 0, DateTimeKind.Utc);
            var weekEndUtc = weekStartUtc.AddDays(7);

            var pomodoroWeekMinutes = await _db.PomodoroSessions
                .AsNoTracking()
                .Where(p => p.CompletedAtUtc >= weekStartUtc && p.CompletedAtUtc < weekEndUtc)
                .SumAsync(p => (int?)p.DurationMinutes) ?? 0;

            var hoursThisWeekLabel = FormatHoursMinutes(plannedWeekMinutes + pomodoroWeekMinutes);

            var weeklyGoalMinutes = weeklyGoalHours * 60;
            var weeklyActualMinutes = plannedWeekMinutes + pomodoroWeekMinutes;
            var weeklyProgressPercent = weeklyGoalMinutes <= 0
                ? 0
                : (int)Math.Round(100.0 * weeklyActualMinutes / weeklyGoalMinutes);

            if (weeklyProgressPercent > 100) weeklyProgressPercent = 100;
            if (weeklyProgressPercent < 0) weeklyProgressPercent = 0;

            var evaluations = await _db.Evaluations
                .AsNoTracking()
                .Include(e => e.Subject)
                .ToListAsync();

            var upcomingEvalRows = evaluations
                .Select(e => new
                {
                    Eval = e,
                    Date = TryParseDateLabel(e.DateLabel)
                })
                .Where(x => x.Date.HasValue && x.Date.Value.Date >= today)
                .OrderBy(x => x.Date!.Value)
                .Take(8)
                .ToList();

            var vm = new DashboardViewModel
            {
                WelcomeName = displayName,
                TodayLabel = today.ToString("dddd, MMMM dd, yyyy", CultureInfo.GetCultureInfo("en-US")),
                ActiveSubjects = activeSubjects,
                UpcomingExams = upcomingEvalRows.Count,
                SessionsToday = sessionsToday,
                HoursThisWeekLabel = hoursThisWeekLabel,
                WeeklyGoalHours = weeklyGoalHours,
                WeeklyActualMinutes = weeklyActualMinutes,
                WeeklyGoalProgressPercent = weeklyProgressPercent
            };

            foreach (var row in upcomingEvalRows)
            {
                var examDate = row.Date!.Value.Date;
                var daysLeft = (examDate - today).Days;

                vm.UpcomingEvaluations.Add(new DashboardEvaluationRow
                {
                    SubjectName = row.Eval.Subject?.Name ?? "Unknown subject",
                    SubjectColorHex = row.Eval.Subject?.ColorHex ?? "#7c3aed",
                    EvaluationTitle = row.Eval.Title,
                    DateLabel = row.Eval.DateLabel,
                    DaysLeft = daysLeft,
                    PillClass = GetPillClass(daysLeft)
                });

                if (daysLeft <= 7)
                {
                    vm.Recommendations.Add(new DashboardRecommendationRow
                    {
                        Title = row.Eval.Subject?.Name ?? "Subject",
                        Message = daysLeft == 0
                            ? "Evaluation is today. Reserve at least one focused review block."
                            : $"Evaluation in {daysLeft} day(s). Prioritize this subject this week.",
                        IsStrong = daysLeft <= 3,
                        Priority = 2
                    });
                }
            }


            var weekSessions = await _db.StudySessions
                .AsNoTracking()
                .Include(s => s.Subject)
                .Where(s => s.StartAt >= weekStart && s.StartAt < weekEnd)
                .ToListAsync();

            var perSubject = weekSessions
                .GroupBy(s => s.Subject?.Name ?? "Unknown subject")
                .Select(g => new
                {
                    SubjectName = g.Key,
                    Planned = g.Count(),
                    Completed = g.Count(x => x.IsCompleted)
                })
                .OrderBy(x => x.SubjectName)
                .ToList();

            foreach (var s in perSubject)
            {
                if (s.Planned == 0) continue;

                var completionPercent = (int)Math.Round(100.0 * s.Completed / s.Planned);

                if (completionPercent < 50)
                {
                    vm.Recommendations.Add(new DashboardRecommendationRow
                    {
                        Title = s.SubjectName,
                        Message = $"You completed only {completionPercent}% of planned sessions this week. Try one extra session today.",
                        IsStrong = false,
                        Priority = 3
                    });
                }
            }

            if (sessionsToday == 0)
            {
                vm.Recommendations.Add(new DashboardRecommendationRow
                {
                    Title = "Today",
                    Message = "No sessions scheduled for today. Add a 60–90 min block to stay consistent.",
                    IsStrong = false,
                    Priority = 1
                });
            }

            vm.Recommendations = vm.Recommendations
                .DistinctBy(r => $"{r.Title}|{r.Message}")
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.IsStrong)
                .Take(6)
                .ToList();

            if (vm.Recommendations.Count == 0)
            {
                vm.Recommendations.Add(new DashboardRecommendationRow
                {
                    Title = "Great progress",
                    Message = "Your schedule looks balanced this week. Keep the momentum.",
                    IsStrong = false
                });
            }

            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static DateTime GetWeekStartMonday(DateTime date)
        {
            int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return date.Date.AddDays(-diff);
        }

        private static DateTime? TryParseDateLabel(string dateLabel)
        {
            if (string.IsNullOrWhiteSpace(dateLabel))
                return null;

            if (DateTime.TryParseExact(
                dateLabel.Trim(),
                "MMM dd, yyyy",
                CultureInfo.GetCultureInfo("en-US"),
                DateTimeStyles.None,
                out var dt))
            {
                return dt;
            }

            if (DateTime.TryParse(dateLabel, out dt))
                return dt;

            return null;
        }

        private static string FormatHoursMinutes(int totalMinutes)
        {
            if (totalMinutes <= 0) return "0h 0m";
            var h = totalMinutes / 60;
            var m = totalMinutes % 60;
            return $"{h}h {m}m";
        }
            
        private static string GetPillClass(int daysLeft)
        {
            if (daysLeft <= 3) return "";
            if (daysLeft <= 7) return "orange";
            if (daysLeft <= 10) return "green";
            return "purple";
        }
    }
}