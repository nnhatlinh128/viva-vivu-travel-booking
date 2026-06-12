namespace ToursAndTravelsManagement.ViewModels
{
    public class AdminUserViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public string Role { get; set; }      // Admin / User
        public bool IsLocked { get; set; }    // Bị khóa hay không
    }
}
