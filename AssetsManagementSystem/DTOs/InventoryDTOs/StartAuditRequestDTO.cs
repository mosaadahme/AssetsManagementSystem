namespace AssetsManagementSystem.DTOs.InventoryDTOs
{
    public class StartAuditRequestDTO
    {
        [Required ( ErrorMessage = "Location ID is required." )]
        public int LocationId { get; set; }
    }
}
