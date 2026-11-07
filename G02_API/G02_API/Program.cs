using Scalar.AspNetCore;
using G02_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Servicios existentes
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<CorreoService>();
builder.Services.AddScoped<TokenRecuperacionService>();
builder.Services.AddScoped<JwtService>();

// Configuración JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "No se configuró la clave JWT.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "No se configuró el emisor JWT.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "No se configuró la audiencia JWT.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configuración del entorno
if (app.Environment.IsDevelopment())
{
    InicializadorAdministrador.Inicializar(app.Configuration);

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
