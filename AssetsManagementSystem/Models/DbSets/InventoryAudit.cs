using AssetsManagementSystem.Models.Commons.ICommon;

namespace AssetsManagementSystem.Models.DbSets
{
    public class InventoryAudit : BaseWithAuditEntity
    {
        [ForeignKey ( "Location" )]
        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } 

         public InventoryAuditStatus Status { get; set; } = InventoryAuditStatus.Pending;

        [ForeignKey ( "Auditor" )]
        public Guid AuditorId { get; set; }
        public virtual User Auditor { get; set; }

        public virtual ICollection<InventoryAuditDetail> AuditDetails { get; set; }
    }

     public class InventoryAuditDetail : BaseWithAuditEntity
    {
        [ForeignKey ( "InventoryAudit" )]
        public int InventoryAuditId { get; set; }

         public virtual InventoryAudit InventoryAudit { get; set; }

        public string ScannedBarcode { get; set; }

        public DateTime ScannedAt { get; set; }

        public bool IsMatched { get; set; }
    }

    public enum InventoryAuditStatus
    {
        Pending,
        Completed,
        Cancelled
    }
}
