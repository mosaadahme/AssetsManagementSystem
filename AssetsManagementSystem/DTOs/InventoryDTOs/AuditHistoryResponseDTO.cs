namespace AssetsManagementSystem.DTOs.InventoryDTOs
{
    public class AuditHistoryResponseDTO
    {
        public int AuditId { get; set; }
        public string LocationName { get; set; }
        public string AuditorName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }

        // إحصائيات سريعة
        public int TotalScannedItems { get; set; }
        public bool HasDiscrepancies { get; set; } // هل كان فيه عجز أو زيادة؟
    }
}
