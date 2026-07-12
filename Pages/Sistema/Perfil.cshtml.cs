using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using StravaTeamApp.Data;
using StravaTeamApp.Models;

namespace StravaTeamApp.Pages.Sistema;

[Authorize]
public class PerfilModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public PerfilModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string EmailUsuario { get; set; } = string.Empty;

    public bool EstaConectadoAStrava { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "El Nombre no puede quedar en blanco.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Apellido no puede quedar en blanco.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes seleccionar un Género.")]
        public string Genero { get; set; } = string.Empty;

        [Required(ErrorMessage = "El UPIN es obligatorio.")]
        public string UPIN { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

        EmailUsuario = user.Email ?? "No disponible";

        Input = new InputModel
        {
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Genero = user.Genero,
            UPIN = user.UPIN
        };

        var logins = await _userManager.GetLoginsAsync(user);
        EstaConectadoAStrava = logins.Any(l => l.LoginProvider == "Strava");

        return Page();
    }

    public async Task<IActionResult> OnPostGuardarPerfilAsync()
    {
        if (!ModelState.IsValid) 
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                EmailUsuario = currentUser.Email ?? "No disponible";
                var currentLogins = await _userManager.GetLoginsAsync(currentUser);
                EstaConectadoAStrava = currentLogins.Any(l => l.LoginProvider == "Strava");
            }
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.Nombre = Input.Nombre;
        user.Apellido = Input.Apellido;
        user.Genero = Input.Genero;
        user.UPIN = Input.UPIN;

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRevocarStravaAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Eliminamos al usuario por completo de la base de datos
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            // Cerramos la sesión en el navegador
            await _signInManager.SignOutAsync();
            
            // Redirigimos a la página principal
            return RedirectToPage("/Index");
        }

        return RedirectToPage();
    }
}