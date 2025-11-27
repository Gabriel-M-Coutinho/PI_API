using LeadSearch.Models;
using Microsoft.AspNetCore.Identity;
using PI_API.models;

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
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = adminRole,
                    NormalizedName = adminRole.ToUpper()
                });
                Console.WriteLine($"[SEED] Role '{adminRole}' criada.");
            }

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrador",
                    CpfCnpj = "00000000000",
                    Tipo = "ADMIN",
                    Active = true,
                    Role = ROLE.ADMIN
                };

                var createResult = await userManager.CreateAsync(adminUser, defaultPassword);

                if (!createResult.Succeeded)
                {
                    throw new Exception($"Erro ao criar Admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRole);

                if (addToRoleResult.Succeeded)
                {
                    Console.WriteLine($"[SEED] Usuário Admin '{adminEmail}' criado e role ADMIN atribuída.");
                }
                else
                {
                    Console.WriteLine($"[SEED] Erro ao adicionar role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"[SEED] Usuário Admin '{adminEmail}' já existe.");

                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (!await userManager.IsInRoleAsync(existingAdmin, adminRole))
                {
                    await userManager.AddToRoleAsync(existingAdmin, adminRole);
                    Console.WriteLine($"[SEED] Role ADMIN atribuída ao usuário existente.");
                }
            }
        }
    }
}