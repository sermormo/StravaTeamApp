using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StravaTeamApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    public string? ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; } = string.Empty;

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Limpiamos la sesión externa anterior para garantizar un inicio limpio
        await HttpContext.SignOutAsync(Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }
}