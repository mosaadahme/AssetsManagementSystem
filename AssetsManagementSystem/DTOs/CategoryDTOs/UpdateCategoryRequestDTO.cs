namespace AssetsManagementSystem.DTOs.CategoryDTOs
{
    public class UpdateCategoryRequestDTO
    {
        [Required]
        [MaxLength ( 100 )]
        public string Name { get; set; }

        [MaxLength ( 500 )]
        public string Description { get; set; }

        public int? ParentCategoryId { get; set; }

    }
}
