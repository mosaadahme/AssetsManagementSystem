namespace AssetsManagementSystem.DTOs.InventoryDTOs
{
    public class DisplacedAssetSummaryDTO : AuditAssetSummaryDTO
    {
        public string OriginalLocationName { get; set; }
    }
}
