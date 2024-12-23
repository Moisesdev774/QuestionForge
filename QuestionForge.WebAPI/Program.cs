using QuestionForge.LogicaDeNegocio;
using QuestionForge.AccesoADatos;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton(connectionString);

// Registro de dependencias
builder.Services.AddScoped<UsuarioBL>(provider =>
{
    var conn = provider.GetRequiredService<string>();
    return new UsuarioBL(conn);
});

builder.Services.AddScoped<PreguntaDAL>(provider =>
{
    var conn = provider.GetRequiredService<string>();
    return new PreguntaDAL(conn);
});

builder.Services.AddScoped<RespuestaDAL>(provider =>
{
    var conn = provider.GetRequiredService<string>();
    return new RespuestaDAL(conn);
});

builder.Services.AddScoped<PreguntaBL>();
builder.Services.AddScoped<RespuestaBL>();

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
