using AssetsManagementSystem.Services.Manfacture;

namespace AssetsManagementSystem.Services
{
    public static class Registeration
    {
        public static void AddFolderOfServices(this IServiceCollection services)
        {
             services.AddScoped<AuthRules>();

             services.AddScoped<Accounting>();

             services.AddScoped<LocationService>();

            services.AddScoped<CategoryService>();

            services.AddScoped<SupplierService>();

            services.AddScoped<AssetService>();

            services.AddScoped<AssetMaintenanceService>();

            services.AddScoped<AssetDisposalService>();

            services.AddScoped<AssetTransferService>();

            services.AddScoped<DocumentService>();

            services.AddScoped<ManfactureService>();

            services.AddScoped<ReceiveMaintainedAssetService>();

            services.AddScoped<AuditTrailService>();

            services.AddScoped<DataConsistencyCheckService>();
        }
    }
}
