var builder = WebApplication.CreateBuilder(args);

// Agrega controladores y vistas
builder.Services.AddControllersWithViews();

// Configura HttpClient para consumir la API de Pregunta
builder.Services.AddHttpClient("PreguntaAPI", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["UrlsAPI:PreguntaAPI"]);
});

// Configuración de sesión dependiendo del entorno
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración
        options.Cookie.HttpOnly = true; // Solo accesible por el servidor
        options.Cookie.IsEssential = true; // Cookie esencial
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // No requiere HTTPS en desarrollo
        options.Cookie.SameSite = SameSiteMode.Lax; // Más permisivo en desarrollo
    });
}
else
{
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requiere HTTPS en producción
        options.Cookie.SameSite = SameSiteMode.Strict; // Más seguro en producción
    });
}

builder.Services.AddDistributedMemoryCache(); // Configura la memoria para sesiones

var app = builder.Build();

// Configuración del middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Manejador de errores
    app.UseHsts(); // Forzar HTTPS
}

app.UseHttpsRedirection(); // Redirige a HTTPS
app.UseStaticFiles(); // Habilita archivos estáticos
app.UseRouting(); // Configura las rutas
app.UseAuthorization(); // Autoriza el acceso
app.UseSession(); // Manejo de sesiones

// Ruta predeterminada
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();
