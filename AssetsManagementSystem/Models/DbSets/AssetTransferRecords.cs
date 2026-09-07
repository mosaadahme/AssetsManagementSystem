namespace AssetsManagementSystem.Models.DbSets
{
    public class AssetTransferRecords:BaseWithAuditEntity
    {
        [ForeignKey("Asset")]
        [Required(ErrorMessage = "Asset ID is required.")]
        public int AssetId { get; set; }   
        public virtual Asset Asset { get; set; }


        [ForeignKey("FromUser")]
        //[Required(ErrorMessage = "From User ID is required.")]
        public Guid ?FromUserId { get; set; }  
        public virtual User? FromUser { get; set; }


        [ForeignKey("ToUser")]
        //[Required(ErrorMessage = "To User ID is required.")]

        public Guid? ToUserId { get; set; }   
        public virtual User? ToUser { get; set; }


        [ForeignKey("FromLocation")]
        //[Required(ErrorMessage = "From Location ID is required.")]

        public int? FromLocationId { get; set; }    
        public virtual Location? FromLocation { get; set; }


        [ForeignKey("ToLocation")]
        //[Required(ErrorMessage = "To Location ID is required.")]
        public int ?ToLocationId { get; set; }   
        public virtual Location ToLocation { get; set; }


        //[Required(ErrorMessage = "Transfer date is required.")]
        //[DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        //[PastOrPresentDate(ErrorMessage = "Transfer date cannot be in the future.")]
        //public DateTime TransferDate { get; set; }



        [Required(ErrorMessage = "Transfer status is required.")]
        public string Status { get; set; }


        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [PastOrPresentDate(ErrorMessage = "Approval date cannot be in the future.")]
        public DateOnly? ApprovalDate { get; set; }


        [MaxLength(1000, ErrorMessage = "Rejection reason cannot exceed 1000 characters.")]
        public string? RejectionReason { get; set; }
        public bool IsUserTransfer { get; set; }

    }
}
