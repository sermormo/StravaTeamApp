using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using StravaTeamApp.Models;

namespace StravaTeamApp.Data.Seed;

public static class IdentitySeeder
{
    private static readonly string[] Roles =
    [
        "Administrator",
        "Member"
    ];

    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in Roles)
        {
            var roleExists =
                await roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                var result =
                    await roleManager.CreateAsync(
                        new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"No se pudo crear el rol {roleName}: {errors}");
                }
            }
        }
    }

    public static async Task SeedAdminAsync(
    IServiceProvider services,
    IConfiguration configuration)
    {
        var email = configuration["InitialAdmin:Email"];
        var password = configuration["InitialAdmin:Password"];
        var upin = configuration["InitialAdmin:UPIN"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(upin))
        {
            return;
        }

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nombre = "Administrador",
                Apellido = "Sistema",
                Genero = "No aplica",
                UPIN = upin
            };

            var createResult =
                await userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"No se pudo crear el administrador: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
            user,
            "Administrator"))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    user,
                    "Administrator");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"No se pudo asignar Administrator: {errors}");
            }
        }
    }
}