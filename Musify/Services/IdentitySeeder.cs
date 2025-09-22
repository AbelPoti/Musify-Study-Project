using Microsoft.AspNetCore.Identity;
using Musify.Models;

namespace Musify.Services
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { UserRole.User, UserRole.StoreManager, UserRole.WarehouseManager, UserRole.Admin };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminUserAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, WebApplicationBuilder builder)
        {
            string username = builder.Configuration["AdminCredentials:Username"]!;
            string email = builder.Configuration["AdminCredentials:Email"]!;
            string password = builder.Configuration["AdminCredentials:Password"]!;

            const string adminRole = UserRole.Admin;

            // Ensure Admin user exists
            var adminUser = await userManager.FindByNameAsync(username);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    RegistrationTime = DateTimeOffset.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Ensure Admin user has the Admin role
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}
