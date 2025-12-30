namespace AssetsManagementSystem.DTOs.AssetTransferDTOs
{
    public class BulkRelocationRequestDTO
    {
        // 1. المصدر
        public string SourceLocationBarcode { get; set; }

        // 2. قائمة الباركودات اللي "اتسحبت" من المصدر (الـ Check-out)
        public List<string> PickedAssetBarcodes { get; set; }

        // 3. التوزيعة (قائمة وجهات)
        public List<DestinationDistributionDTO> Distributions { get; set; }
    }

    // كلاس بيمثل كل مخزن وجهة واللي هيدخل جواه
    public class DestinationDistributionDTO
    {
        public string ToLocationBarcode { get; set; }
        public List<string> AssetBarcodesToPlace { get; set; }
    }

    // كلاس للرد النهائي (ملخص العملية)
    public class BulkRelocationResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalMoved { get; set; }
        public List<string> TransactionIds { get; set; }
    }
}
