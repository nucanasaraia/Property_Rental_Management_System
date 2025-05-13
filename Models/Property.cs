using Microsoft.Extensions.Options;
using PropertyRentalManagementSystem.Enums.PropertyEnum;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace PropertyRentalManagementSystem.Models
{
    public class Property
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }

        
        public PROPERTY_TYPE Type { get; set; } // apartment, house, commercial space

        public double Area { get; set; }            
        public int NumberOfRooms { get; set; }
        public int Floor { get; set; }


        public ICollection<string> Photos { get; set; }       
        public ICollection<AMENITY> Amenities { get; set; }

        public decimal Price { get; set; }        // monthly rent or sale price
        public PROPERTY_STATUS PropertyStatus { get; set; }

        public int UserId { get; set; }         
        public User User { get; set; }

        public List<MaintenanceTicket> MaintenanceTickets { get; set; } = new List<MaintenanceTicket>();

        public List<Rental> Rentals { get; set; } = new List<Rental>();

        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
