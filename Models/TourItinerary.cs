using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToursAndTravelsManagement.Models
{
    public class TourItinerary
    {
        [Key] 
        public int TourItineraryId { get; set; }

        [Required]
        public int TourId { get; set; }

        [ForeignKey(nameof(TourId))]
        public Tour? Tour { get; set; }

        [Required]
        public int DayNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
