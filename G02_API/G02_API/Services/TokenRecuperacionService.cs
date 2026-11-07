using System.Security.Cryptography;
using System.Text;

namespace G02_API.Services
{
    public class TokenRecuperacionService
    {
        public string GenerarToken()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public string ObtenerHash(string token)
        {
            byte[] bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(bytes);
        }
    }
}
