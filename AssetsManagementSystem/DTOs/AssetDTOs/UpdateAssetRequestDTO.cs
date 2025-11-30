using System.ComponentModel.DataAnnotations;
 using AssetsManagementSystem.Models.Enums;

namespace AssetsManagementSystem.DTOs.AssetDTOs
{
    public class UpdateAssetRequestDTO
    {
        [Required ( ErrorMessage = "Asset name is required." )]
        [MaxLength ( 100, ErrorMessage = "Asset name cannot exceed 100 characters." )]
        public string Name { get; set; }

        [Required ( ErrorMessage = "Model number is required." )]
        [MaxLength ( 50, ErrorMessage = "Model number cannot exceed 50 characters." )]
        public string ModelNumber { get; set; }

        // السيريال هنا string واحد بس (لأننا بنعدل أصل معين)
        // ومسموح بـ Null للأصول الـ Non-IT
        [MaxLength ( 100, ErrorMessage = "Serial number cannot exceed 100 characters." )]
        public string? SerialNumber { get; set; }

        [MaxLength ( 1000, ErrorMessage = "Description cannot exceed 1000 characters." )]
        public string? Description { get; set; }

        [Required ( ErrorMessage = "Purchase date is required." )]
        [DataType ( DataType.Date )]
        [PastOrPresentDate ( ErrorMessage = "Purchase date cannot be in the future." )]
        public DateOnly PurchaseDate { get; set; }

        [Required ( ErrorMessage = "Purchase price is required." )]
        [Range ( 0, double.MaxValue, ErrorMessage = "Purchase price must be a positive value." )]
        public decimal PurchasePrice { get; set; }

        [DataType ( DataType.Date )]
        [FutureDate ( "PurchaseDate", ErrorMessage = "Warranty expiry date must be after purchase date." )]
        public DateOnly? WarrantyExpiryDate { get; set; }

        [DataType ( DataType.Date )]
        public DateOnly? DepreciationDate { get; set; }

        [EnumDataType ( typeof ( AssetStatus ) )]
        public AssetStatus Status { get; set; }

        [Required ( ErrorMessage = "Location is required." )]
        public int LocationId { get; set; }

        //// Nullable: عشان لو عايز "تسحب" الأصل من الموظف وترجعه المخزن، تبعت null
        public Guid? AssignedUserId { get; set; }

        [Required ( ErrorMessage = "Category is required." )]
        public int CategoryId { get; set; }

        public int? ManufacturerId { get; set; }

        // قائمة الموردين الجدد (هتستبدل القائمة القديمة)
        public List<int>? SupplierIds { get; set; }

        // بنسمح بتعديل حد الطلب (للـ Consumables)
        [Range ( 0, int.MaxValue )]
        public int? MinQuantityLimit { get; set; }
    }
}