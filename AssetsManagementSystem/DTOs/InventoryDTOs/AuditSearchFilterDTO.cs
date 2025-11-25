namespace AssetsManagementSystem.DTOs.InventoryDTOs
{
    public class AuditSearchFilterDTO
    {
        public int? LocationId { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // ده عشان نبحث جوة التفاصيل: "هاتلي الجرد اللي ظهر فيه اللابتوب ده"
        public string? AssetBarcode { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
