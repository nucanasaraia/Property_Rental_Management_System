using PropertyRentalManagementSystem.Enums.RentalEnum;

namespace PropertyRentalManagementSystem.Models
{
    public class Rental
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int ContractlId { get; set; }
        public Contract Contract { get; set; }

        public List<Payment> Payments { get; set; } = new List<Payment>();
        public List<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();



        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public RENTAL_STATUS RentalStatus { get; set; }


        public decimal Amount { get; set; }
        public PAYMENT_FREQUENCY PaymentFrequency { get; set; }  // Monthly, Weekly
        public bool IsPaid { get; set; }

    }
}
