using System.ComponentModel.DataAnnotations;

namespace ToursAndTravelsManagement.Attributes;

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var date = (DateTime)value;

        if (date < DateTime.Today)
        {
            return new ValidationResult("End date must be in the future.");
        }

        return ValidationResult.Success;
    }
}
