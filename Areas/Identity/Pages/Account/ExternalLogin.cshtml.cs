using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StravaTeamApp.Services;
using StravaTeamApp.Models;

namespace StravaTeamApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly StravaAthleteService _athleteService;

        public ExternalLoginModel(
          SignInManager<ApplicationUser> signInManager,
          UserManager<ApplicationUser> userManager,
          IUserStore<ApplicationUser> userStore,
          ILogger<ExternalLoginModel> logger,
          StravaAthleteService athleteService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _logger = logger;
            _athleteService = athleteService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string ProviderDisplayName { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public class InputModel
    {
        [Required(ErrorMessage = "El Correo Electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Género es obligatorio.")]
        public string Genero { get; set; } = string.Empty;

        [Required(ErrorMessage = "El UPIN es obligatorio.")]
        public string UPIN { get; set; } = string.Empty;
    }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error del proveedor externo: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error al cargar la información externa.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // --- INICIO DE LA BARRERA DE SEGURIDAD DEL EQUIPO ---
            var accessToken = info.AuthenticationTokens?.FirstOrDefault(t => t.Name == "access_token")?.Value;

            if (!string.IsNullOrEmpty(accessToken))
            {
                long equipoClubId = 1252154; // ID oficial del equipo
                bool esMiembro = await _athleteService.EsMiembroDelClubAsync(accessToken, equipoClubId);

                if (!esMiembro)
                {
                    ErrorMessage = "Acceso denegado: Solo miembros del equipo oficial pueden ingresar a la plataforma.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
            }
            // --- FIN DE LA BARRERA DE SEGURIDAD ---

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user != null && info.AuthenticationTokens != null)
                {
                    foreach (var prop in info.AuthenticationTokens)
                    {
                        await _userManager.SetAuthenticationTokenAsync(user, info.LoginProvider, prop.Name, prop.Value);
                    }
                }

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                Input = new InputModel
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty
                };
            }
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error al cargar la información externa durante la confirmación.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                
                // --- ASIGNACIÓN DE LOS DATOS AL USUARIO ---
                user.Nombre = Input.Nombre;
                user.Apellido = Input.Apellido;
                user.Genero = Input.Genero; // Nuevo
                user.UPIN = Input.UPIN;     // Nuevo

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        var props = info.AuthenticationTokens;
                        if (props != null)
                        {
                            foreach (var prop in props)
                            {
                                await _userManager.SetAuthenticationTokenAsync(user, info.LoginProvider, prop.Name, prop.Value);
                            }
                        }

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"No se puede crear una instancia de '{nameof(ApplicationUser)}'.");
            }
        }
    }
}