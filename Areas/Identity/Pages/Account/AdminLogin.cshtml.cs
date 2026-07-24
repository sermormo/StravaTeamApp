using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StravaTeamApp.Models;
using StravaTeamApp.Services;

namespace StravaTeamApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class AdminLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISystemLogService _systemLogService;

    public AdminLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ISystemLogService systemLogService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _systemLogService = systemLogService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = "/";

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null ||
            !await _userManager.IsInRoleAsync(user, "Administrator"))
        {
            await _systemLogService.WarningAsync(
                category:
                    SystemLogCategories.Authentication,
                eventName: "AdminLoginRejected",
                message:
                    "Se rechazó un intento de acceso administrativo.",
                details:
                    "Reason=UnknownUserOrMissingAdministratorRole");

            ModelState.AddModelError(
                string.Empty,
                "Correo o contraseña incorrectos.");

            ReturnUrl = returnUrl;
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await _systemLogService.InformationAsync(
                category:
                    SystemLogCategories.Authentication,
                eventName: "AdminLoginSucceeded",
                message:
                    "Un administrador inició sesión correctamente.",
                actorUserId: user.Id);

            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            await _systemLogService.WarningAsync(
                category:
                    SystemLogCategories.Authentication,
                eventName: "AdminAccountLocked",
                message:
                    "La cuenta administrativa fue bloqueada temporalmente.",
                details:
                    "Reason=RepeatedFailedLoginAttempts",
                actorUserId: user.Id);

            ModelState.AddModelError(
                string.Empty,
                "La cuenta está temporalmente bloqueada.");
        }
        else
        {
            await _systemLogService.WarningAsync(
                category:
                    SystemLogCategories.Authentication,
                eventName: "AdminLoginFailed",
                message:
                    "Falló un inicio de sesión administrativo.",
                details:
                    "Reason=InvalidCredentials",
                actorUserId: user.Id);

            ModelState.AddModelError(
                string.Empty,
                "Correo o contraseña incorrectos.");
        }

        ReturnUrl = returnUrl;
        return Page();
    }
}
