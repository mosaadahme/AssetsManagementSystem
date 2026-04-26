namespace AssetsManagementSystem.DTOs.LocationDTOs
{
    public class GetLocationRequestDTO
    {
        public int Id { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Address { get; set; } // ده هيرجع العنوان الكامل التفصيلي
        public DateTime AddedOnDate { set; get; }
        public DateTime? UpdatedDate { set; get; }

        // ==========================================
        // الإضافات الجديدة
        // ==========================================

        public LocationLevel Level { get; set; }

        // حقل إضافي بيحول الـ Enum لـ String عشان الفرونت إند يعرضه بسهولة (مثلاً: "City")
        public string LevelName => Level.ToString ( );

        public int? ParentLocationId { get; set; }

        // (اختياري بس ممتاز للـ UX) لو حابب ترجع اسم المكان الأب كمان
        public string ParentLocationName { get; set; }

    }
}
