namespace AssetsManagementSystem.DTOs.InventoryDTOs
{
    public class AuditReportResponseDTO
    {
        public int AuditId { get; set; }
        public DateTime Date { get; set; }
        public string LocationName { get; set; }
        public string AuditorName { get; set; }
        public string Status { get; set; }

        // --- الإحصائيات ---
        public int TotalExpected { get; set; }
        public int TotalScanned { get; set; }
        public int MatchCount { get; set; }
        public int MissingCount { get; set; }
        public int DisplacedCount { get; set; }
         
        public List<AuditAssetSummaryDTO> MatchedAssets { get; set; } = new List<AuditAssetSummaryDTO> ( );
         
        public List<AuditAssetSummaryDTO> MissingAssets { get; set; } = new List<AuditAssetSummaryDTO> ( );
         
        public List<DisplacedAssetInfoDTO> DisplacedAssets { get; set; } = new List<DisplacedAssetInfoDTO> ( );
         
        public List<string> UnknownBarcodes { get; set; } = new List<string> ( );
    }
}
