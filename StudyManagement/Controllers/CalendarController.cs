using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using StudyManagement.Data;
using StudyManagement.Helpers;
using StudyManagement.Models;
using StudyManagement.Models.ViewModels;

namespace StudyManagement.Controllers;

[Authorize]
public class CalendarController : Controller
{
    private readonly AppDbContext _db;

    public CalendarController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? reference)
    {
        DateTime refDate = DateTime.Today;
        if (!string.IsNullOrWhiteSpace(reference))
        {
            if (DateTime.TryParseExact(reference.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                refDate = parsed;
        }

        int diff = (7 + (int)refDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        var weekStart = refDate.Date.AddDays(-diff);
        var weekEnd = weekStart.AddDays(7);

        ViewBag.SubjectOptions = await _db.Subjects
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();

        var sessions = await _db.StudySessions
            .Include(s => s.Subject)
            .Where(s => s.StartAt >= weekStart && s.StartAt < weekEnd)
            .ToListAsync();

        var vm = new CalendarViewModel
        {
            WeekStart = weekStart,
            WeekLabel = $"{weekStart.ToString("MMM dd", CultureInfo.GetCultureInfo("en-US"))} - {weekStart.AddDays(6).ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"))}"
        };

        const int hourHeight = 56;
        int totalHours = vm.HourEndExclusive - vm.HourStart;
        double totalHeightPx = totalHours * hourHeight;

        var dayToSessions = new Dictionary<int, List<TmpSession>>();

        for (int i = 0; i < 7; i++)
            dayToSessions[i] = new List<TmpSession>();

        foreach (var s in sessions)
        {
            var dayIndex = (int)(s.StartAt.Date - weekStart).TotalDays;
            if (dayIndex < 0 || dayIndex > 6) continue;

            var minutesFromHourStart = (s.StartAt.Hour - vm.HourStart) * 60 + s.StartAt.Minute;
            var topPx = (minutesFromHourStart / 60.0) * hourHeight;
            var heightPx = (s.DurationMinutes / 60.0) * hourHeight;

            if (topPx + heightPx < 0) continue;
            topPx = Math.Max(topPx, 0);
            heightPx = Math.Min(heightPx, totalHeightPx - topPx);
            if (heightPx <= 4) continue;

            dayToSessions[dayIndex].Add(new TmpSession
            {
                Id = s.Id,
                DayIndex = dayIndex,
                StartAt = s.StartAt,
                EndAt = s.StartAt.AddMinutes(s.DurationMinutes),
                TopPx = topPx,
                HeightPx = heightPx,
                SubjectName = s.Subject?.Name ?? "",
                SubjectColorHex = s.Subject?.ColorHex ?? "#7c3aed",
                SubjectColorBg = StudyUiFormat.HexToRgba(s.Subject?.ColorHex ?? "#7c3aed", 0.18),
                ActivityLabel = s.ActivityLabel,
                DurationMinutes = s.DurationMinutes
            });
        }

        foreach (var kv in dayToSessions)
        {
            var list = kv.Value;
            if (list.Count == 0) continue;

            list = list
                .OrderBy(x => x.StartAt)
                .ThenBy(x => x.EndAt)
                .ToList();

            var colEnds = new List<DateTime>();
            int maxCols = 1;

            foreach (var item in list)
            {
                int colIndex = -1;
                for (int i = 0; i < colEnds.Count; i++)
                {
                    if (item.StartAt >= colEnds[i])
                    {
                        colIndex = i;
                        break;
                    }
                }

                if (colIndex == -1)
                {
                    colIndex = colEnds.Count;
                    colEnds.Add(item.EndAt);
                }
                else
                {
                    colEnds[colIndex] = item.EndAt;
                }

                item.ColumnIndex = colIndex;
                maxCols = Math.Max(maxCols, colEnds.Count);
            }

            var widthPct = 100.0 / maxCols;

            foreach (var item in list)
            {
                vm.Sessions.Add(new CalendarSessionDto
                {
                    Id = item.Id,
                    DayIndex = item.DayIndex,
                    TopPx = item.TopPx,
                    HeightPx = item.HeightPx,
                    LeftPct = item.ColumnIndex * widthPct,
                    WidthPct = widthPct,
                    SubjectName = item.SubjectName,
                    SubjectColorHex = item.SubjectColorHex,
                    SubjectColorBg = item.SubjectColorBg,
                    ActivityLabel = item.ActivityLabel,
                    DurationMinutes = item.DurationMinutes
                });
            }
        }

        vm.Sessions = vm.Sessions
            .OrderBy(x => x.DayIndex)
            .ThenBy(x => x.TopPx)
            .ToList();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudySession(
        int subjectId,
        string reference,
        string startDate,
        string startTime,
        int durationMinutes,
        string activityLabel)
    {
        if (subjectId <= 0)
            return RedirectToAction(nameof(Index), new { reference });

        if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(startTime))
            return RedirectToAction(nameof(Index), new { reference });

        if (durationMinutes <= 0 || durationMinutes > 600)
            return RedirectToAction(nameof(Index), new { reference });

        var combined = $"{startDate.Trim()} {startTime.Trim()}";
        if (!DateTime.TryParseExact(combined, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startAt))
            return RedirectToAction(nameof(Index), new { reference });

        var label = (activityLabel ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(label))
            label = "review";

        var subjectExists = await _db.Subjects.AnyAsync(s => s.Id == subjectId);
        if (!subjectExists)
            return RedirectToAction(nameof(Index), new { reference });

        _db.StudySessions.Add(new StudySession
        {
            SubjectId = subjectId,
            StartAt = startAt,
            DurationMinutes = durationMinutes,
            ActivityLabel = label
        });

        await _db.SaveChangesAsync();

        var refValue = string.IsNullOrWhiteSpace(reference)
            ? startAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : reference.Trim();

        return RedirectToAction(nameof(Index), new { reference = refValue });
    }

    [HttpGet]
    public async Task<IActionResult> GetStudySession(int sessionId)
    {
        var s = await _db.StudySessions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s == null) return NotFound();

        return Json(new
        {
            id = s.Id,
            subjectId = s.SubjectId,
            startDateIso = s.StartAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            startTime = s.StartAt.ToString("HH:mm", CultureInfo.InvariantCulture),
            durationMinutes = s.DurationMinutes,
            activityLabel = s.ActivityLabel
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStudySession(
        int sessionId,
        int subjectId,
        string startDate,
        string startTime,
        int durationMinutes,
        string activityLabel)
    {
        var s = await _db.StudySessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s == null)
            return RedirectToAction(nameof(Index));

        if (subjectId <= 0)
            return RedirectToAction(nameof(Index), new { reference = StudyUiFormat.GetWeekStartMonday(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });

        if (durationMinutes <= 0 || durationMinutes > 600)
            return RedirectToAction(nameof(Index), new { reference = StudyUiFormat.GetWeekStartMonday(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });

        var combined = $"{startDate.Trim()} {startTime.Trim()}";
        if (!DateTime.TryParseExact(combined, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startAt))
            return RedirectToAction(nameof(Index), new { reference = StudyUiFormat.GetWeekStartMonday(DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });

        s.SubjectId = subjectId;
        s.StartAt = startAt;
        s.DurationMinutes = durationMinutes;
        s.ActivityLabel = string.IsNullOrWhiteSpace(activityLabel) ? "review" : activityLabel.Trim().ToLowerInvariant();

        await _db.SaveChangesAsync();

        var weekStart = StudyUiFormat.GetWeekStartMonday(startAt);
        return RedirectToAction(nameof(Index), new { reference = weekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudySession(int sessionId)
    {
        var s = await _db.StudySessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s == null)
            return RedirectToAction(nameof(Index));

        var weekStart = StudyUiFormat.GetWeekStartMonday(s.StartAt);
        _db.StudySessions.Remove(s);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { reference = weekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
    }

    private sealed class TmpSession
    {
        public int Id { get; set; }
        public int DayIndex { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public double TopPx { get; set; }
        public double HeightPx { get; set; }

        public int ColumnIndex { get; set; }

        public string SubjectName { get; set; } = "";
        public string SubjectColorHex { get; set; } = "#7c3aed";
        public string SubjectColorBg { get; set; } = "rgba(124,58,237,0.18)";
        public string ActivityLabel { get; set; } = "review";
        public int DurationMinutes { get; set; }
    }
}
