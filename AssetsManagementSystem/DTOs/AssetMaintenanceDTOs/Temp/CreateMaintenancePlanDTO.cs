// إنشاء خطة صيانة دورية
using AssetsManagementSystem.Models.Enums.AssetsManagementSystem.Models.Enums;

public class CreateMaintenancePlanDTO
{
    public MaintenanceTargetLevel TargetLevel { get; set; }
    public int TargetId { get; set; }
    public Periodicity Periodicity { get; set; }
    public int ActionTypeId { get; set; }
    public int? PreferredVendorId { get; set; }
    public DateOnly StartDate { get; set; }
}

// تنفيذ الصيانة (التحديث من الجدول الزمني)
public class ExecuteMaintenanceDTO
{
    public int ScheduleId { get; set; }
    public MaintenanceStatus Status { get; set; }
    public decimal Cost { get; set; }
    public string? Notes { get; set; }
    public DateOnly? NewRescheduledDate { get; set; } // لو الحالة Rescheduled
    public DateTime ?RescheduledDate { get; set; }
}
// بلاغ عطل جديد
public class SubmitRepairRequestDTO
{
    public int AssetId { get; set; }
    public string AssetBarcode { get; set; }
    public string ProblemDescription { get; set; }
    public string? PhotoBase64 { get; set; } // أو IFormFile حسب تعاملك مع الملفات
    public bool NeedReplacement { get; set; }
}

// مراجعة المسؤول (Triage)
public class ReviewRequestDTO
{
    public int RequestId { get; set; }
    public RequestStatus Decision { get; set; } // Approve, Reject, InfoRequired
    public string? TechnicianId { get; set; }   // مطلوب لو تم القبول
    public string? Reason { get; set; }         // لو تم الرفض

    public string? Notes { get; set; }
}

// إغلاق الطلب (Closure)
public class CloseRepairRequestDTO
{
    public decimal FinalCost { get; set; }
    public int? VendorId { get; set; }
    public DateTime OutDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? TechnicianNotes { get; set; }
}