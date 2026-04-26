namespace AssetsManagementSystem.DTOs.LocationDTOs
{
    public class UpdateLocationRequestDTO
    {
        [Required ( ErrorMessage = "Location name is required." )]
        [MaxLength ( 200, ErrorMessage = "Location name cannot exceed 200 characters." )]
        public string Name { get; set; }

        // 🛑 تم إزالة الـ Address (لو غير الاسم أو الـ Parent، السيرفيس هيحسب الـ Address من تاني)

        [Required ( ErrorMessage = "Location level is required." )]
        public LocationLevel Level { get; set; }

        public int? ParentLocationId { get; set; }
    }
}
