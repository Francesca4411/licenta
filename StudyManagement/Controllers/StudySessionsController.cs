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
public class StudySessionsController : Controller
{
    private const int MaxStudySessionsPerTab = 10;

    private readonly AppDbContext _db;

    public StudySessionsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string tab = "planned")
    {
        tab = (tab ?? "planned").Trim().ToLowerInvariant();
        if (tab != "planned" && tab != "completed") tab = "planned";

        await PruneExcessCompletedStudySessionsAsync(MaxStudySessionsPerTab);

        var subjectOptions = await _db.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SubjectChoiceDto
            {
                Id = s.Id,
                Name = s.Name,
                ColorHex = s.ColorHex
            })
            .ToListAsync();

        var baseQuery = _db.StudySessions
            .Include(s => s.Subject)
            .AsNoTracking()
            .AsQueryable();

        baseQuery = tab == "completed"
            ? baseQuery.Where(s => s.IsCompleted)
            : baseQuery.Where(s => !s.IsCompleted);

        List<StudySession> sessions;
        if (tab == "completed")
        {
            sessions = await baseQuery
                .OrderByDescending(s => s.ActualStartAt ?? s.StartAt)
                .ThenByDescending(s => s.Id)
                .Take(MaxStudySessionsPerTab)
                .ToListAsync();
        }
        else
        {
            sessions = await baseQuery
                .OrderByDescending(s => s.StartAt)
                .ThenByDescending(s => s.Id)
                .Take(MaxStudySessionsPerTab)
                .ToListAsync();
        }

        var vm = new StudySessionsPageViewModel
        {
            Tab = tab,
            SubjectOptions = subjectOptions
        };

        foreach (var s in sessions)
        {
            var subjectColorHex = s.Subject?.ColorHex ?? "#7c3aed";

            vm.Sessions.Add(new StudySessionRowDto
            {
                Id = s.Id,
                SubjectId = s.SubjectId,
                SubjectName = s.Subject?.Name ?? "",
                SubjectColorHex = subjectColorHex,
                SubjectColorBg = StudyUiFormat.HexToRgba(subjectColorHex, 0.18),
                StartAt = s.StartAt,
                DurationMinutes = s.DurationMinutes,
                ActivityLabel = s.ActivityLabel,
                IsCompleted = s.IsCompleted,
                ActualDurationMinutes = s.ActualDurationMinutes,
                ActualStartAt = s.ActualStartAt,
                DifficultyLabel = s.DifficultyLabel,
                Notes = s.Notes
            });
        }

        return View(vm);
    }

    private async Task PruneExcessCompletedStudySessionsAsync(int maxKeep)
    {
        var totalCompleted = await _db.StudySessions.CountAsync(s => s.IsCompleted);
        if (totalCompleted <= maxKeep)
            return;

        var keepIds = await _db.StudySessions.AsNoTracking()
            .Where(s => s.IsCompleted)
            .OrderByDescending(s => s.ActualStartAt ?? s.StartAt)
            .ThenByDescending(s => s.Id)
            .Select(s => s.Id)
            .Take(maxKeep)
            .ToListAsync();

        var toRemove = await _db.StudySessions
            .Where(s => s.IsCompleted && !keepIds.Contains(s.Id))
            .ToListAsync();

        if (toRemove.Count == 0)
            return;

        _db.StudySessions.RemoveRange(toRemove);
        await _db.SaveChangesAsync();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudySessionForStudySessions(
        int subjectId,
        string startDate,
        string startTime,
        int durationMinutes,
        string activityLabel)
    {
        if (subjectId <= 0)
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(startTime))
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        if (durationMinutes <= 0 || durationMinutes > 600)
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        var combined = $"{startDate.Trim()} {startTime.Trim()}";
        if (!DateTime.TryParseExact(combined, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startAt))
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        var label = (activityLabel ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(label))
            label = "review";

        var subjectExists = await _db.Subjects.AnyAsync(s => s.Id == subjectId);
        if (!subjectExists)
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        _db.StudySessions.Add(new StudySession
        {
            SubjectId = subjectId,
            StartAt = startAt,
            DurationMinutes = durationMinutes,
            ActivityLabel = label,
            IsCompleted = false
        });

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { tab = "planned" });
    }

    [HttpGet]
    public async Task<IActionResult> GetStudySessionForStudySessions(int sessionId)
    {
        var s = await _db.StudySessions
            .AsNoTracking()
            .Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (s == null) return NotFound();

        return Json(new
        {
            id = s.Id,
            subjectId = s.SubjectId,
            startDateIso = s.StartAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            startTime = s.StartAt.ToString("HH:mm", CultureInfo.InvariantCulture),
            durationMinutes = s.DurationMinutes,
            activityLabel = s.ActivityLabel,
            isCompleted = s.IsCompleted,
            actualDurationMinutes = s.ActualDurationMinutes,
            actualStartDateIso = s.ActualStartAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            actualStartTime = s.ActualStartAt?.ToString("HH:mm", CultureInfo.InvariantCulture),
            difficultyLabel = s.DifficultyLabel,
            notes = s.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStudySessionForStudySessions(
        int sessionId,
        int subjectId,
        string startDate,
        string startTime,
        int durationMinutes,
        string activityLabel)
    {
        var s = await _db.StudySessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s == null) return RedirectToAction(nameof(Index), new { tab = "planned" });

        if (s.IsCompleted)
            return RedirectToAction(nameof(Index), new { tab = "completed" });

        if (subjectId <= 0)
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(startTime))
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        if (durationMinutes <= 0 || durationMinutes > 600)
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        var combined = $"{startDate.Trim()} {startTime.Trim()}";
        if (!DateTime.TryParseExact(combined, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startAt))
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        var label = (activityLabel ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(label))
            label = "review";

        s.SubjectId = subjectId;
        s.StartAt = startAt;
        s.DurationMinutes = durationMinutes;
        s.ActivityLabel = label;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { tab = "planned" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteStudySessionForStudySessions(
        int sessionId,
        string actualDate,
        string actualTime,
        int actualDurationMinutes,
        string difficultyLabel,
        string? notes)
    {
        var s = await _db.StudySessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s == null) return RedirectToAction(nameof(Index), new { tab = "completed" });

        if (s.IsCompleted)
            return RedirectToAction(nameof(Index), new { tab = "completed" });

        if (string.IsNullOrWhiteSpace(actualDate) || string.IsNullOrWhiteSpace(actualTime))
            return RedirectToAction(nameof(Index), new { tab = "completed" });

        if (actualDurationMinutes <= 0 || actualDurationMinutes > 600)
            return RedirectToAction(nameof(Index), new { tab = "planned" });

        difficultyLabel = (difficultyLabel ?? "").Trim().ToLowerInvariant();
        if (difficultyLabel != "low" && difficultyLabel != "medium" && difficultyLabel != "high")
            difficultyLabel = "medium";

        var combined = $"{actualDate.Trim()} {actualTime.Trim()}";
        if (!DateTime.TryParseExact(combined, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var actualStartAt))
            return RedirectToAction(nameof(Index), new { tab = "completed" });

        s.IsCompleted = true;
        s.ActualDurationMinutes = actualDurationMinutes;
        s.ActualStartAt = actualStartAt;
        s.DifficultyLabel = difficultyLabel;
        s.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        await _db.SaveChangesAsync();

        await PruneExcessCompletedStudySessionsAsync(MaxStudySessionsPerTab);

        return RedirectToAction(nameof(Index), new { tab = "completed" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudySessionForStudySessions(int sessionId)
    {
        var s = await _db.StudySessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (s == null) return RedirectToAction(nameof(Index), new { tab = "planned" });

        if (s.IsCompleted)
            return RedirectToAction(nameof(Index), new { tab = "completed" });

        _db.StudySessions.Remove(s);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { tab = "planned" });
    }
}
