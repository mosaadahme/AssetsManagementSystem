using AssetsManagementSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssetsManagementSystem.DTOs.AssetSearchDTOs
{
    #region Simple Search Criteria
    /// <summary>
    /// Basic search criteria for assets (General Search)
    /// </summary>
    public class AssetSearchCriteria
    {
        // البحث العام (بيبحث في الاسم والباركود والسيريال)
        public string? SearchTerm { get; set; }
    }
    #endregion

    #region Advanced Search Criteria
    /// <summary>
    /// Detailed search filters
    /// </summary>
    public class AdvancedAssetSearchCriteria
    {
        // Basic Info
        public string? Name { get; set; }
        public string? Barcode { get; set; } // أهم حقل
        public string? SerialNumber { get; set; }
        public string? ModelNumber { get; set; }

        // يمكن البحث بالـ Enum (Available, InUse...)
        public string? Status { get; set; }

        public AssetType? AssetType { get; set; } // IT, NonIT, Consumable

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

    #region Asset Response DTO (Shared)
    /// <summary>
    /// The unified response shape for all asset queries
    /// </summary>
    public class GetAssetResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; } // Added
        public string AssetType { get; set; } // IT, NonIT...
        public string ModelNumber { get; set; }
        public string? SerialNumber { get; set; } // Nullable
        public string? Description { get; set; }

        public DateOnly PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }

        public DateOnly? WarrantyExpiryDate { get; set; } // Nullable
        public DateOnly? DepreciationDate { get; set; }   // Nullable

        public string Status { get; set; }

        // Location Info
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? LocationBarcode { get; set; }

        // User Info (Nullable)
        public Guid? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }

        // Category Info
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Manufacturer Info
        public int? ManufacturerId { get; set; }
        public string? ManufacturerName { get; set; }

        // Suppliers (List of names)
        public List<string> SupplierNames { get; set; } = new List<string> ( );

        // Stock Info
        public int Quantity { get; set; }
        public int? MinQuantityLimit { get; set; }

        public DateTime? AddedOnDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    #endregion

    #region Asset Summary Report DTO
    /// <summary>
    /// Statistics Dashboard Data
    /// </summary>
    public class AssetSummaryReportDTO
    {
        public int TotalAssets { get; set; }
        public decimal TotalValue { get; set; } // إجمالي القيمة المالية

        // تفصيل الحالات بناءً على الـ Enum الجديد
        public int AvailableAssets { get; set; } // في المخزن
        public int ActiveAssets { get; set; }    // InUse (مع موظفين)
        public int UnderMaintenanceAssets { get; set; }
        public int RetiredAssets { get; set; }   // كهنة/تالف

        // تنبيهات
        public int LowStockAssets { get; set; }
        public int ExpiredWarrantyAssets { get; set; }
    }
    #endregion
}