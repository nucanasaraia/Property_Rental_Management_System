using PropertyRentalManagementSystem.Enums.PaymentEnum;

namespace PropertyRentalManagementSystem.DTOs.PaymentDTOs
{
    public class PaymentsDto
    {
        public decimal Amount { get; set; }
        public PAYMENT_TYPE PaymentType { get; set; } // Rent, Deposit, Penalty
    }
}
