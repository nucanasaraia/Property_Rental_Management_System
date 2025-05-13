using PropertyRentalManagementSystem.Enums;

namespace PropertyRentalManagementSystem.Models
{
    public class Contract
    {
        public int Id { get; set; }

        public int RentalId { get; set; }
        public Rental Rental { get; set; }

        public string Terms { get; set; }
        public string Conditions { get; set; }
        public decimal Amount { get; set; }

        public CONTRACT_STATUS ContractStatus { get; set; } // Draft, Signed,Terminated

    }
}
