using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace ToursAndTravelsManagement.Services.EmailService;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    // ================= BASIC EMAIL =================
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _emailSettings.SenderName,
                _emailSettings.SenderEmail
            ));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            // ⚠️ DEV ONLY – bypass SSL certificate
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await smtp.ConnectAsync(
                _emailSettings.SMTPServer,
                _emailSettings.SMTPPort,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _emailSettings.SMTPUsername,
                _emailSettings.SMTPPassword
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // KHÔNG throw – chỉ log
            Console.WriteLine("SendEmailAsync failed: " + ex.Message);
        }
    }

    // ================= EMAIL WITH ATTACHMENT =================
    public async Task SendEmailAsync(string to, string subject, string body, byte[] attachment)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _emailSettings.SenderName,
                _emailSettings.SenderEmail
            ));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                TextBody = body
            };

            if (attachment != null && attachment.Length > 0)
            {
                builder.Attachments.Add(
                    "report.pdf",
                    attachment,
                    new ContentType("application", "pdf")
                );
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            // ⚠️ DEV ONLY – bypass SSL certificate
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(
                _emailSettings.SMTPServer,
                _emailSettings.SMTPPort,
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _emailSettings.SMTPUsername,
                _emailSettings.SMTPPassword
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("SendEmailAsync (attachment) failed: " + ex.Message);
        }
    }

    // ================= TICKET EMAIL =================
    public async Task SendTicketEmailAsync(string to, string subject, string body, byte[] attachment)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _emailSettings.SenderName,
                _emailSettings.SenderEmail
            ));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                TextBody = body
            };

            if (attachment != null && attachment.Length > 0)
            {
                builder.Attachments.Add(
                    "ticket.pdf",
                    attachment,
                    new ContentType("application", "pdf")
                );
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            // ⚠️ DEV ONLY – bypass SSL certificate
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(
                _emailSettings.SMTPServer,
                _emailSettings.SMTPPort,
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _emailSettings.SMTPUsername,
                _emailSettings.SMTPPassword
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("SendTicketEmailAsync failed: " + ex.Message);
        }
    }
}
