namespace AssetsManagementSystem.DTOs.SupplierDTOs
{
    public class AddSupplierRequestDTO
    {
        [Required(ErrorMessage = "Supplier name is required.")]
        [MaxLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters.")]
        public string CompanyName { get; set; }

        public string email { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Note { get; set; }
        public List<SupplierContactPersonDTO> ContactPersons { get; set; } = new List<SupplierContactPersonDTO> ( );
    }
 }
