namespace AssetsManagementSystem.DTOs.ReceiveMaintainedDeviceDTOs
{
    public class AddReceiveMaintainedAssetRequest
    {
        public string Title { get; set; }
        public string AssetSerialNumber { get; set; }
        public Guid UserRecieveDevFromSupplierId { get; set; }
    
        public Guid NewUserAssignedId { get; set; }
  
        public int NewLocationAssignedId { get; set; }

        public IFormFile? ReceiveMaintainedAssetDoc { get; set; }
    }
}
