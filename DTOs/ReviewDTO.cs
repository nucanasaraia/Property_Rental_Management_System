using PropertyRentalManagementSystem.Models;

namespace PropertyRentalManagementSystem.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewedOn { get; set; }
    }
}
