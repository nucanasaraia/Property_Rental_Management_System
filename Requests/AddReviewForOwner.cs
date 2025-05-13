namespace PropertyRentalManagementSystem.Requests
{
    public class AddReviewForOwner
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public int ReviewedTenantId { get; set; }
    }
}
