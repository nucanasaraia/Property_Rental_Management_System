using PropertyRentalManagementSystem.Enums.TicketEnum;
using PropertyRentalManagementSystem.Models;

namespace PropertyRentalManagementSystem.DTOs
{
    public class MaintenanceDTO
    {
        public int Id { get; set; }
        public string ProblemDescription { get; set; }
        public TICKET_PRIORITY Priority { get; set; } // Low,Medium,High,Critical

        public TICKET_STATUS TicketStatus { get; set; } // Open,InProgress,Closed

        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

    }
}
