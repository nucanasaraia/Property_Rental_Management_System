namespace PropertyRentalManagementSystem.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int ReviewerId { get; set; }
        public User Reviewer { get; set; }

        public int ReviewedTenantId { get; set; }
        public User ReviewedUser { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewedOn { get; set; }
    }

}
