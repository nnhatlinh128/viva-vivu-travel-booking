namespace ToursAndTravelsManagement.Services.EmailService;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendEmailAsync(string to, string subject, string body, byte[] attachment);
    Task SendTicketEmailAsync(string to, string subject, string body, byte[] attachment);

}
