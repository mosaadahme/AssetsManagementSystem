namespace AssetsManagementSystem.Models.DbSets
{
    public class AuditTrail : IBaseEntityForGeneric
    {
        public int Id { get; set; }

        public string EntityType { get; set; }

        public string EntityName { get; set; } = string.Empty;

        public string Action { get; set; }
        public DateTime AddedOn { get; set; }

        public string UserId { get; set; }


    }
}
