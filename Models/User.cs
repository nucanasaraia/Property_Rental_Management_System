using PropertyRentalManagementSystem.Enums.UserEnum;

namespace PropertyRentalManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public ACCOUNT_STATUS Status { get; set; } = ACCOUNT_STATUS.CODE_SENT;
        public string? VerificationCode { get; set; }
        public string? PasswordResetCode { get; set; }


        public string UserRole { get; set; } // Owner, Tenant, Admin
        public string PhoneNumber { get; set; }

        public List<Rental> Rental{ get; set; } = new List<Rental>();

        public List<Review> Reviews { get; set; } = new List<Review>();

    }
}
