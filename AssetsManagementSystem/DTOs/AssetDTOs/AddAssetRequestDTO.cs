using AssetsManagementSystem.Models.Enums; // تأكد من الـ Namespace للـ Enum
using System.ComponentModel.DataAnnotations;
 
namespace AssetsManagementSystem.DTOs.AssetDTOs
{
    public class AddAssetRequestDTO
    {
        [Required ( ErrorMessage = "Asset name is required." )]
        [MaxLength ( 100, ErrorMessage = "Asset name cannot exceed 100 characters." )]
        public string Name { get; set; }

        [Required ( ErrorMessage = "Model number is required." )]
        [MaxLength ( 50, ErrorMessage = "Model number cannot exceed 50 characters." )]
        public string ModelNumber { get; set; }

        //// 1. السيريال الفردي (لو كمية 1)
        //[MaxLength ( 100, ErrorMessage = "Serial number cannot exceed 100 characters." )]
        //public string? SerialNumber { get; set; }

        // 2. إضافة جديدة: قائمة سيريالات (لو الكمية > 1 ونوع IT)
        public List<string>? SerialNumbers { get; set; }

        // 3. تصحيح الاسم ورسالة الخطأ
        [MaxLength ( 1000, ErrorMessage = "Description cannot exceed 1000 characters." )]
        public string? Description { get; set; }

        [Required ( ErrorMessage = "Purchase date is required." )]
        [DataType ( DataType.Date )]
        [PastOrPresentDate ( ErrorMessage = "Purchase date cannot be in the future." )]
        public DateOnly PurchaseDate { get; set; } // تم تعديل رسالة الخطأ 😄

        [Required ( ErrorMessage = "Purchase price is required." )]
        [Range ( 0, double.MaxValue, ErrorMessage = "Purchase price must be a positive value." )]
        public decimal PurchasePrice { get; set; }

        // 4. التواريخ بقت Nullable
        [DataType ( DataType.Date )]
        [FutureDate ( "PurchaseDate", ErrorMessage = "Warranty expiry date must be after purchase date." )]
        public DateOnly? WarrantyExpiryDate { get; set; }

        [DataType ( DataType.Date )]
        public DateOnly? DepreciationDate { get; set; }

        // Enum Validation
        [EnumDataType ( typeof ( AssetStatus ) )]
        public AssetStatus Status { get; set; } = AssetStatus.Active;  
         
        [Required ( ErrorMessage = "Location is required." )]
        public int LocationId { get; set; }

        // 5. بقت Nullable عشان المخزن
        public Guid? AssignedUserId { get; set; }

        [Required ( ErrorMessage = "Category is required." )]
        public int CategoryId { get; set; } // خليناها int عشان الـ FK

        public int? ManufacturerId { get; set; } // Nullable

        public List<int>? SupplierIds { get; set; } // للربط مع الموردين

        [Required ( ErrorMessage = "Quantity is required." )]
        [Range ( 1, int.MaxValue, ErrorMessage = "Quantity must be at least 1." )]
        public int Quantity { get; set; } = 1;

        // 6. الحد الأدنى اختياري
        [Range ( 0, int.MaxValue )]
        public int? MinQuantityLimit { get; set; }
    }
}