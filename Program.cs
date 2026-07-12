using StravaTeamApp.Services;
using StravaTeamApp.Data;
using StravaTeamApp.Models;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<StravaAthleteService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=StravaTeam.db"));


builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    // Opciones relajadas para facilitar las pruebas del equipo
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>();

// Configuración de inicio de sesión externo con Strava
builder.Services.AddAuthentication()
    .AddStrava(options =>
    {
        // Reemplaza esto con los números de tu portal de Strava
        options.ClientId = "127168"; 
        options.ClientSecret = "9b4d574479c806b5e574f55dc46caf53a9395a85"; 
        options.SaveTokens = true;
        options.Scope.Add("activity:read_all");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
