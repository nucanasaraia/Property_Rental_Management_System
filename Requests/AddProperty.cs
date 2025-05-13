using PropertyRentalManagementSystem.Enums.PropertyEnum;

namespace PropertyRentalManagementSystem.Requests
{
    public class AddProperty
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }


        public PROPERTY_TYPE Type { get; set; } // apartment, house, commercial space

        public double Area { get; set; }
        public int NumberOfRooms { get; set; }
        public int Floor { get; set; }


        public ICollection<string> Photos { get; set; }
        public ICollection<AMENITY> Amenities { get; set; }

        public decimal Price { get; set; }       
        public PROPERTY_STATUS PropertyStatus { get; set; }
    }
}
