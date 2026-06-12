using System.ComponentModel.DataAnnotations;

namespace ToursAndTravelsManagement.Models
{
    public class MembershipTier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }           // Bronze, Silver, Gold...

        [Required]
        public decimal MinRevenue { get; set; }    // Doanh thu tối thiểu

        [Required]
        public int DiscountPercent { get; set; }   // % giảm giá
    }
}
