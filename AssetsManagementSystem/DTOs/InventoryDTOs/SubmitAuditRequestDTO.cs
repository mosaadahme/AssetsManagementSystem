namespace AssetsManagementSystem.DTOs.InventoryDTOs
{
    public class SubmitAuditRequestDTO
    {
        [Required ( ErrorMessage = "Audit ID is required." )]
        public int AuditId { get; set; }

        [Required ( ErrorMessage = "Scanned barcodes list is required." )]
        [MinLength ( 1, ErrorMessage = "At least one barcode must be scanned." )]
        public List<string> ScannedBarcodes { get; set; } = new List<string> ( );
    }
}
