var builder = WebApplication.CreateBuilder(args);

// Configura servicios y dependencias
builder.Services.AddControllersWithViews();


// Configura el cliente HttpClient para consumir la API de Usuario
builder.Services.AddHttpClient("UsuarioAPI", c =>
{
    // La URL base de la API de Usuario, puedes cambiarla según el entorno
    c.BaseAddress = new Uri(builder.Configuration["UrlsAPI:UsuarioAPI"]);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Configuración de middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();


// Configuración de la ruta predeterminada
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();
