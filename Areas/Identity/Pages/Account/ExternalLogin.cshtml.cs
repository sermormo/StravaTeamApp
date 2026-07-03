using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StravaTeamApp.Models;
using StravaTeamApp.Services;

namespace StravaTeamApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly StravaClubService _stravaClubService;

    public ExternalLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        StravaClubService stravaClubService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _stravaClubService = stravaClubService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    
    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;

    public class InputModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = string.Empty;
    }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        
        // 1. Manejo de errores si el usuario cancela en la pantalla de Strava
        if (remoteError != null)
        {
            ErrorMessage = $"Error del proveedor externo: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // 2. Cargamos la información que Strava nos acaba de enviar
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error al cargar la información de Strava.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // --- VALIDACIÓN DE SEGURIDAD: 3M HEALTHY LIVING ---
        
        // 3. Extraemos el token "vivo" que Strava generó en este preciso momento
        var userAccessToken = info.AuthenticationTokens?.FirstOrDefault(t => t.Name == "access_token")?.Value;

        if (string.IsNullOrEmpty(userAccessToken))
        {
            ErrorMessage = "Error de seguridad: No se pudo obtener el token de autorización de Strava.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // 4. Validación dinámica con el ID de nuestro equipo
        long miClubId = 1252154; 

        bool esMiembro = await _stravaClubService.ValidarAtletaEnClubAsync(miClubId, userAccessToken);
        
        if (!esMiembro)
        {
            // El candado funciona: Expulsamos a quien no esté en el club de la Spirit Race
            ErrorMessage = "Acceso denegado: Tu perfil de Strava no pertenece al equipo oficial 3M Healthy Living.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
        // --------------------------------------------------

        // 5. Si es miembro, verificamos si ya tenía una cuenta registrada previamente en nuestra base de datos
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded) 
        {
            return LocalRedirect(returnUrl);
        }

        // 6. Si es un usuario nuevo, pre-llenamos el formulario final para que confirme sus datos
        ProviderDisplayName = info.ProviderDisplayName ?? "Strava";
        ReturnUrl = returnUrl;
        
        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
            
        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.GivenName))
            Input.Nombre = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
            
        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Surname))
            Input.Apellido = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return RedirectToPage("./ExternalLogin", new { ReturnUrl = returnUrl });

        if (ModelState.IsValid)
        {
            long stravaId = 0;
            long.TryParse(info.ProviderKey, out stravaId);

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                Nombre = Input.Nombre,
                Apellido = Input.Apellido,
                StravaAthleteId = stravaId // El ID seguro que nos dio Strava
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        }
        
        ProviderDisplayName = info.ProviderDisplayName ?? "Strava";
        ReturnUrl = returnUrl;
        return Page();
    }
}