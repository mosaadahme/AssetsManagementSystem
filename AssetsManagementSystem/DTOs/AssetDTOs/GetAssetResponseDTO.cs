namespace AssetsManagementSystem.DTOs.AssetDTOs
{
    public class GetAssetResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public string? dicription { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateOnly? WarrantyExpiryDate { get; set; }
         public DateOnly DepreciationDate { get; set; }

        public string Status { get; set; }
        public string LocationBarcode { get; set; }
        public string LocationName { get; set; }
        public string UserId { get; set; }

        public string AssignedUserName { get; set; }
        public string CategoryName { get; set; }
        public string ManfactureName { get; set; }

        public ICollection<string> SupplierNames { get; set; }
        public DateTime AddedOnDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
