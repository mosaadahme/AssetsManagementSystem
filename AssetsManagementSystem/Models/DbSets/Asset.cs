namespace AssetsManagementSystem.Models.DbSets
{
    public class Asset:BaseWithAuditEntity
    {
        //[Required(ErrorMessage = "Asset name is required.")]
        //[MaxLength(100, ErrorMessage = "Asset name cannot exceed 100 characters.")]
        //public string Name { get; set; }

        //[Required]
        //[MaxLength ( 100 )]
        //public string Barcode { get; set; }

        //public AssetType AssetType { get; set; } // IT, NonIT, Consumable


        //[Required(ErrorMessage = "Model number is required.")]
        //[MaxLength(50, ErrorMessage = "Model number cannot exceed 50 characters.")]
        //public string ModelNumber { get; set; }


        //[Required(ErrorMessage = "Serial number is required.")]
        //[MaxLength(100, ErrorMessage = "Serial number cannot exceed 100 characters.")]  
        //public string? SerialNumber { get; set; }


        //[Required(ErrorMessage = "Purchase date is required.")]
        //[DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        //[PastOrPresentDate(ErrorMessage = "Purchase date cannot be in the future.")]
        //public DateOnly PurchaseDate { get; set; }


        //[Required(ErrorMessage = "Purchase price is required.")]
        //[Range(0, double.MaxValue, ErrorMessage = "Purchase price must be a positive value.")]
        //public decimal PurchasePrice { get; set; }

        //[DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        //[FutureDate( "WarrantyExpiryDate", ErrorMessage = "Warranty expiry date must be after the purchase date.")]
        //public DateOnly ? WarrantyExpiryDate { get; set; }


        //[DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        //public DateOnly? DepreciationDate { get; set; }


        //[Required(ErrorMessage = "Asset status is required.")]
        //public string Status { get; set; }

        //[MaxLength(1000, ErrorMessage = "Model number cannot exceed 50 characters.")]
        //public string? Description { get; set; }


        //[Required(ErrorMessage = "Location is required.")]
        //[ForeignKey("Location")]
        //public int LocationId { get; set; }   
        //public virtual Location Location { get; set; }


        // [ForeignKey("AssignedUser")]
        //public Guid? AssignedUserId { get; set; }  
        //public virtual  User ? AssignedUser { get; set; }


        //[ForeignKey("Category")]
        //[Required(ErrorMessage = "Category is required.")]
        //public int CategoryId { get; set; }   
        //public virtual Category Category { get; set; }


        //[ForeignKey("Manufacturer")]
        //public int ? ManufacturerId { get; set; }
        //public virtual Manufacturer ?  Manufacturer { get; set; }

        //public virtual ICollection<AssetsSuppliers> AssetsSuppliers { get; set; }  
        //public virtual ICollection<Document> Documents { get; set; }   
        //public virtual ICollection<AssetDisposalRecord> AssetDisposalRecords { set; get; }
        //public virtual ICollection<AssetMaintenanceRecords> AssetMaintenanceRecords { set; get; }
        //public virtual ICollection<AssetTransferRecords>  AssetTransferRecords { set; get; }


        //[Required ( ErrorMessage = "Quantity is required." )]
        //[Range ( 0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative value." )]
        //public int Quantity { get; set; }=1;




        //[Range ( 0, int.MaxValue, ErrorMessage = "Minimum quantity limit must be a non-negative value." )]
        //public int? MinQuantityLimit { get; set; }


        [Required ( ErrorMessage = "Asset name is required." )]
        [MaxLength ( 100, ErrorMessage = "Asset name cannot exceed 100 characters." )]
        public string Name { get; set; }

        [Required]
        [MaxLength ( 100 )]
        // يفضل إضافة [Index(IsUnique = true)] هنا أو في الـ FluentAPI
        public string Barcode { get; set; }

        [Required]
        public AssetType AssetType { get; set; } // IT, NonIT, Consumable

        [Required ( ErrorMessage = "Model number is required." )]
        [MaxLength ( 50, ErrorMessage = "Model number cannot exceed 50 characters." )]
        public string ModelNumber { get; set; }

        // Nullable: عشان يقبل Null في حالة الـ Consumables والـ Non-IT
        [MaxLength ( 100, ErrorMessage = "Serial number cannot exceed 100 characters." )]
        public string? SerialNumber { get; set; }

        [Required ( ErrorMessage = "Purchase date is required." )]
        [DataType ( DataType.Date )]
        [PastOrPresentDate ( ErrorMessage = "Purchase date cannot be in the future." )]
        public DateOnly PurchaseDate { get; set; }

        [Required ( ErrorMessage = "Purchase price is required." )]
        [Range ( 0, double.MaxValue, ErrorMessage = "Purchase price must be a positive value." )]
        public decimal PurchasePrice { get; set; }

        [DataType ( DataType.Date )]
        [FutureDate ( "PurchaseDate", ErrorMessage = "Warranty expiry date must be after the purchase date." )]
        public DateOnly? WarrantyExpiryDate { get; set; }

        [DataType ( DataType.Date )]
        public DateOnly? DepreciationDate { get; set; }

        [Required ( ErrorMessage = "Asset status is required." )]
        public string Status { get; set; }

        // --- تم التصحيح هنا ---
        [MaxLength ( 1000, ErrorMessage = "Description cannot exceed 1000 characters." )]
        public string? Description { get; set; }

        // Relationships
         [ForeignKey ( "Location" )]
        public int? LocationId { get; set; }
        public virtual Location? Location { get; set; }

        [ForeignKey ( "AssignedUser" )]
        public Guid? AssignedUserId { get; set; }
        public virtual User? AssignedUser { get; set; }

        [Required ( ErrorMessage = "Category is required." )]
        [ForeignKey ( "Category" )]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        [ForeignKey ( "Manufacturer" )]
        public int? ManufacturerId { get; set; }
        public virtual Manufacturer? Manufacturer { get; set; }

        // Collections
        public virtual ICollection<AssetsSuppliers> AssetsSuppliers { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<AssetDisposalRecord> AssetDisposalRecords { set; get; }
        public virtual ICollection<AssetMaintenanceRecords> AssetMaintenanceRecords { set; get; }
        public virtual ICollection<AssetTransferRecords> AssetTransferRecords { set; get; }

        // Stock Management
        [Required ( ErrorMessage = "Quantity is required." )]
        [Range ( 0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative value." )]
        public int Quantity { get; set; } = 1; // Default 1 for Fixed Assets

        // Nullable: عشان مش هنحتاجه مع اللابتوبات والكراسي
        [Range ( 0, int.MaxValue, ErrorMessage = "Minimum quantity limit must be a non-negative value." )]
        public int? MinQuantityLimit { get; set; }


    }
}
