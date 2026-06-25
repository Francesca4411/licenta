using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyManagement.Models;
using StudyManagement.Models.ViewModels.Admin;

namespace StudyManagement.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var items = new List<AdminUserItemViewModel>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new AdminUserItemViewModel
            {
                Email = user.Email ?? user.UserName ?? "",
                Roles = roles.ToList(),
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        return View(new AdminUsersPageViewModel
        {
            Users = items,
            TotalCount = items.Count
        });
    }
}
