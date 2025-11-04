namespace AssetsManagementSystem.Models.DbSets
{
    public class ReceiveMaintainedAsset: IBaseEntityForGeneric
    {
 
        public int Id { get; set; }


        [Required(ErrorMessage = "DateOfRecieve is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [PastOrPresentDate(ErrorMessage = "Date Of Recieve cannot be in the future.")]
        public DateOnly DateOfRecieve { get; set; }


        [ForeignKey("Asset")]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; }


        [Required(ErrorMessage = "UserRecieveDevFromSupplierId is required.")]
        [ForeignKey("UserRecieveDevFromSupplier")]
        public Guid UserRecieveDevFromSupplierId { get; set; }
        public virtual User UserRecieveDevFromSupplier { get; set; }


        
        [ForeignKey("NewUserAssigned")]
        public Guid NewUserAssignedId { get; set; }
        public virtual User NewUserAssigned { get; set; }


        [ForeignKey("Location")]
        public int NewLocationAssigned { get; set; }
        public virtual Location Location { get; set; }

       
        
        [ForeignKey("Document")]
        public int? DocumentId { get; set; }
        public virtual Document ? Document { get; set; }
    }
}
