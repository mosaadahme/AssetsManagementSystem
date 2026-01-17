namespace AssetsManagementSystem.DTOs.AssetTransferDTOs
{
    public class BulkMoveToLocationDTO
    {
        public string TargetLocationBarcode { get; set; }

        // الأصول اللي هتتنقل
        public List<string> AssetBarcodes { get; set; }
    }
}
