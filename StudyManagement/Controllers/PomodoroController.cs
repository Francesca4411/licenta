using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyManagement.Data;
using StudyManagement.Models;
using StudyManagement.Services;

namespace StudyManagement.Controllers;

[Authorize]
public class PomodoroController : Controller
{
    private readonly AppDbContext _db;
    private readonly StudyAnalyticsService _analytics;

    public PomodoroController(AppDbContext db, StudyAnalyticsService analytics)
    {
        _db = db;
        _analytics = analytics;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _analytics.GetPomodoroStatsAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPomodoroCompletion([FromForm] int durationMinutes = 25)
    {
        if (durationMinutes < 1 || durationMinutes > 120)
            durationMinutes = 25;

        _db.PomodoroSessions.Add(new PomodoroSession
        {
            CompletedAtUtc = DateTime.UtcNow,
            DurationMinutes = durationMinutes
        });
        await _db.SaveChangesAsync();

        return Json(await _analytics.GetPomodoroStatsAsync());
    }
}
