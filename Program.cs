using StravaTeamApp.Services;
using StravaTeamApp.Data;
using StravaTeamApp.Models;
using StravaTeamApp.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Missing configuration: ConnectionStrings:DefaultConnection");

var stravaClientId =
    builder.Configuration["Strava:ClientId"]
    ?? throw new InvalidOperationException(
        "Missing configuration: Strava:ClientId");

var stravaClientSecret =
    builder.Configuration["Strava:ClientSecret"]
    ?? throw new InvalidOperationException(
        "Missing configuration: Strava:ClientSecret");


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<StravaAthleteService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// Configuración de inicio de sesión externo con Strava
builder.Services.AddAuthentication()
    .AddStrava(options =>
    {
        options.ClientId = stravaClientId; 
        options.ClientSecret = stravaClientSecret; 
        options.SaveTokens = true;
        options.Scope.Add("activity:read_all");
    });

var app = builder.Build();

//Seed database
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(
        scope.ServiceProvider);
}

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
