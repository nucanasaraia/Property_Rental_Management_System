using PropertyRentalManagementSystem.Enums.RentalEnum;
using PropertyRentalManagementSystem.Models;

namespace PropertyRentalManagementSystem.DTOs
{
    public class RentalDto
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public RENTAL_STATUS RentalStatus { get; set; }

        public decimal Amount { get; set; }
        public PAYMENT_FREQUENCY PaymentFrequency { get; set; } // Monthly, Weekly
        public bool IsPaid { get; set; }
    }
}
