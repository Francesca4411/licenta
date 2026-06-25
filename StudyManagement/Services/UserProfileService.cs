using Microsoft.EntityFrameworkCore;
using StudyManagement.Data;
using StudyManagement.Models;

namespace StudyManagement.Services;

public class UserProfileService
{
    private readonly AppDbContext _db;

    public UserProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserProfile> GetOrCreateAsync(string userId, string? email = null)
    {
        var profile = await _db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

        if (profile != null)
            return profile;

        profile = new UserProfile
        {
            IdentityUserId = userId,
            Email = email?.Trim() ?? "",
            FullName = "",
            WeeklyGoalHours = 12
        };

        _db.UserProfiles.Add(profile);
        await _db.SaveChangesAsync();
        return profile;
    }

    public Task<UserProfile?> GetTrackedAsync(string userId) =>
        _db.UserProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

    public static string GetDisplayName(UserProfile profile)
    {
        if (!string.IsNullOrWhiteSpace(profile.FullName))
            return profile.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(profile.Email))
        {
            var at = profile.Email.IndexOf('@');
            if (at > 0)
                return profile.Email[..at];
        }

        return "Student";
    }
}
