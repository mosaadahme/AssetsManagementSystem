using Newtonsoft.Json.Converters;

namespace AssetsManagementSystem.DTOs.AssetDTOs
{
    public interface IAssetDTO
    {
        [Required(ErrorMessage = "Asset name is required.")]
        [MaxLength(100, ErrorMessage = "Asset name cannot exceed 100 characters.")]
        public string Name { get; set; }



        //[Required(ErrorMessage = "Model number is required.")]
        [MaxLength(50, ErrorMessage = "Model number cannot exceed 50 characters.")]
        public string ?  ModelNumber { get; set; }


        [Required(ErrorMessage = "Purchase date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateOnly PurchaseDate { get; set; }



        [Required(ErrorMessage = "Purchase price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be a positive value.")]
        public decimal PurchasePrice { get; set; }



        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateOnly WarrantyExpiryDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(AssetStatus.Available)]
        public AssetStatus Status { get; set; }



         public int? LocationId { get; set; }



        [Required(ErrorMessage = "Assigned user is required.")]
        public Guid AssignedUserId { get; set; }



        [Required(ErrorMessage = "Category is required.")]
        public string CategoryId { get; set; }

        public ICollection<int> SupplierIds { get; set; }
    }
}
