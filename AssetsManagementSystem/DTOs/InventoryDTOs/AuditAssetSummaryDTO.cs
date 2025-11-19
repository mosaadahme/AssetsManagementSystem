namespace AssetsManagementSystem.DTOs.InventoryDTOs
{
    public class AuditAssetSummaryDTO
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string? SerialNumber { get; set; }
    }
}
