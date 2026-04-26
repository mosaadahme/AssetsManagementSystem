using AssetsManagementSystem.DTOs.AssetSearchDTOs;
using AssetsManagementSystem.Models.DbSets;
using AssetsManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AssetsManagementSystem.Services.Report
{
    public class ReportService : BaseClassForServices
    {
        public ReportService ( IUnitOfWork unitOfWork,
            Others.Interfaces.IAutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor )
            : base ( unitOfWork, mapper, httpContextAccessor )
        {
        }

        #region Search Assets (Single Criteria)
        /// <summary>
        /// Search for assets based on a single field criteria (Basic Search)
        /// </summary>
        public async Task<IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO>> SearchAssetsAsync ( AssetSearchCriteria criteria )
        {
            if ( criteria == null ) throw new ArgumentNullException ( nameof ( criteria ), "Search criteria cannot be null." );

            var predicate = BuildSearchPredicate ( criteria );

            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync (
                    predicate: predicate,
                    include: source => source
                        .Include ( x => x.Location )
                        .Include ( x => x.AssignedUser )
                        .Include ( x => x.Category )
                        .Include ( x => x.Manufacturer )
                        .Include ( x => x.AssetsSuppliers ).ThenInclude ( s => s.Supplier )
                );

            return MapToResponseDTO ( assets );
        }
        #endregion

        #region Advanced Search (Multiple Criteria & Pagination)
        /// <summary>
        /// Advanced search with multiple criteria and pagination
        /// </summary>
        public async Task<(IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO> Assets, int TotalCount)>
            AdvancedSearchAssetsAsync ( AdvancedAssetSearchCriteria criteria, int currentPage = 1, int pageSize = 10 )
        {
            if ( criteria == null ) throw new ArgumentNullException ( nameof ( criteria ), "Search criteria cannot be null." );

            var predicate = BuildAdvancedSearchPredicate ( criteria );

            // 1. Get Total Count (for pagination metadata)
            var totalCount = await UnitOfWork.readRepository<Asset> ( ).CountAsync ( predicate );

            // 2. Get Paginated Data
            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllByPagningAsync (
                    predicate: predicate,
                    currentPage: currentPage,
                    pageSize: pageSize,
                    include: source => source
                        .Include ( x => x.Location )
                        .Include ( x => x.AssignedUser )
                        .Include ( x => x.Category )
                        .Include ( x => x.Manufacturer )
                );

            return (MapToResponseDTO ( assets ), totalCount);
        }
        #endregion

        #region Reports (Status, Low Stock, Warranty, etc.)

        public async Task<IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO>> GetAssetsByStatusReportAsync ( string status )
        {
            if ( string.IsNullOrWhiteSpace ( status ) ) throw new ArgumentException ( "Status required." );

            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync (
                    predicate: a => a.Status == status && ( a.IsDeleted == false || a.IsDeleted == null ),
                    include: a => a.Include ( x => x.Location ).Include ( x => x.AssignedUser ).Include ( x => x.Category ).Include ( x => x.Manufacturer )
                );

            return MapToResponseDTO ( assets );
        }

        public async Task<IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO>> GetLowStockAssetsReportAsync ( )
        {
            // Low stock applies mainly to Consumables where Quantity <= MinLimit
            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync (
                    predicate: a => a.Quantity <= a.MinQuantityLimit && ( a.IsDeleted == false || a.IsDeleted == null ),
                    include: a => a.Include ( x => x.Location ).Include ( x => x.Category )
                );

            return MapToResponseDTO ( assets );
        }

        public async Task<IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO>> GetExpiredWarrantyAssetsReportAsync ( )
        {
            var today = DateOnly.FromDateTime ( DateTime.Now );

            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync (
                    predicate: a => a.WarrantyExpiryDate < today && ( a.IsDeleted == false || a.IsDeleted == null ),
                    include: a => a.Include ( x => x.Location ).Include ( x => x.Category ).Include ( x => x.Manufacturer )
                );

            return MapToResponseDTO ( assets );
        }

        public async Task<IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO>> GetAssetsByCategoryReportAsync ( int categoryId )
        {
            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync (
                    predicate: a => a.CategoryId == categoryId && ( a.IsDeleted == false || a.IsDeleted == null ),
                    include: a => a.Include ( x => x.Location ).Include ( x => x.AssignedUser ).Include ( x => x.Category )
                );

            return MapToResponseDTO ( assets );
        }

        public async Task<IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO>> GetAssetsByLocationReportAsync ( int locationId )
        {
            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync (
                    predicate: a => a.LocationId == locationId && ( a.IsDeleted == false || a.IsDeleted == null ),
                    include: a => a.Include ( x => x.Location ).Include ( x => x.AssignedUser ).Include ( x => x.Category )
                );

            return MapToResponseDTO ( assets );
        }

        public async Task<IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO>> GetAssetsByUserReportAsync ( Guid userId )
        {
            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync (
                    predicate: a => a.AssignedUserId == userId && ( a.IsDeleted == false || a.IsDeleted == null ),
                    include: a => a.Include ( x => x.Location ).Include ( x => x.Category )
                );

            return MapToResponseDTO ( assets );
        }

        public async Task<AssetSummaryReportDTO> GetAssetsSummaryReportAsync ( )
        {
            // For summary, we don't need tracking or includes (Optimization)
            var allAssets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync ( predicate: a => a.IsDeleted == false || a.IsDeleted == null, enableTracing: false );

            var today = DateOnly.FromDateTime ( DateTime.Now );

            return new AssetSummaryReportDTO
            {
                TotalAssets = allAssets.Count,
                // Calculate value: Price * Quantity (important for bulk items)
                TotalValue = allAssets.Sum ( a => a.PurchasePrice??0 * a.Quantity ),

                ActiveAssets = allAssets.Count ( a => a.Status == AssetStatus.InUse.ToString ( ) ), // Adjusted to Enum String
                AvailableAssets = allAssets.Count ( a => a.Status == AssetStatus.Available.ToString ( ) ),
                UnderMaintenanceAssets = allAssets.Count ( a => a.Status == AssetStatus.UnderMaintenance.ToString ( ) ),
                RetiredAssets = allAssets.Count ( a => a.Status == AssetStatus.Retired.ToString ( ) ),

                LowStockAssets = allAssets.Count ( a => a.MinQuantityLimit.HasValue && a.Quantity <= a.MinQuantityLimit ),
                ExpiredWarrantyAssets = allAssets.Count ( a => a.WarrantyExpiryDate.HasValue && a.WarrantyExpiryDate < today )
            };
        }

        #endregion

        #region Private Helpers (Smart Predicate Builders)

        // Helper to combine predicates (AND logic) without Expression.Invoke issues
        private static Expression<Func<T, bool>> And<T> ( Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2 )
        {
            var parameter = Expression.Parameter ( typeof ( T ) );
            var body = Expression.AndAlso (
                Expression.Invoke ( expr1, parameter ),
                Expression.Invoke ( expr2, parameter )
            );
            return Expression.Lambda<Func<T, bool>> ( body, parameter );
        }

        private Expression<Func<Asset, bool>> BuildSearchPredicate ( AssetSearchCriteria criteria )
        {
            Expression<Func<Asset, bool>> predicate = a => ( a.IsDeleted == false || a.IsDeleted == null );

            if ( !string.IsNullOrWhiteSpace ( criteria.SearchTerm ) )
            {
                // Generic search across multiple fields (Name, Barcode, Serial)
                Expression<Func<Asset, bool>> search = a =>
                    a.Name.Contains ( criteria.SearchTerm ) ||
                    a.Barcode.Contains ( criteria.SearchTerm ) ||
                    ( a.SerialNumber != null && a.SerialNumber.Contains ( criteria.SearchTerm ) );

                predicate = And ( predicate, search );
            }

            // ... Add specific single criteria if needed ...
            return predicate;
        }

        private Expression<Func<Asset, bool>> BuildAdvancedSearchPredicate ( AdvancedAssetSearchCriteria criteria )
        {
            Expression<Func<Asset, bool>> predicate = a => ( a.IsDeleted == false || a.IsDeleted == null );

            if ( !string.IsNullOrWhiteSpace ( criteria.Name ) )
                predicate = And ( predicate, a => a.Name.Contains ( criteria.Name ) );

            if ( !string.IsNullOrWhiteSpace ( criteria.Barcode ) )
                predicate = And ( predicate, a => a.Barcode.Contains ( criteria.Barcode ) );

            if ( !string.IsNullOrWhiteSpace ( criteria.SerialNumber ) )
                predicate = And ( predicate, a => a.SerialNumber != null && a.SerialNumber.Contains ( criteria.SerialNumber ) );

            if ( !string.IsNullOrWhiteSpace ( criteria.ModelNumber ) )
                predicate = And ( predicate, a => a.ModelNumber.Contains ( criteria.ModelNumber ) );

            if ( !string.IsNullOrWhiteSpace ( criteria.Status ) )
                predicate = And ( predicate, a => a.Status == criteria.Status );

            if ( criteria.CategoryId.HasValue )
                predicate = And ( predicate, a => a.CategoryId == criteria.CategoryId );

            if ( criteria.LocationId.HasValue )
                predicate = And ( predicate, a => a.LocationId == criteria.LocationId );

            if ( criteria.AssignedUserId.HasValue )
                predicate = And ( predicate, a => a.AssignedUserId == criteria.AssignedUserId );

            if ( criteria.ManufacturerId.HasValue )
                predicate = And ( predicate, a => a.ManufacturerId == criteria.ManufacturerId );

            // Ranges
            if ( criteria.MinPrice.HasValue )
                predicate = And ( predicate, a => a.PurchasePrice >= criteria.MinPrice );

            if ( criteria.MaxPrice.HasValue )
                predicate = And ( predicate, a => a.PurchasePrice <= criteria.MaxPrice );

            if ( criteria.PurchaseDateFrom.HasValue )
                predicate = And ( predicate, a => a.PurchaseDate >= criteria.PurchaseDateFrom );

            if ( criteria.PurchaseDateTo.HasValue )
                predicate = And ( predicate, a => a.PurchaseDate <= criteria.PurchaseDateTo );

            return predicate;
        }

        private IEnumerable<DTOs.AssetDTOs.GetAssetResponseDTO> MapToResponseDTO ( IEnumerable<Asset> assets )
        {
            return assets.Select ( a => new DTOs.AssetDTOs.GetAssetResponseDTO
            {
                Id = a.Id,
                Name = a.Name,
                Barcode = a.Barcode,
                AssetType = a.AssetType.ToString ( ),
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                Description = a.Description, // Fixed typo
                PurchaseDate = a.PurchaseDate??new DateOnly(),
                PurchasePrice = a.PurchasePrice??0,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                DepreciationDate = a.DepreciationDate,
                Status = a.Status,

                LocationId = a.LocationId??0,
                LocationName = a.Location?.Name ?? "Unknown",
                LocationBarcode = a.Location?.Barcode ?? "",

                AssignedUserId = a.AssignedUserId,
                AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : "In Stock",

                CategoryId = a.CategoryId,
                CategoryName = a.Category?.Name ?? "Unknown",

                ManufacturerId = a.ManufacturerId,
                ManufacturerName = a.Manufacturer?.Name ?? "N/A",

                Quantity = a.Quantity,
                MinQuantityLimit = a.MinQuantityLimit,
                AddedOnDate = a.AddedOnDate,
                UpdatedDate = a.UpdatedDate
            } );
        }

        #endregion
    }
}