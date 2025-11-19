
//using AssetsManagementSystem.DTOs.AssetDTOs;
//using AssetsManagementSystem.DTOs.AssetSearchDTOs;
//using AssetsManagementSystem.Models.DbSets;
//using System.Linq.Expressions;

//namespace AssetsManagementSystem.Services.Report
//{
//    public class ReportService : BaseClassForServices
//    {
//        public ReportService ( IUnitOfWork unitOfWork,
//            Others.Interfaces.IAutoMapper.IMapper mapper,
//            IHttpContextAccessor httpContextAccessor )
//            : base ( unitOfWork, mapper, httpContextAccessor )
//        {
//        }

//        #region Search Assets by Single Criteria
//        /// <summary>
//        /// Search for assets based on a single field criteria
//        /// </summary>
//        public async Task<IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO>> SearchAssetsAsync ( AssetSearchCriteria criteria )
//        {
//            if ( criteria == null )
//            {
//                throw new ArgumentNullException ( nameof ( criteria ), "Search criteria cannot be null." );
//            }

//            // Build the predicate dynamically based on criteria
//            Expression<Func<Asset, bool>> predicate = BuildSearchPredicate ( criteria );

//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync (
//                    predicate: predicate,
//                    include: a => a
//                        .Include ( x => x.Location )
//                        .Include ( x => x.AssignedUser )
//                        .Include ( x => x.Category )
//                        .Include ( x => x.Manufacturer )
//                        .Include ( x => x.AssetsSuppliers )
//                            .ThenInclude ( s => s.Supplier )
//                );

//            var result = assets.Select ( a => new DTOs.AssetSearchDTOs.GetAssetResponseDTO
//            {
//                Id = a.Id,
//                Name = a.Name,
//                ModelNumber = a.ModelNumber,
//                SerialNumber = a.SerialNumber,
//                Description = a.dicription,
//                PurchaseDate = a.PurchaseDate,
//                PurchasePrice = a.PurchasePrice,
//                WarrantyExpiryDate = a.WarrantyExpiryDate,
//                DepreciationDate = a.DepreciationDate,
//                Status = a.Status,
//                LocationId = a.LocationId,
//                LocationName = a.Location?.Name,
//                AssignedUserId = a.AssignedUserId,
//                AssignedUserName = a.AssignedUser?.UserName,
//                CategoryId = a.CategoryId,
//                CategoryName = a.Category?.Name,
//                ManufacturerId = a.ManufacturerId,
//                ManufacturerName = a.Manufacturer?.Name,
//                Quantity = a.Quantity,
//                MinQuantityLimit = a.MinQuantityLimit.HasValue?a.MinQuantityLimit:0,
//                AddedOnDate = a.AddedOnDate,
//                UpdatedDate = a.UpdatedDate
//            } );

//            return result;
//        }
//        #endregion

//        #region Search Assets with Advanced Filters
//        /// <summary>
//        /// Advanced search with multiple criteria and pagination
//        /// </summary>
//        public async Task<(IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> Assets, int TotalCount)>
//            AdvancedSearchAssetsAsync ( AdvancedAssetSearchCriteria criteria, int currentPage = 1, int pageSize = 10 )
//        {
//            if ( criteria == null )
//            {
//                throw new ArgumentNullException ( nameof ( criteria ), "Search criteria cannot be null." );
//            }

//            Expression<Func<Asset, bool>> predicate = BuildAdvancedSearchPredicate ( criteria );

//            // Get total count for pagination
//            var allAssets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync ( predicate: predicate );

//            int totalCount = allAssets.Count ( );

//            // Get paginated results
//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllByPagningAsync (
//                    predicate: predicate,
//                    currentPage: currentPage,
//                    pageSize: pageSize,
//                    include: a => a
//                        .Include ( x => x.Location )
//                        .Include ( x => x.AssignedUser )
//                        .Include ( x => x.Category )
//                        .Include ( x => x.Manufacturer )
//                );

//            var result = assets.Select ( a => new DTOs.AssetSearchDTOs.GetAssetResponseDTO
//            {
//                Id = a.Id,
//                Name = a.Name,
//                ModelNumber = a.ModelNumber,
//                SerialNumber = a.SerialNumber,
//                Description = a.dicription,
//                PurchaseDate = a.PurchaseDate,
//                PurchasePrice = a.PurchasePrice,
//                WarrantyExpiryDate = a.WarrantyExpiryDate,
//                DepreciationDate = a.DepreciationDate,
//                Status = a.Status,
//                LocationId = a.LocationId,
//                LocationName = a.Location?.Name,
//                AssignedUserId = a.AssignedUserId,
//                AssignedUserName = a.AssignedUser?.UserName,
//                CategoryId = a.CategoryId,
//                CategoryName = a.Category?.Name,
//                ManufacturerId = a.ManufacturerId,
//                ManufacturerName = a.Manufacturer?.Name,
//                Quantity = a.Quantity,
//                MinQuantityLimit = a.MinQuantityLimit,
//                AddedOnDate = a.AddedOnDate,
//                UpdatedDate = a.UpdatedDate
//            } );

//            return (result, totalCount);
//        }
//        #endregion

//        #region Get Assets by Status Report
//        /// <summary>
//        /// Generate report for assets by status
//        /// </summary>
//        public async Task<IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO>> GetAssetsByStatusReportAsync ( string status )
//        {
//            if ( string.IsNullOrWhiteSpace ( status ) )
//            {
//                throw new ArgumentException ( "Status cannot be null or empty.", nameof ( status ) );
//            }

//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync (
//                    predicate: a => a.Status == status && ( a.IsDeleted == false || a.IsDeleted == null ),
//                    include: a => a
//                        .Include ( x => x.Location )
//                        .Include ( x => x.AssignedUser )
//                        .Include ( x => x.Category )
//                        .Include ( x => x.Manufacturer )
//                );

//            return MapToResponseDTO ( assets );
//        }
//        #endregion

//        #region Get Low Stock Assets Report
//        /// <summary>
//        /// Get assets that are below minimum quantity limit
//        /// </summary>
//        public async Task<IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO>> GetLowStockAssetsReportAsync ( )
//        {
//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync (
//                    predicate: a => a.Quantity <= a.MinQuantityLimit
//                        && ( a.IsDeleted == false || a.IsDeleted == null ),
//                    include: a => a
//                        .Include ( x => x.Location )
//                        .Include ( x => x.AssignedUser )
//                        .Include ( x => x.Category )
//                        .Include ( x => x.Manufacturer )
//                );

//            return MapToResponseDTO ( assets );
//        }
//        #endregion

//        #region Get Expired Warranty Assets Report
//        /// <summary>
//        /// Get assets with expired warranties
//        /// </summary>
//        public async Task<IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO>> GetExpiredWarrantyAssetsReportAsync ( )
//        {
//            var today = DateOnly.FromDateTime ( DateTime.Now );

//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync (
//                    predicate: a => a.WarrantyExpiryDate < today
//                        && ( a.IsDeleted == false || a.IsDeleted == null ),
//                    include: a => a
//                        .Include ( x => x.Location )
//                        .Include ( x => x.AssignedUser )
//                        .Include ( x => x.Category )
//                        .Include ( x => x.Manufacturer )
//                );

//            return MapToResponseDTO ( assets );
//        }
//        #endregion

//        #region Get Assets by Category Report
//        /// <summary>
//        /// Get all assets in a specific category
//        /// </summary>
//        public async Task<IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO>> GetAssetsByCategoryReportAsync ( int categoryId )
//        {
//            if ( categoryId <= 0 )
//            {
//                throw new ArgumentException ( "Invalid category ID.", nameof ( categoryId ) );
//            }

//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync (
//                    predicate: a => a.CategoryId == categoryId
//                        && ( a.IsDeleted == false || a.IsDeleted == null ),
//                    include: a => a
//                        .Include ( x => x.Location )
//                        .Include ( x => x.AssignedUser )
//                        .Include ( x => x.Category )
//                        .Include ( x => x.Manufacturer )
//                );

//            return MapToResponseDTO ( assets );
//        }
//        #endregion

//        #region Get Assets by Location Report
//        /// <summary>
//        /// Get all assets in a specific location
//        /// </summary>
//        public async Task<IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO>> GetAssetsByLocationReportAsync ( int locationId )
//        {
//            if ( locationId <= 0 )
//            {
//                throw new ArgumentException ( "Invalid location ID.", nameof ( locationId ) );
//            }

//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync (
//                    predicate: a => a.LocationId == locationId
//                        && ( a.IsDeleted == false || a.IsDeleted == null ),
//                    include: a => a
//                        .Include ( x => x.Location )
//                        .Include ( x => x.AssignedUser )
//                        .Include ( x => x.Category )
//                        .Include ( x => x.Manufacturer )
//                );

//            return MapToResponseDTO ( assets );
//        }
//        #endregion

//        #region Get Assets by User Report
//        /// <summary>
//        /// Get all assets assigned to a specific user
//        /// </summary>
//        public async Task<IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO>> GetAssetsByUserReportAsync ( Guid userId )
//        {
//            if ( userId == Guid.Empty )
//            {
//                throw new ArgumentException ( "Invalid user ID.", nameof ( userId ) );
//            }

//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync (
//                    predicate: a => a.AssignedUserId == userId
//                        && ( a.IsDeleted == false || a.IsDeleted == null ),
//                    include: a => a
//                        .Include ( x => x.Location )
//                        .Include ( x => x.AssignedUser )
//                        .Include ( x => x.Category )
//                        .Include ( x => x.Manufacturer )
//                );

//            return MapToResponseDTO ( assets );
//        }
//        #endregion

//        #region Get Assets Summary Report
//        /// <summary>
//        /// Get summary statistics for assets
//        /// </summary>
//        public async Task<AssetSummaryReportDTO> GetAssetsSummaryReportAsync ( )
//        {
//            var allAssets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync ( predicate: a => a.IsDeleted == false || a.IsDeleted == null );

//            return new AssetSummaryReportDTO
//            {
//                TotalAssets = allAssets.Count ( ),
//                TotalValue = allAssets.Sum ( a => a.PurchasePrice * a.Quantity ),
//                ActiveAssets = allAssets.Count ( a => a.Status == "Active" ),
//                InactiveAssets = allAssets.Count ( a => a.Status == "Inactive" ),
//                UnderMaintenanceAssets = allAssets.Count ( a => a.Status == "Under Maintenance" ),
//                LowStockAssets = allAssets.Count ( a => a.Quantity <= a.MinQuantityLimit ),
//                ExpiredWarrantyAssets = allAssets.Count ( a => a.WarrantyExpiryDate < DateOnly.FromDateTime ( DateTime.Now ) )
//            };
//        }
//        #endregion

//        #region Private Helper Methods
//        private Expression<Func<Asset, bool>> BuildSearchPredicate ( AssetSearchCriteria criteria )
//        {
//            Expression<Func<Asset, bool>> predicate = a => ( a.IsDeleted == false || a.IsDeleted == null );

//            if ( !string.IsNullOrWhiteSpace ( criteria.Name ) )
//            {
//                Expression<Func<Asset, bool>> namePredicate = a => a.Name.Contains ( criteria.Name );
//                predicate = CombinePredicates ( predicate, namePredicate );
//            }

//            if ( !string.IsNullOrWhiteSpace ( criteria.SerialNumber ) )
//            {
//                Expression<Func<Asset, bool>> serialPredicate = a => a.SerialNumber.Contains ( criteria.SerialNumber );
//                predicate = CombinePredicates ( predicate, serialPredicate );
//            }

//            if ( !string.IsNullOrWhiteSpace ( criteria.ModelNumber ) )
//            {
//                Expression<Func<Asset, bool>> modelPredicate = a => a.ModelNumber.Contains ( criteria.ModelNumber );
//                predicate = CombinePredicates ( predicate, modelPredicate );
//            }

//            if ( !string.IsNullOrWhiteSpace ( criteria.Status ) )
//            {
//                Expression<Func<Asset, bool>> statusPredicate = a => a.Status == criteria.Status;
//                predicate = CombinePredicates ( predicate, statusPredicate );
//            }

//            if ( criteria.CategoryId.HasValue )
//            {
//                Expression<Func<Asset, bool>> categoryPredicate = a => a.CategoryId == criteria.CategoryId.Value;
//                predicate = CombinePredicates ( predicate, categoryPredicate );
//            }

//            if ( criteria.LocationId.HasValue )
//            {
//                Expression<Func<Asset, bool>> locationPredicate = a => a.LocationId == criteria.LocationId.Value;
//                predicate = CombinePredicates ( predicate, locationPredicate );
//            }

//            if ( criteria.AssignedUserId.HasValue )
//            {
//                Expression<Func<Asset, bool>> userPredicate = a => a.AssignedUserId == criteria.AssignedUserId.Value;
//                predicate = CombinePredicates ( predicate, userPredicate );
//            }

//            if ( criteria.ManufacturerId.HasValue )
//            {
//                Expression<Func<Asset, bool>> manufacturerPredicate = a => a.ManufacturerId == criteria.ManufacturerId.Value;
//                predicate = CombinePredicates ( predicate, manufacturerPredicate );
//            }

//            return predicate;
//        }

//        private Expression<Func<Asset, bool>> BuildAdvancedSearchPredicate ( AdvancedAssetSearchCriteria criteria )
//        {
//            Expression<Func<Asset, bool>> predicate = a => ( a.IsDeleted == false || a.IsDeleted == null );

//            // Basic search criteria
//            if ( !string.IsNullOrWhiteSpace ( criteria.Name ) )
//            {
//                Expression<Func<Asset, bool>> namePredicate = a => a.Name.Contains ( criteria.Name );
//                predicate = CombinePredicates ( predicate, namePredicate );
//            }

//            if ( !string.IsNullOrWhiteSpace ( criteria.SerialNumber ) )
//            {
//                Expression<Func<Asset, bool>> serialPredicate = a => a.SerialNumber.Contains ( criteria.SerialNumber );
//                predicate = CombinePredicates ( predicate, serialPredicate );
//            }

//            if ( !string.IsNullOrWhiteSpace ( criteria.ModelNumber ) )
//            {
//                Expression<Func<Asset, bool>> modelPredicate = a => a.ModelNumber.Contains ( criteria.ModelNumber );
//                predicate = CombinePredicates ( predicate, modelPredicate );
//            }

//            if ( !string.IsNullOrWhiteSpace ( criteria.Status ) )
//            {
//                Expression<Func<Asset, bool>> statusPredicate = a => a.Status == criteria.Status;
//                predicate = CombinePredicates ( predicate, statusPredicate );
//            }

//            if ( criteria.CategoryId.HasValue )
//            {
//                Expression<Func<Asset, bool>> categoryPredicate = a => a.CategoryId == criteria.CategoryId.Value;
//                predicate = CombinePredicates ( predicate, categoryPredicate );
//            }

//            if ( criteria.LocationId.HasValue )
//            {
//                Expression<Func<Asset, bool>> locationPredicate = a => a.LocationId == criteria.LocationId.Value;
//                predicate = CombinePredicates ( predicate, locationPredicate );
//            }

//            if ( criteria.AssignedUserId.HasValue )
//            {
//                Expression<Func<Asset, bool>> userPredicate = a => a.AssignedUserId == criteria.AssignedUserId.Value;
//                predicate = CombinePredicates ( predicate, userPredicate );
//            }

//            if ( criteria.ManufacturerId.HasValue )
//            {
//                Expression<Func<Asset, bool>> manufacturerPredicate = a => a.ManufacturerId == criteria.ManufacturerId.Value;
//                predicate = CombinePredicates ( predicate, manufacturerPredicate );
//            }

//            // Price range
//            if ( criteria.MinPrice.HasValue )
//            {
//                Expression<Func<Asset, bool>> minPricePredicate = a => a.PurchasePrice >= criteria.MinPrice.Value;
//                predicate = CombinePredicates ( predicate, minPricePredicate );
//            }

//            if ( criteria.MaxPrice.HasValue )
//            {
//                Expression<Func<Asset, bool>> maxPricePredicate = a => a.PurchasePrice <= criteria.MaxPrice.Value;
//                predicate = CombinePredicates ( predicate, maxPricePredicate );
//            }

//            // Date range for purchase
//            if ( criteria.PurchaseDateFrom.HasValue )
//            {
//                Expression<Func<Asset, bool>> dateFromPredicate = a => a.PurchaseDate >= criteria.PurchaseDateFrom.Value;
//                predicate = CombinePredicates ( predicate, dateFromPredicate );
//            }

//            if ( criteria.PurchaseDateTo.HasValue )
//            {
//                Expression<Func<Asset, bool>> dateToPredicate = a => a.PurchaseDate <= criteria.PurchaseDateTo.Value;
//                predicate = CombinePredicates ( predicate, dateToPredicate );
//            }

//            // Quantity filters
//            if ( criteria.MinQuantity.HasValue )
//            {
//                Expression<Func<Asset, bool>> minQtyPredicate = a => a.Quantity >= criteria.MinQuantity.Value;
//                predicate = CombinePredicates ( predicate, minQtyPredicate );
//            }

//            if ( criteria.MaxQuantity.HasValue )
//            {
//                Expression<Func<Asset, bool>> maxQtyPredicate = a => a.Quantity <= criteria.MaxQuantity.Value;
//                predicate = CombinePredicates ( predicate, maxQtyPredicate );
//            }

//            // Warranty status
//            if ( criteria.WarrantyExpired.HasValue )
//            {
//                var today = DateOnly.FromDateTime ( DateTime.Now );
//                if ( criteria.WarrantyExpired.Value )
//                {
//                    Expression<Func<Asset, bool>> expiredPredicate = a => a.WarrantyExpiryDate < today;
//                    predicate = CombinePredicates ( predicate, expiredPredicate );
//                }
//                else
//                {
//                    Expression<Func<Asset, bool>> validPredicate = a => a.WarrantyExpiryDate >= today;
//                    predicate = CombinePredicates ( predicate, validPredicate );
//                }
//            }

//            // Low stock filter
//            if ( criteria.LowStock.HasValue && criteria.LowStock.Value )
//            {
//                Expression<Func<Asset, bool>> lowStockPredicate = a => a.Quantity <= a.MinQuantityLimit;
//                predicate = CombinePredicates ( predicate, lowStockPredicate );
//            }

//            return predicate;
//        }

//        private Expression<Func<Asset, bool>> CombinePredicates (
//            Expression<Func<Asset, bool>> first,
//            Expression<Func<Asset, bool>> second )
//        {
//            var parameter = Expression.Parameter ( typeof ( Asset ), "a" );
//            var combined = Expression.AndAlso (
//                Expression.Invoke ( first, parameter ),
//                Expression.Invoke ( second, parameter )
//            );
//            return Expression.Lambda<Func<Asset, bool>> ( combined, parameter );
//        }

//        private IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> MapToResponseDTO ( IEnumerable<Asset> assets )
//        {
//            return assets.Select ( a => new DTOs.AssetSearchDTOs.GetAssetResponseDTO
//            {
//                Id = a.Id,
//                Name = a.Name,
//                ModelNumber = a.ModelNumber,
//                SerialNumber = a.SerialNumber,
//                Description = a.dicription,
//                PurchaseDate = a.PurchaseDate,
//                PurchasePrice = a.PurchasePrice,
//                WarrantyExpiryDate = a.WarrantyExpiryDate,
//                DepreciationDate = a.DepreciationDate,
//                Status = a.Status,
//                LocationId = a.LocationId,
//                LocationName = a.Location?.Name,
//                AssignedUserId = a.AssignedUserId,
//                AssignedUserName = a.AssignedUser?.UserName,
//                CategoryId = a.CategoryId,
//                CategoryName = a.Category?.Name,
//                ManufacturerId = a.ManufacturerId,
//                ManufacturerName = a.Manufacturer?.Name,
//                Quantity = a.Quantity,
//                MinQuantityLimit = a.MinQuantityLimit,
//                AddedOnDate = a.AddedOnDate,
//                UpdatedDate = a.UpdatedDate
//            } );
//        }
//        #endregion
//    }
//}
