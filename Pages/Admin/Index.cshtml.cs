using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
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

    public string UsuarioActualId { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        UsuarioActualId = _userManager.GetUserId(User) ?? string.Empty;
        var usuariosRegistrados = await _userManager.Users
            .OrderBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Apellido)
            .ToListAsync();

        foreach (var usuario in usuariosRegistrados)
        {
            var roles = await _userManager.GetRolesAsync(usuario);

            Usuarios.Add(new UsuarioAdminViewModel
            {
                Id = usuario.Id,
                NombreCompleto =
                    $"{usuario.Nombre} {usuario.Apellido}".Trim(),

                Email = usuario.Email ?? string.Empty,
                UPIN = usuario.UPIN,

                Roles = roles.Count > 0
                    ? string.Join(", ", roles)
                    : "Sin rol",
                EsAdministrador = roles.Contains("Administrator")
            });
        }
    }

    public async Task<IActionResult> OnPostPromoverAsync(string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return BadRequest();
        }

        var usuario = await _userManager.FindByIdAsync(usuarioId);

        if (usuario is null)
        {
            return NotFound();
        }

        if (!await _userManager.IsInRoleAsync(
                usuario,
                "Administrator"))
        {
            var resultado = await _userManager.AddToRoleAsync(
                usuario,
                "Administrator");

            if (!resultado.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "No fue posible asignar el rol de administrador.";

                return RedirectToPage();
            }
        }

        TempData["SuccessMessage"] =
            "El usuario fue promovido a administrador.";

        return RedirectToPage();
    }

  public async Task<IActionResult> OnPostQuitarAdministradorAsync(string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return BadRequest();
        }

        var usuarioActualId = _userManager.GetUserId(User);

        if (usuarioId == usuarioActualId)
        {
            TempData["ErrorMessage"] =
                "No puedes quitarte tu propio acceso administrativo.";

            return RedirectToPage();
        }

        var usuario = await _userManager.FindByIdAsync(usuarioId);

        if (usuario is null)
        {
            return NotFound();
        }

        if (await _userManager.IsInRoleAsync(
                usuario,
                "Administrator"))
        {
            var resultado = await _userManager.RemoveFromRoleAsync(
                usuario,
                "Administrator");

            if (!resultado.Succeeded)
            {
                TempData["ErrorMessage"] =
                    "No fue posible quitar el rol de administrador.";

                return RedirectToPage();
            }
        }

        TempData["SuccessMessage"] =
            "El permiso de administrador fue eliminado.";

        return RedirectToPage();
    }
    public sealed class UsuarioAdminViewModel
    {
        public string Id { get; init; } = string.Empty;
        public bool EsAdministrador { get; init; }
        public string NombreCompleto { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string UPIN { get; init; } = string.Empty;

        public string Roles { get; init; } = string.Empty;
    }
}