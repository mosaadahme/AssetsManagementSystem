//namespace AssetsManagementSystem.Models.DbSets.Temp
//{
//    public class MaintenanceRequest : BaseWithAuditEntity
//    {
//        public int AssetId { get; set; }
//        public Asset Asset { get; set; }
//        public string RequesterId { get; set; } // الموظف اللي بلغ
//        public string? TechnicianId { get; set; } // الفني المسؤول (Doer)
//        public string ProblemDescription { get; set; }
//        public DateTime RequestDate { get; set; }
//        public string? PhotoUrl { get; set; }
//        public bool NeedReplacement { get; set; }
//        public RequestStatus Status { get; set; }

//        // بيانات الإغلاق (Closure)
//        public decimal? FinalCost { get; set; }
//        public int? VendorId { get; set; }
//        public Supplier Vendor { get; set; }
//        public DateTime? OutDate { get; set; }   // تاريخ خروج الجهاز للإصلاح
//        public DateTime? ReturnDate { get; set; } // تاريخ العودة
//    }
//}
