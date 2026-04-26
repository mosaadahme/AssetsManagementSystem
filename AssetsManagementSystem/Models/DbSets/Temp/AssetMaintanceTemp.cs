using AssetsManagementSystem.Models.Enums.AssetsManagementSystem.Models.Enums;

namespace AssetsManagementSystem.Models.DbSets.Temp
{
    public class AssetMaintanceTemp
    {
    }

    public class MaintenanceActionType : BaseWithAuditEntity
    {
        public string Name { get; set; } // مثال: Oil Change, Calibration, Software Update
        public string? Description { get; set; }
    }

    public class MaintenancePlan : BaseWithAuditEntity
    {
        public MaintenanceTargetLevel TargetLevel { get; set; }
        public int TargetId { get; set; }
        public Periodicity Periodicity { get; set; }
        public int ActionTypeId { get; set; }
        public MaintenanceActionType ActionType { get; set; }
        public int? PreferredVendorId { get; set; }
        public Supplier PreferredVendor { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<MaintenanceSchedule> Schedules { get; set; }
    }

    public class MaintenanceSchedule : BaseWithAuditEntity
    {
        public int PlanId { get; set; }
        public MaintenancePlan Plan { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ActualExecutionDate { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } // Pending, Done, Rescheduled, Cancelled
        public string? Notes { get; set; }
    }

    public class MaintenanceRequest : BaseWithAuditEntity
    {
        public int AssetId { get; set; }
        public Asset Asset { get; set; }
        public string RequesterId { get; set; }
        public string? TechnicianId { get; set; }
        public string ProblemDescription { get; set; }
        public DateTime RequestDate { get; set; }
        public string? PhotoUrl { get; set; }
        public bool NeedReplacement { get; set; }
        public RequestStatus Status { get; set; }
        public decimal? FinalCost { get; set; }
        public int? VendorId { get; set; }
        public DateTime? OutDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? MaintenanceNotes { get; set; }
    }

}
