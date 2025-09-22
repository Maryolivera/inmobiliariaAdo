using Microsoft.AspNetCore.Authentication.Cookies; // <-- ya lo tenías

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
builder.Services.AddScoped<InmobiliariaAdo.Data.TipoInmuebleRepositorio>();

// === SESIONES (necesario para el avatar en _Layout) ===
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // expira después de 30 minutos inactivo
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// === Autenticación por Cookies ===
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".Inmobiliaria.Auth";
        options.LoginPath = "/Cuenta/Login";          // acción de login
        options.AccessDeniedPath = "/Cuenta/Denegado"; 
        options.LogoutPath = "/Cuenta/Logout";        // acción de logout
        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

// === Autorización por Roles/Políticas ===
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
app.UseStaticFiles();

app.UseRouting();

// 🚨 Sesión debe ir ANTES de Authentication/Authorization
app.UseSession();

app.UseAuthentication();   
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
