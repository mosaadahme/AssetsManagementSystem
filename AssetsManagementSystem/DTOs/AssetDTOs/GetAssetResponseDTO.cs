namespace AssetsManagementSystem.DTOs.AssetDTOs
{
    public class GetAssetResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // 1. أهم حقل ناقص
        public string Barcode { get; set; }

        // 2. مهم عشان الـ Frontend يعرف يرسم الواجهة (يخفي السيريال لو مش IT)
        public string AssetType { get; set; }

        public string ? ModelNumber { get; set; }

        // بقى Nullable عشان الـ Non-IT
        public string? SerialNumber { get; set; }

        public string? Description { get; set; } // تم تصحيح الاسم

        public DateOnly PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateOnly? WarrantyExpiryDate { get; set; }

        // بقى Nullable
        public DateOnly? DepreciationDate { get; set; }

        public string Status { get; set; }

        // --- Location Info ---
        public int LocationId { get; set; } // مهم عشان الـ Edit Form
        public string LocationName { get; set; }
        public string LocationBarcode { get; set; } // لو بتستخدمه للعرض

        // --- Assignment Info ---
        // Nullable عشان لو في المخزن
        public Guid? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }

        // --- Category Info ---
        public int CategoryId { get; set; } // مهم عشان الـ Edit Form
        public string CategoryName { get; set; }

        // --- Manufacturer Info ---
        public int? ManufacturerId { get; set; }
        public string? ManufacturerName { get; set; } // تم تصحيح الاسم

        public ICollection<string> SupplierNames { get; set; } = new List<string> ( );

        public DateTime AddedOnDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // --- Stock Info ---
        public int Quantity { get; set; }
        public int? MinQuantityLimit { get; set; }
    }
}