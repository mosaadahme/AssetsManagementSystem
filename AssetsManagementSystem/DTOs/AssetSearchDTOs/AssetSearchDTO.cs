namespace AssetsManagementSystem.DTOs.AssetSearchDTOs
{
    public class AssetSearchDTO
    {
    }

    #region Simple Search Criteria
    /// <summary>
    /// Basic search criteria for assets
    /// </summary>
    public class AssetSearchCriteria
    {
        public string? Name { get; set; }
        public string? SerialNumber { get; set; }
        public string? ModelNumber { get; set; }
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public Guid? AssignedUserId { get; set; }
        public int? ManufacturerId { get; set; }
    }
    #endregion

    #region Advanced Search Criteria
    /// <summary>
    /// Advanced search criteria with range filters
    /// </summary>
    public class AdvancedAssetSearchCriteria
    {
        // Basic Info
        public string? Name { get; set; }
        public string? SerialNumber { get; set; }
        public string? ModelNumber { get; set; }
        public string? Status { get; set; }

        // Relations
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public Guid? AssignedUserId { get; set; }
        public int? ManufacturerId { get; set; }

        // Price Range
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Date Range
        public DateOnly? PurchaseDateFrom { get; set; }
        public DateOnly? PurchaseDateTo { get; set; }

        // Quantity Range
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }

        // Boolean Filters
        public bool? WarrantyExpired { get; set; }
        public bool? LowStock { get; set; }
    }
    #endregion

    #region Asset Response DTO
    /// <summary>
    /// Response DTO for Asset data
    /// </summary>
    public class GetAssetResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public string? Description { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateOnly WarrantyExpiryDate { get; set; }
        public DateOnly DepreciationDate { get; set; }
        public string Status { get; set; }

        public int LocationId { get; set; }
        public string? LocationName { get; set; }

        public Guid AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int ManufacturerId { get; set; }
        public string? ManufacturerName { get; set; }

        public int Quantity { get; set; }
        public int? MinQuantityLimit { get; set; }

        public DateTime? AddedOnDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    #endregion

    #region Asset Summary Report DTO
    /// <summary>
    /// Summary statistics for assets
    /// </summary>
    public class AssetSummaryReportDTO
    {
        public int TotalAssets { get; set; }
        public decimal TotalValue { get; set; }
        public int ActiveAssets { get; set; }
        public int InactiveAssets { get; set; }
        public int UnderMaintenanceAssets { get; set; }
        public int LowStockAssets { get; set; }
        public int ExpiredWarrantyAssets { get; set; }
    }
    #endregion
}
