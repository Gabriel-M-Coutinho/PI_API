using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using PI_API.models;
using PI_API.settings;


namespace PI_API.services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(SendEmailDTO sendEmailDto)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(sendEmailDto.Emailto));
            email.Subject = sendEmailDto.subject;
            email.Body = new TextPart(TextFormat.Html) 
            { 
                Text = sendEmailDto.message 
            };

            // Conexão
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);

            // Autenticação
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

            // Envio de e-mail
            await smtp.SendAsync(email);

            // Desconexão
            await smtp.DisconnectAsync(true);
        }

        
        
    }
}