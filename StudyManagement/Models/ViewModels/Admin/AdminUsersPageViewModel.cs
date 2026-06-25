namespace StudyManagement.Models.ViewModels.Admin;

public sealed class AdminUsersPageViewModel
{
    public IReadOnlyList<AdminUserItemViewModel> Users { get; init; } = Array.Empty<AdminUserItemViewModel>();
    public int TotalCount { get; init; }
}

public sealed class AdminUserItemViewModel
{
    public string Email { get; init; } = "";
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public bool EmailConfirmed { get; init; }
    public bool IsLockedOut { get; init; }
}
