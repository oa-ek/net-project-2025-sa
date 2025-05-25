using Microsoft.AspNetCore.Identity;

namespace LocalFood.Data
{
    public static class SeedRoles
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1) Створюємо чотири ролі
            string[] roles = { "Admin", "Manager", "Courier", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2) Тестовий адмін
            var adminEmail = "admin@example.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var r = await userManager.CreateAsync(admin, "Admin123!");
                if (r.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
            }

            // 3) Тестовий кур’єр
            var courierEmail = "courier@example.com";
            var courier = await userManager.FindByEmailAsync(courierEmail);
            if (courier == null)
            {
                courier = new IdentityUser { UserName = courierEmail, Email = courierEmail, EmailConfirmed = true };
                var r = await userManager.CreateAsync(courier, "Courier123!");
                if (r.Succeeded) await userManager.AddToRoleAsync(courier, "Courier");
            }

            // 4) Тестовий менеджер ресторану
            var managerEmail = "manager@example.com";
            var manager = await userManager.FindByEmailAsync(managerEmail);
            if (manager == null)
            {
                manager = new IdentityUser { UserName = managerEmail, Email = managerEmail, EmailConfirmed = true };
                var r = await userManager.CreateAsync(manager, "Manager123!");
                if (r.Succeeded) await userManager.AddToRoleAsync(manager, "Manager");
            }
        }
    }
}
