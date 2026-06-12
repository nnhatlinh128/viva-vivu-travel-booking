using Microsoft.AspNetCore.Http;
using System;

namespace ToursAndTravelsManagement.ViewModels
{
    public class UpdateUserProfileViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}
