using Microsoft.AspNetCore.Identity;

namespace LocalFood.Data
{
    public static class SeedRoles
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "User", "Courier" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Адміністратор
            var adminEmail = "admin@example.com";
            var adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Кур'єр
            var courierEmail = "courier@example.com";
            var courierPassword = "Courier123!";

            var courierUser = await userManager.FindByEmailAsync(courierEmail);
            if (courierUser == null)
            {
                courierUser = new IdentityUser
                {
                    UserName = courierEmail,
                    Email = courierEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(courierUser, courierPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(courierUser, "Courier");
            }
        }
    }
}
