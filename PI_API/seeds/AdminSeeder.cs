using LeadSearch.Models;
using Microsoft.AspNetCore.Identity;

namespace PI_API.seeds
{
    public static class AdminSeeder
    {
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider, string defaultPassword)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            const string adminRole = "ADMIN";
            const string adminEmail = "admin@leadsearch.com";

            if (await roleManager.FindByNameAsync(adminRole) == null)
            {
                var result = await roleManager.CreateAsync(new ApplicationRole { Name = adminRole });
                if (result.Succeeded)
                {
                    Console.WriteLine($"[SEED] Role '{adminRole}' criada com sucesso.");
                }
            }
            else
            {
                Console.WriteLine($"[SEED] Role '{adminRole}' já existe.");
            }

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrador"
                };

                IdentityResult createResult = await userManager.CreateAsync(adminUser, defaultPassword);

                if (!createResult.Succeeded) throw new Exception($"Erro ao criar Admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

                var createdAdmin = await userManager.FindByEmailAsync(adminEmail);
                await userManager.AddToRoleAsync(createdAdmin, adminRole);

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                    Console.WriteLine($"[SEED] Usuário Admin '{adminEmail}' criado e role atribuída.");
                }
            }
            else
            {
                Console.WriteLine($"[SEED] Usuário Admin '{adminEmail}' já existe.");
            }
        }
    }
}
