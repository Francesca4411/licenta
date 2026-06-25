namespace StudyManagement.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
}
