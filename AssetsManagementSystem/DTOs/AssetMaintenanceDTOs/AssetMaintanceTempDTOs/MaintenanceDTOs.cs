using AssetsManagementSystem.Models.Enums.AssetsManagementSystem.Models.Enums;

namespace AssetsManagementSystem.DTOs.AssetMaintenanceDTOs.AssetMaintanceTempDTOs
{
    public class MaintenanceDTOs
    {
    }
    public class CreateMaintenancePlanRequestDTO
    {
        public MaintenanceTargetLevel TargetLevel { get; set; }
        public int TargetId { get; set; }
        public Periodicity Periodicity { get; set; }
        public int ActionTypeId { get; set; }
        public int? PreferredVendorId { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class ExecuteMaintenanceDTO
    {
        public int ScheduleId { get; set; }
        public MaintenanceStatus Status { get; set; }
        public decimal Cost { get; set; }
        public string? Notes { get; set; }
        public DateTime? RescheduledDate { get; set; }
    }

    public class SubmitRepairRequestDTO
    {
        public int AssetId { get; set; }
        public string ProblemDescription { get; set; }
        public string? PhotoUrl { get; set; }
        public bool NeedReplacement { get; set; }
    }

    public class ReviewRequestDTO
    {
        public int RequestId { get; set; }
        public RequestStatus Decision { get; set; }
        public string? TechnicianId { get; set; }
        public string? Notes { get; set; }
    }

    public class CloseRepairDTO
    {
        public int RequestId { get; set; }
        public decimal Cost { get; set; }
        public int? VendorId { get; set; }
        public DateTime OutDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Details { get; set; }
    }
}
