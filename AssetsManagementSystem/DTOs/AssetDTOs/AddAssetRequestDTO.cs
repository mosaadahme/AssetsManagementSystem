using Newtonsoft.Json.Converters;
using System.Drawing;

namespace AssetsManagementSystem.DTOs.AssetDTOs
{
    public class AddAssetRequestDTO:IAssetDTO
    {

        [Required(ErrorMessage = "Asset name is required.")]
        [MaxLength(100, ErrorMessage = "Asset name cannot exceed 100 characters.")]
        public string Name { get; set; }



        [Required(ErrorMessage = "Model number is required.")]
        [MaxLength(50, ErrorMessage = "Model number cannot exceed 50 characters.")]
        public string ModelNumber { get; set; }



        [Required(ErrorMessage = "Serial number is required.")]
        [MaxLength(100, ErrorMessage = "Serial number cannot exceed 100 characters.")]
        public string SerialNumber { get; set; }

        [MaxLength(1000, ErrorMessage = "Model number cannot exceed 50 characters.")]
        public string? dicription { get; set; }

        [Required(ErrorMessage = "Purchase date is required.")]
        [DataType(DataType.Date, ErrorMessage = "A7A.")]//Invalid date format
        [PastOrPresentDate(ErrorMessage = "Purchase date cannot be in the future.")]

        public DateOnly PurchaseDate { get; set; }



        [Required(ErrorMessage = "Purchase price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be a positive value.")]
        public decimal PurchasePrice { get; set; }



        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [FutureDate("PurchaseDate", ErrorMessage = "Warranty expiry date must be after the purchase date.")]
        public DateOnly WarrantyExpiryDate { get; set; }

        
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateOnly DepreciationDate { get; set; }


        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(AssetStatus.Active)]
        public AssetStatus Status { get; set; }



        [Required(ErrorMessage = "Location is required.")]
        public int LocationId { get; set; }



        [Required(ErrorMessage = "Assigned user is required.")]
        public Guid AssignedUserId { get; set; }



        [Required(ErrorMessage = "Category is required.")]
        public string CategoryId { get; set; }


        public int ManufacturerId { get; set; }

        public ICollection<int> SupplierIds { get; set; }

    }
}
