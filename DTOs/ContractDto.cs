using PropertyRentalManagementSystem.Enums;

namespace PropertyRentalManagementSystem.DTOs
{
    public class ContractDto
    {
        public int Id { get; set; }
        public string Terms { get; set; }
        public string Conditions { get; set; }
        public decimal Amount { get; set; }
        public CONTRACT_STATUS ContractStatus { get; set; }

    }
}
