namespace AssetsManagementSystem.Data.Context
{
    public class ApplicationDbContext:IdentityDbContext<User,Role,Guid>
    {
       // public IHttpContextAccessor HttpContextAccessor { get; }
        //public string UserId { get; set; }
       
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> optionsBuilder)//, IHttpContextAccessor httpContextAccessor)
        : base(optionsBuilder) 
        {
        //    HttpContextAccessor = httpContextAccessor;
          //  UserId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Server=.;Database=AssetManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
        //        //                        , sqlServerOptions =>
        //        //sqlServerOptions.EnableRetryOnFailure(
        //        //    maxRetryCount: 0, // Number of retry attempts
        //        //    maxRetryDelay: TimeSpan.FromSeconds(10), // Delay between retries
        //        //    errorNumbersToAdd: null // Optional: SQL error numbers to trigger a retry
        //        //)
        //    );
        //}
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
           .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

        }

     

        ////public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        ////{
        ////    base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        ////    var entries =  ChangeTracker.Entries()
        ////       .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified ||
        ////       e.State == EntityState.Deleted).ToList();

        ////    foreach (var entry in entries)
        ////    {
        ////        var auditLog = new AuditLog
        ////        {
        ////            EventType = entry.State.ToString(),
        ////            EntityName = entry.Entity.GetType().Name,
        ////            EntityId  = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString(),
        ////            Changes = GetChanges(entry),
        ////            Timestamp = DateTime.Now,
        ////            PerformedBy = UserId ?? "System",
        ////            IPAddress = HttpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        ////        };


        ////       AuditLogs.Add(auditLog);
        ////    }

 
        ////     return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        ////}

        //private string GetChanges(EntityEntry entry)
        //{
        //    var changes = new StringBuilder();

        //    if (entry.State == EntityState.Added)
        //    {
        //        changes.Append("New entity added.");
        //    }
        //    else if (entry.State == EntityState.Deleted)
        //    {
        //        changes.Append("Entity deleted.");
        //    }
        //    else if (entry.State == EntityState.Modified)
        //    {
        //        foreach (var property in entry.Properties)
        //        {
        //            if (property.IsModified)
        //            {
        //                changes.AppendLine($"{property.Metadata.Name}: {property.OriginalValue} -> {property.CurrentValue}");
        //            }
        //        }
        //    }

        //    return changes.ToString();
        //}

        #region DbSets
        public virtual DbSet<User> users { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<AssetDisposalRecord> AssetDisposalRecords { get; set; }
        public virtual DbSet<AssetMaintenanceRecords> AssetMaintenanceRecords { get; set; }
        public virtual DbSet<AssetTransferRecords>  AssetTransferRecords { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Category> Categories  { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<DataConsistencyCheck> DataConsistencyChecks { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
         public virtual DbSet<Manufacturer> Manufacturers { get; set; }
        public virtual DbSet<ReceiveMaintainedAsset> ReceiveMaintainedAsset { get; set; }
        public virtual DbSet<AuditTrail> AuditTrails { get; set; }
        #endregion
    }
}
