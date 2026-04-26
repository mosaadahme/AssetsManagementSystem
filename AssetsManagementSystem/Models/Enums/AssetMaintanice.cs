namespace AssetsManagementSystem.Models.Enums
{
    public class AssetMaintanice
    {
    }
    //    public enum MaintenanceTargetLevel { Asset, Category, Model }
    //    public enum Periodicity { Weekly, Monthly, Quarterly, SemiAnnually, Annually }
    //    public enum MaintenanceStatus { Pending, Done, Rescheduled, Cancelled }
    //    public enum RequestStatus { PendingReview, Approved, Rejected, InfoRequired, InProgress, Completed }

    namespace AssetsManagementSystem.Models.Enums
    {
        public enum MaintenanceTargetLevel { Asset = 1, Category = 2, Model = 3 }
        public enum Periodicity { Weekly = 1, Monthly = 2, Quarterly = 3, SemiAnnually = 4, Annually = 5 }
        public enum MaintenanceStatus { Pending, Done, Rescheduled, Cancelled }
        public enum RequestStatus { PendingReview, Approved, Rejected, InfoRequired, InProgress, Completed }
    }
}
