using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using StudyManagement.Data;
using StudyManagement.Models;

namespace StudyManagement.Controllers;

[Authorize]
public class SubjectsController : Controller
{
    private readonly AppDbContext _db;

    public SubjectsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var subjects = await _db.Subjects
            .Include(s => s.Evaluations)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(subjects);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubject(string name, string professor, int credits, string description, string colorHex)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RedirectToAction(nameof(Index));

        var subject = new Subject
        {
            Name = name.Trim(),
            Professor = (professor ?? "").Trim(),
            Credits = credits,
            Description = (description ?? "").Trim(),
            ColorHex = string.IsNullOrWhiteSpace(colorHex) ? "#7c3aed" : colorHex.Trim()
        };

        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id);
        if (subject != null)
        {
            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvaluation(int subjectId, string title, string dateLabel, int weightPercent)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
        if (subject == null)
            return RedirectToAction(nameof(Index));

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(dateLabel))
            return RedirectToAction(nameof(Index));

        if (weightPercent < 0 || weightPercent > 100)
        {
            TempData["EvalError"] = "Weight percent must be between 0% and 100%.";
            return RedirectToAction(nameof(Index));
        }

        string formattedDate = dateLabel.Trim();
        if (DateTime.TryParse(dateLabel, out var dt))
        {
            formattedDate = dt.ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"));
        }

        var currentSum = await _db.Evaluations
            .Where(e => e.SubjectId == subjectId)
            .SumAsync(e => e.WeightPercent);

        if (currentSum + weightPercent > 100)
        {
            TempData["EvalError"] = "The total percentage for this course cannot exceed 100%";
            return RedirectToAction(nameof(Index));
        }

        var evaluation = new Evaluation
        {
            SubjectId = subjectId,
            Title = title.Trim(),
            DateLabel = formattedDate,
            WeightPercent = weightPercent
        };

        _db.Evaluations.Add(evaluation);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvaluation(int evalId)
    {
        var ev = await _db.Evaluations
            .FirstOrDefaultAsync(e => e.Id == evalId && _db.Subjects.Any(s => s.Id == e.SubjectId));
        if (ev != null)
        {
            _db.Evaluations.Remove(ev);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetEvaluation(int evalId)
    {
        var ev = await _db.Evaluations
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == evalId && _db.Subjects.Any(s => s.Id == e.SubjectId));

        if (ev == null)
            return NotFound();

        string dateIso = "";
        if (DateTime.TryParseExact(
                ev.DateLabel,
                "MMM dd, yyyy",
                CultureInfo.GetCultureInfo("en-US"),
                DateTimeStyles.None,
                out var dt))
        {
            dateIso = dt.ToString("yyyy-MM-dd");
        }

        return Json(new
        {
            id = ev.Id,
            subjectId = ev.SubjectId,
            title = ev.Title,
            dateIso,
            weightPercent = ev.WeightPercent
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvaluation(int evalId, string title, string dateLabel, int weightPercent)
    {
        var ev = await _db.Evaluations
            .FirstOrDefaultAsync(e => e.Id == evalId && _db.Subjects.Any(s => s.Id == e.SubjectId));
        if (ev == null)
            return RedirectToAction(nameof(Index));

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(dateLabel))
            return RedirectToAction(nameof(Index));

        if (weightPercent < 0 || weightPercent > 100)
        {
            TempData["EvalError"] = "Weight percent must be between 0% and 100%.";
            return RedirectToAction(nameof(Index));
        }

        string formattedDate;
        if (DateTime.TryParseExact(dateLabel.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            formattedDate = dt.ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"));
        }
        else if (DateTime.TryParse(dateLabel, out dt))
        {
            formattedDate = dt.ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"));
        }
        else
        {
            formattedDate = ev.DateLabel;
        }

        var currentSumOther = await _db.Evaluations
            .Where(e => e.SubjectId == ev.SubjectId && e.Id != evalId)
            .SumAsync(e => e.WeightPercent);

        if (currentSumOther + weightPercent > 100)
        {
            TempData["EvalError"] = "The total percentage for this course cannot exceed 100%.";
            return RedirectToAction(nameof(Index));
        }

        ev.Title = title.Trim();
        ev.DateLabel = formattedDate;
        ev.WeightPercent = weightPercent;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditSubject(int id)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id);
        if (subject == null) return RedirectToAction(nameof(Index));
        return View(subject);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubject(int id, string name, string professor, int credits, string description, string colorHex)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id);
        if (subject == null) return RedirectToAction(nameof(Index));

        subject.Name = (name ?? "").Trim();
        subject.Professor = (professor ?? "").Trim();
        subject.Credits = credits;
        subject.Description = (description ?? "").Trim();
        subject.ColorHex = string.IsNullOrWhiteSpace(colorHex) ? subject.ColorHex : colorHex.Trim();

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
