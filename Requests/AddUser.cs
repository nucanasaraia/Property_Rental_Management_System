using PropertyRentalManagementSystem.Enums.UserEnum;

namespace PropertyRentalManagementSystem.Requests
{
    public class AddUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserRole { get; set; } // Owner, Tenant, Admin
        public string PhoneNumber { get; set; }
    }
}
