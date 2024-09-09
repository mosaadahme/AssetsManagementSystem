﻿namespace AssetsManagementSystem.DTOs.AssetTransferDTOs
{
    public class LocationToLocationTransferDTO
    {
        [Required(ErrorMessage = "Asset ID is required.")]
        public int AssetId { get; set; }

        [Required(ErrorMessage = "From Location ID is required.")]
        public int FromLocationId { get; set; }

        [Required(ErrorMessage = "To Location ID is required.")]
        public int ToLocationId { get; set; }

        
    }
}
