namespace ToursAndTravelsManagement.Services.EmailService;

public class EmailSettings
{
    public string SMTPServer { get; set; }
    public int SMTPPort { get; set; }
    public string SenderEmail { get; set; }
    public string SenderName { get; set; }
    public string SMTPUsername { get; set; }
    public string SMTPPassword { get; set; }
}