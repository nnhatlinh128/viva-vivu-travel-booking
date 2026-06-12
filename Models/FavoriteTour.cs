using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ToursAndTravelsManagement.Models;

namespace ToursAndTravelsManagement.Models
{
    public class FavoriteTour
    {
        [Key]
        public int FavoriteTourId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required]
        public int TourId { get; set; }

        [ForeignKey(nameof(TourId))]
        public Tour Tour { get; set; }
    }
}
