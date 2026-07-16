using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StravaTeamApp.Models;

namespace StravaTeamApp.Pages.Admin;

[Authorize(Roles = "Administrator")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UsuarioAdminViewModel> Usuarios { get; private set; }
        = new List<UsuarioAdminViewModel>();

    public async Task OnGetAsync()
    {
        var usuariosRegistrados = await _userManager.Users
            .OrderBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Apellido)
            .ToListAsync();

        foreach (var usuario in usuariosRegistrados)
        {
            var roles = await _userManager.GetRolesAsync(usuario);

            Usuarios.Add(new UsuarioAdminViewModel
            {
                NombreCompleto =
                    $"{usuario.Nombre} {usuario.Apellido}".Trim(),

                Email = usuario.Email ?? string.Empty,
                UPIN = usuario.UPIN,

                Roles = roles.Count > 0
                    ? string.Join(", ", roles)
                    : "Sin rol"
            });
        }
    }

    public sealed class UsuarioAdminViewModel
    {
        public string NombreCompleto { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string UPIN { get; init; } = string.Empty;

        public string Roles { get; init; } = string.Empty;
    }
}