using PropertyRentalManagementSystem.Enums.PaymentEnum;

namespace PropertyRentalManagementSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int RentalId { get; set; }
        public Rental Rental { get; set; }

        public DateTime NextDuoDate { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PAYMENT_STATUS PaymentStatus { get; set; } // Pending, Completed, Failed
        public PAYMENT_TYPE PaymentType { get; set; } // Rent, Deposit, Penalty


    }
}
