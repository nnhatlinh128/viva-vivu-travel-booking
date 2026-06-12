using System.ComponentModel.DataAnnotations;

namespace ToursAndTravelsManagement.Services.EmailService;

public class ContactFormModel
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    public string Subject { get; set; }

    [Required(ErrorMessage = "Message is required.")]
    public string Message { get; set; }
}
