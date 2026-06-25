using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyManagement.Data;
using StudyManagement.Models;
using StudyManagement.Services;

namespace StudyManagement.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly StudyAnalyticsService _analytics;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserProfileService _profiles;

    public ProfileController(
        AppDbContext db,
        IWebHostEnvironment env,
        StudyAnalyticsService analytics,
        UserManager<ApplicationUser> userManager,
        UserProfileService profiles)
    {
        _db = db;
        _env = env;
        _analytics = analytics;
        _userManager = userManager;
        _profiles = profiles;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var profile = await _profiles.GetOrCreateAsync(user.Id, user.Email);
        return View(await _analytics.BuildProfilePageViewModelAsync(profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index([Bind(Prefix = "Profile")] UserProfile model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var existing = await _profiles.GetTrackedAsync(user.Id);
        if (existing == null || existing.Id != model.Id)
            return NotFound();

        model.FullName = (model.FullName ?? "").Trim();
        model.Email = (user.Email ?? model.Email ?? "").Trim();
        model.Faculty = (model.Faculty ?? "").Trim();
        model.AcademicYear = (model.AcademicYear ?? "").Trim();
        model.AvatarPhotoPath = existing.AvatarPhotoPath;

        ModelState.Clear();
        TryValidateModel(model);
        if (!ModelState.IsValid)
            return View(await _analytics.BuildProfilePageViewModelAsync(model));

        model.WeeklyGoalHours = Math.Clamp(model.WeeklyGoalHours, 1, 80);

        existing.FullName = model.FullName;
        existing.Email = model.Email;
        existing.Faculty = model.Faculty;
        existing.AcademicYear = model.AcademicYear;
        existing.WeeklyGoalHours = model.WeeklyGoalHours;

        await _db.SaveChangesAsync();
        TempData["ProfileSaved"] = "Your profile has been saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(2_500_000)]
    public async Task<IActionResult> UploadAvatar(IFormFile? file)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (file == null || file.Length == 0)
        {
            TempData["ProfileError"] = "Choose an image file.";
            return RedirectToAction(nameof(Index));
        }

        if (file.Length > 2_000_000)
        {
            TempData["ProfileError"] = "Image must be under 2 MB.";
            return RedirectToAction(nameof(Index));
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
        {
            TempData["ProfileError"] = "Use JPG, PNG, or WebP.";
            return RedirectToAction(nameof(Index));
        }

        var profile = await _profiles.GetTrackedAsync(user.Id);
        if (profile == null)
        {
            await _profiles.GetOrCreateAsync(user.Id, user.Email);
            profile = await _profiles.GetTrackedAsync(user.Id);
        }

        if (profile == null)
            return NotFound();

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "profile");
        Directory.CreateDirectory(uploadDir);
        var safeName = $"avatar-{profile.Id}{ext}";
        var fullPath = Path.Combine(uploadDir, safeName);
        await using (var fs = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(fs);

        profile.AvatarPhotoPath = "/uploads/profile/" + safeName;
        await _db.SaveChangesAsync();
        TempData["ProfileSaved"] = "Profile photo updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAvatar()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var profile = await _profiles.GetTrackedAsync(user.Id);
        if (profile?.AvatarPhotoPath != null)
        {
            var rel = profile.AvatarPhotoPath.TrimStart('/');
            var physical = Path.Combine(_env.WebRootPath, rel.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(physical))
                System.IO.File.Delete(physical);

            profile.AvatarPhotoPath = null;
            await _db.SaveChangesAsync();
        }

        TempData["ProfileSaved"] = "Profile photo removed.";
        return RedirectToAction(nameof(Index));
    }
}
