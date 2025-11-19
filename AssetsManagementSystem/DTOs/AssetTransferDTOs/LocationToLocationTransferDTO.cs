namespace AssetsManagementSystem.DTOs.AssetTransferDTOs
{
    public class LocationToLocationTransferDTO
    {
        [Required(ErrorMessage = "Asset ID is required.")]
        public string AssetBarcode { get; set; }

        [Required(ErrorMessage = "From Location ID is required.")]
        public string FromLocationBarcode { get; set; }

        [Required(ErrorMessage = "To Location ID is required.")]
        public string ToLocationBarcode { get; set; }

        
    }
}
