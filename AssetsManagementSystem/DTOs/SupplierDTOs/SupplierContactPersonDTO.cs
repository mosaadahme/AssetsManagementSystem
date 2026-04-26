namespace AssetsManagementSystem.DTOs.SupplierDTOs
{
    public class SupplierContactPersonDTO
    {
         public int? Id { get; set; }

        [Required ( ErrorMessage = "Contact person name is required." )]
        public string Name { get; set; }

        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
    }
}