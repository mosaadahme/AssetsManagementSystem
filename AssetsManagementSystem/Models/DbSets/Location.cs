namespace AssetsManagementSystem.Models.DbSets
{
    [Index ( nameof ( Barcode ), IsUnique = true )] // ضمان عدم تكرار باركود المكان
    public class Location : BaseWithAuditEntity
    {
        [Required ( ErrorMessage = "Barcode is required." )]
        [MaxLength ( 100 )]
        public string Barcode { get; set; }

        [Required ( ErrorMessage = "Location name is required." )]
        [MaxLength ( 200 )]
        public string Name { get; set; }

        [Required ( ErrorMessage = "Address is required." )]
        [MaxLength ( 500 )]
        public string Address { get; set; }

        public virtual ICollection<Asset> Assets { get; set; }
    }
}
