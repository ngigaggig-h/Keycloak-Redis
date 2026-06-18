using FinalProject.Data;
using FinalProject.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

// Головна точка входу в застосунок (startup).
// Тут налаштовується інфраструктура проєкту:
// - MVC;
// - підключення БД;
// - DI-сервіси;
// - cookie-авторизація;
// - маршрути.
var builder = WebApplication.CreateBuilder(args);

// Додаємо MVC-контролери + views (Razor).
builder.Services.AddControllersWithViews();

// Беремо рядок підключення до SQL Server / LocalDB з appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Реєструємо EF Core контекст.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Реєструємо сервіси бізнес-логіки в DI.
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IHashService, SimpleHashService>();
builder.Services.AddScoped<IAuthService, AuthServiceWithHash>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = "http://localhost:8080/realms/Keycloak-Redis";
    options.ClientId = "mvc-client";
    options.ClientSecret = "q89J850hJkch6cBnzlYHkaF7jzeWhGAU";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.RequireHttpsMetadata = false;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "preferred_username",
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };

    options.Events = new OpenIdConnectEvents
    {
        //OnTokenValidated = context =>
        //{
        //    var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
        //    if (identity != null)
        //    {
        //        var roleClaims = identity.FindAll("roles").ToList();
        //        foreach (var role in roleClaims)
        //        {
        //            identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role.Value));
        //        }
        //    }
        //    return Task.CompletedTask;
        //}
    };
});

var app = builder.Build();

await ApplicationDbInitializer.InitializeAsync(app.Services);

// HTTP pipeline (порядок важливий).
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Маршрут за замовчуванням:
// /Home/Index або /{controller}/{action}/{id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
