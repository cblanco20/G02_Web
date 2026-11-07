using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace G02_API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(int idUsuario, int idRol)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "No se configuró la clave JWT.");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "No se configuró el emisor JWT.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "No se configuró la audiencia JWT.");

            int minutos = _configuration.GetValue<int>(
                "Jwt:ExpirationMinutes");

            if (minutos <= 0)
            {
                throw new InvalidOperationException(
                    "La duración del token JWT no es válida.");
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    idUsuario.ToString()),

                new Claim(
                    ClaimTypes.Role,
                    idRol.ToString())
            };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutos),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
