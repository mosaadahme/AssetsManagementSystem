namespace AssetsManagementSystem.DTOs.CategoryDTOs
{
    public class AddCategoryRequestDTO
    {
        [Required ( ErrorMessage = "Category name is required." )]
        [MaxLength ( 100 )]
        public string Name { get; set; }

        [MaxLength ( 500 )]
        public string Description { get; set; }

 
        [Required ( ErrorMessage = "Serial Code is required." )]
        [MaxLength ( 10 )]
        [RegularExpression ( @"^[A-Z0-9]+$", ErrorMessage = "Code must be uppercase letters/numbers only." )]
        public string SerialCode { get; set; }

        public int? ParentCategoryId { get; set; }
    }
}
