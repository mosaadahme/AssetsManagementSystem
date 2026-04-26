namespace AssetsManagementSystem.DTOs.AccountServiceDTOs.AddUserRequestDTOs
{
    public class AddUserRequestDTO
    {
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name can't be longer than 50 characters.")]
        [DefaultValue("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name can't be longer than 50 characters.")]
        [DefaultValue("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(255, ErrorMessage = "Email can't be longer than 255 characters.")]
        [DefaultValue("Enter Your Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DefaultValue("Enter Your Password")]
        public string Password { get; set; }


        [Required(ErrorMessage = "Role is required.")]
        public UserRole Role { get; set; }

        public string PhoneNumber { get; set; }
    }
}
