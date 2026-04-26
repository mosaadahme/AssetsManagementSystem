namespace AssetsManagementSystem.DTOs.AccountServiceDTOs.AllUsers
{
    public class AllUsersResponse
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string UserStatus { get; set; }
    }
}
