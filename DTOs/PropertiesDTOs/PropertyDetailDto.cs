using PropertyRentalManagementSystem.Enums.PropertyEnum;
using PropertyRentalManagementSystem.Models;

namespace PropertyRentalManagementSystem.DTOs.PropertiesDTOs
{
    public class PropertyDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public PROPERTY_TYPE Type { get; set; }
        public double Area { get; set; }
        public int NumberOfRooms { get; set; }
        public int Floor { get; set; }
        public ICollection<string> Photos { get; set; }
        public ICollection<AMENITY> Amenities { get; set; }
        public decimal Price { get; set; }
        public PROPERTY_STATUS PropertyStatus { get; set; }
        public List<MaintenanceTicket> MaintenanceTickets { get; set; }
        public List<RentalDto> Rentals { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
