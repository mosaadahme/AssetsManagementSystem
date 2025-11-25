namespace AssetsManagementSystem.DTOs.CategoryDTOs
{
    public class GetCategoryRequestDTO
    {
        public int Id { get; set; }

      
        public string Name { get; set; }

        public string SerialCode { get; set; }
        public string Description { get; set; }
        public string AssetType { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public DateTime AddedOnDate { set; get; }
        public DateTime? UpdatedDate { set; get; }
      
    }
}
