using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace G02_API.Services
{
    public class CorreoService
    {
        private readonly IConfiguration _configuration;

        public CorreoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarCorreo(
            string destinatario,
            string asunto,
            string contenidoHtml)
        {
            var cuenta = _configuration["Variables:CuentaCorreo"];
            var clave = _configuration["Variables:ClaveCorreo"];

            if (string.IsNullOrWhiteSpace(cuenta) ||
                string.IsNullOrWhiteSpace(clave))
            {
                throw new InvalidOperationException(
                    "La configuración del correo no está completa.");
            }

            var mensaje = new MimeMessage();

            mensaje.From.Add(
                new MailboxAddress("MealPrep", cuenta));

            mensaje.To.Add(
                MailboxAddress.Parse(destinatario));

            mensaje.Subject = asunto;

            mensaje.Body = new BodyBuilder
            {
                HtmlBody = contenidoHtml
            }.ToMessageBody();

            using var cliente = new SmtpClient();

            await cliente.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls);

            await cliente.AuthenticateAsync(cuenta, clave);

            await cliente.SendAsync(mensaje);

            await cliente.DisconnectAsync(true);
        }
    }
}
