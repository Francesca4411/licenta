using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StudyManagement.Models;
using StudyManagement.Services.Ai;

static string? FindDotEnvPath()
{
    var roots = new[]
    {
        Directory.GetCurrentDirectory(),
        AppContext.BaseDirectory
    };

    foreach (var root in roots)
    {
        var dir = new DirectoryInfo(root);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
    }

    return null;
}

var envPath = FindDotEnvPath();
if (!string.IsNullOrWhiteSpace(envPath))
    DotNetEnv.Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<AiTutorOptions>(builder.Configuration.GetSection("AiTutor"));
builder.Services.AddHttpClient<IAiTutorService, ClaudeTutorService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<StudyManagement.Services.ICurrentUserService, StudyManagement.Services.CurrentUserService>();
builder.Services.AddScoped<StudyManagement.Services.StudyAnalyticsService>();
builder.Services.AddScoped<StudyManagement.Services.UserProfileService>();

builder.Services.AddDbContext<StudyManagement.Data.AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<StudyManagement.Data.AppDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudyManagement.Data.AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await db.Database.MigrateAsync();
    await StudyManagement.Data.DbInitializer.SeedAsync(userManager, roleManager);
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
