using Microsoft.AspNetCore.Authentication.Cookies; // <-- agrega este using

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Repos y servicios
builder.Services.AddSingleton<InmobiliariaAdo.Data.Db>();
builder.Services.AddScoped<InmobiliariaAdo.Data.PropietarioRepositorio>();
builder.Services.AddScoped<InmobiliariaAdo.Data.InquilinoRepositorio>();
builder.Services.AddScoped<InmobiliariaAdo.Data.ContratoRepositorio>();
builder.Services.AddScoped<InmobiliariaAdo.Data.InmuebleRepositorio>();
builder.Services.AddScoped<InmobiliariaAdo.Data.PagoRepositorio>();
builder.Services.AddScoped<InmobiliariaAdo.Data.UsuarioRepositorio>();


// === PASO 2: Autenticación por Cookies ===
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".Inmobiliaria.Auth";
        options.LoginPath = "/Cuenta/Login";                 // acción de login
        options.AccessDeniedPath = "/Cuenta/Denegado"; // vista de 403
        options.LogoutPath = "/Cuenta/Logout";               // acción de logout
        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

// === PASO 2: Autorización por Roles/Políticas ===
// (Administrador puede todo; Empleado al menos su perfil)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EsAdmin", p => p.RequireRole("Administrador"));
    options.AddPolicy("EsEmpleado", p => p.RequireRole("Empleado", "Administrador"));
});

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// IMPORTANTE: el orden: primero Authentication, luego Authorization
app.UseAuthentication();   
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
