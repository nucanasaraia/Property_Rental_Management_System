using PropertyRentalManagementSystem.Enums.TicketEnum;
using System.Text.Json.Serialization;

namespace PropertyRentalManagementSystem.Models
{
    public class MaintenanceTicket
    {
        public int Id { get; set; }

        [JsonIgnore]
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int? RentalId { get; set; }   
        public Rental Rental { get; set; }

        public string ProblemDescription { get; set; }
        public TICKET_PRIORITY Priority { get; set; } // Low,Medium,High,Critical

        public TICKET_STATUS TicketStatus { get; set; } // Open,InProgress,Closed

        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
