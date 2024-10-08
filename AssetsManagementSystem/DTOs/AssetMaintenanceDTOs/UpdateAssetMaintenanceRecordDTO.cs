﻿namespace AssetsManagementSystem.DTOs.AssetMaintenanceDTOs
{
    public class UpdateAssetMaintenanceRecordDTO
    {
        [Required(ErrorMessage = "Maintenance record ID is required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Maintenance date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [PastOrPresentDate(ErrorMessage = "Maintenance date cannot be in the future.")]
        public DateOnly MaintenanceDate { get; set; }

        [Required(ErrorMessage = "Maintenance description is required.")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "USer Id date is required.")]
        public Guid TOWhomOfUserId { get; set; }

        [Required(ErrorMessage = "Supplier Id is required.")]
        public int TOWhomOfSupplierId { get; set; }
    }
}
