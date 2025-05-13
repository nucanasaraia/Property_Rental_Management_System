namespace PropertyRentalManagementSystem.DTOs.PaymentDTOs
{
    public class upcomingPaymentsDTO
    {
        public int RentalId { get; set; }
        public DateTime NextDueDate { get; set; }
        public decimal Amount { get; set; }
    }
}
