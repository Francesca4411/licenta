using Microsoft.AspNetCore.Identity;

using StudyManagement.Models;



namespace StudyManagement.Data

{

    public static class DbInitializer

    {

        private const string AdminRoleName = "Admin";



        public static async Task SeedAsync(

            UserManager<ApplicationUser> userManager,

            RoleManager<IdentityRole> roleManager)

        {

            await EnsureAdminRoleAssignedAsync(userManager, roleManager);

        }



        public static async Task<bool> EnsureAdminRoleAssignedAsync(

            UserManager<ApplicationUser> userManager,

            RoleManager<IdentityRole> roleManager,

            ApplicationUser? user = null)

        {

            if (!await roleManager.RoleExistsAsync(AdminRoleName))

                await roleManager.CreateAsync(new IdentityRole(AdminRoleName));



            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")?.Trim();

            if (string.IsNullOrWhiteSpace(adminEmail))

                return false;



            user ??= await userManager.FindByEmailAsync(adminEmail);

            if (user == null || !string.Equals(user.Email, adminEmail, StringComparison.OrdinalIgnoreCase))

                return false;



            if (await userManager.IsInRoleAsync(user, AdminRoleName))

                return false;



            await userManager.AddToRoleAsync(user, AdminRoleName);

            return true;

        }

    }

}


