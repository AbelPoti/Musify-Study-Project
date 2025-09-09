using Microsoft.AspNetCore.Identity;

namespace Musify.Services
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "User", "StoreManager", "WarehouseManager", "Admin" };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminUserAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, WebApplicationBuilder builder)
        {
            string username = builder.Configuration["AdminCredentials:Username"]!;
            string email = builder.Configuration["AdminCredentials:Email"]!;
            string password = builder.Configuration["AdminCredentials:Password"]!;

            // Ensure Admin user exists
            var adminUser = await userManager.FindByNameAsync(username);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
