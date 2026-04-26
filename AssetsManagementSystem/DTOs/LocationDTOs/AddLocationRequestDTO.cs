namespace AssetsManagementSystem.DTOs.LocationDTOs
{
    public class AddLocationRequestDTO
    {


        [Required ( ErrorMessage = "Barcode is required." )]
        [MaxLength ( 100 )]
        public string Barcode { get; set; }

        [Required ( ErrorMessage = "Location name is required." )]
        [MaxLength ( 200, ErrorMessage = "Location name cannot exceed 200 characters." )]
        public string Name { get; set; }

        // 🛑 تم إزالة الـ Address من هنا لأن السيرفيس هتكريته أوتوماتيك

        // ==========================================
        // الإضافات الجديدة
        // ==========================================

        [Required ( ErrorMessage = "Location level is required." )]
        public LocationLevel Level { get; set; }

        // يقبل Null عشان لو بنضيف "دولة" ملهاش أب
        public int? ParentLocationId { get; set; }


    }
}
