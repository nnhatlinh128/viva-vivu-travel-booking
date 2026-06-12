using System.ComponentModel.DataAnnotations;

namespace ToursAndTravelsManagement.Models
{
    public class Voucher
    {
        [Key]
        public int VoucherId { get; set; }

        [Required]
        public string Code { get; set; }   // VD: SALE10

        [Required]
        public decimal DiscountValue { get; set; } 
        // Nếu % thì là 10, nếu tiền thì là 100000

        public bool IsPercentage { get; set; } 
        // true = %, false = tiền mặt

        public decimal? MaxDiscountAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int Quantity { get; set; } // số lượt dùng

        public bool IsActive { get; set; }
    }
}
