
using AssetsManagementSystem.DTOs.AssetDTOs;
using AssetsManagementSystem.Models.DbSets;
using AssetsManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace AssetsManagementSystem.Services.Assets
{
    public class AssetService : BaseClassForServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AssetService> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AssetService (
            IUnitOfWork unitOfWork,
            Others.Interfaces.IAutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ILogger<AssetService> logger,
            UserManager<User> userManager,
            IConfiguration configuration )
            : base ( unitOfWork, mapper, httpContextAccessor )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
        }

        #region Get Assets (All, ById, ForUser)

        public async Task<GetAssetResponseDTO> GetAssetByBarcodeAsync ( string barcode )
        {
            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync (
                predicate: a => a.Barcode == barcode && ( a.IsDeleted == false || a.IsDeleted == null ) &&
                                a.Status != AssetStatus.Retired.ToString ( ),
                include: source => source
                    .Include ( a => a.Location )
                    .Include ( a => a.Category )
                    .Include ( a => a.Manufacturer )
                    .Include ( a => a.AssignedUser )
                    .Include ( a => a.AssetsSuppliers ).ThenInclude ( s => s.Supplier )
            );

            if ( asset == null ) throw new KeyNotFoundException ( $"Asset with barcode '{barcode}' not found." );

            return MapToGetAssetDTO ( asset );
        }

        public async Task<List<GetAssetResponseDTO>> GetAllByPaginationAssetsAsync ( int currentPage = 1, int pageSize = 10 )
        {
            var assets = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
                predicate: a => a.IsDeleted == false || a.IsDeleted == null,
                include: source => source
                    .Include ( a => a.Location )
                    .Include ( a => a.Category )
                    .Include ( a => a.Manufacturer )
                    .Include ( a => a.AssignedUser )
                    .Include ( a => a.AssetsSuppliers ).ThenInclude ( s => s.Supplier ),
                pageSize: pageSize,
                currentPage: currentPage
            );

            return assets.Select ( MapToGetAssetDTO ).ToList ( );
        }

        public async Task<List<GetAssetResponseDTO>> GetAssetsForCurrentUser ( )
        {
            var currentUser = Guid.Parse ( UserId );
            var assets = await UnitOfWork.readRepository<Asset> ( ).GetAllAsync (
                predicate: a => a.AssignedUserId == currentUser && ( a.IsDeleted == false || a.IsDeleted == null ),
                include: source => source.Include ( a => a.Location ).Include ( a => a.Category ).Include ( a => a.Manufacturer ).Include ( a => a.AssignedUser )
            );
            return assets.Select ( MapToGetAssetDTO ).ToList ( );
        }

        #endregion

        #region Add Asset
        //        public async Task<List<string>> AddAssetAsync ( AddAssetRequestDTO dto )
        //        {
        //            // 1. Validation
        //            await ValidateForeignKeysAsync ( dto.LocationId??0, dto.CategoryId, dto.ManufacturerId, dto.AssignedUserId, dto.SupplierIds );

        //            // 2. Check Duplicate Serials (Batch Check)
        //            if ( dto.SerialNumbers != null && dto.SerialNumbers.Any ( ) )
        //            {
        //                if ( dto.SerialNumbers.Distinct ( ).Count ( ) != dto.SerialNumbers.Count )
        //                    throw new InvalidOperationException ( "Duplicate serial numbers found in the request list." );

        //                var existingAssets = await UnitOfWork.readRepository<Asset> ( ).GetAllAsync ( a => dto.SerialNumbers.Contains ( a.SerialNumber )
        //                                                                                          && ( a.IsDeleted == false || a.IsDeleted == null ) );
        //                if ( existingAssets.Any ( ) )
        //                {
        //                    var duplicates = string.Join ( ", ", existingAssets.Select ( a => a.SerialNumber ) );
        //                    throw new InvalidOperationException ( $"The following Serial Numbers already exist in database: {duplicates}" );
        //                }
        //            }

        //            // 3. Prepare Barcode Prefix
        //            var category = await UnitOfWork.readRepository<Category> ( ).GetAsync ( c => c.Id == dto.CategoryId, include: s => s.Include ( x => x.ParentCategory ) );
        //            if ( category == null ) throw new InvalidOperationException ( "Category not found." );

        //            string barcodePrefix = category.ParentCategory != null
        //                ? $"{category.ParentCategory.SerialCode}-{category.SerialCode}"
        //                : category.SerialCode;

        //            //var lastAssetList = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
        //            //    predicate: a => a.Barcode.StartsWith ( barcodePrefix ),
        //            //    orderby: q => q.OrderByDescending ( a => a.Barcode ),
        //            //    currentPage: 1, pageSize: 1 );

        //            int expectedLength = barcodePrefix.Length + 1 + 6;
        //            var lastAssetList = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
        //    predicate: a => a.Barcode.StartsWith ( barcodePrefix + "-" ) // نتأكد إن بعد البريفكس فيه داش
        //                 && a.Barcode.Length == expectedLength,       // 🛑 ده الشرط اللي هيحل المشكلة
        //    orderby: q => q.OrderByDescending ( a => a.Barcode ),
        //    currentPage: 1,
        //    pageSize: 1
        //);


        //            int currentSequence = 0;
        //            var lastAsset = lastAssetList.FirstOrDefault ( );
        //            if ( lastAsset != null )
        //            {
        //                var parts = lastAsset.Barcode.Split ( '-' );
        //                if ( parts.Length > 0 && int.TryParse ( parts.Last ( ), out int seq ) ) currentSequence = seq;
        //            }

        //            var assetsToAdd = new List<Asset> ( );
        //            var generatedBarcodes = new List<string> ( );

        //            // -------------------------------------------------------
        //            // 🔥 Core Logic: AssetType Decision (Bulk vs Individual)
        //            // -------------------------------------------------------
        //            if ( dto.AssetType == AssetType.Consumable )
        //            {
        //                // === Case 1: Consumable (Bulk - 1 Row) ===
        //                currentSequence++;
        //                string newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";

        //                var asset = MapDtoToAsset ( dto );
        //                asset.Barcode = newBarcode;
        //                asset.Quantity = dto.Quantity;        // Full Quantity
        //                asset.SerialNumber = null;            // No Serial
        //                asset.MinQuantityLimit = dto.MinQuantityLimit;

        //                assetsToAdd.Add ( asset );
        //                generatedBarcodes.Add ( newBarcode );
        //            }
        //            else
        //            {
        //                // === Case 2: Fixed Assets (IT & Non-IT) - Loop ===
        //                for ( int i = 0; i < dto.Quantity; i++ )
        //                {
        //                    currentSequence++;
        //                    string newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";

        //                    var asset = MapDtoToAsset ( dto );
        //                    asset.Barcode = newBarcode;
        //                    asset.Quantity = 1; // Always 1 per row
        //                    asset.MinQuantityLimit = null;

        //                    // --- Serial Logic ---
        //                    if ( dto.AssetType == AssetType.IT )
        //                    {
        //                        if ( dto.SerialNumbers != null && dto.SerialNumbers.Count > i )
        //                            asset.SerialNumber = dto.SerialNumbers [i];
        //                        else
        //                            throw new InvalidOperationException ( $"Serial Number is required for IT Asset #{i + 1}." );
        //                    }
        //                    else // Non-IT
        //                    {
        //                        asset.SerialNumber = ( dto.SerialNumbers != null && dto.SerialNumbers.Count > i ) ? dto.SerialNumbers [i] : null;
        //                    }

        //                    assetsToAdd.Add ( asset );
        //                    generatedBarcodes.Add ( newBarcode );
        //                }
        //            }

        //            // 4. Save Transaction
        //            await UnitOfWork.BeginTransactionAsync ( );
        //            try
        //            {
        //                await UnitOfWork.writeRepository<Asset> ( ).AddRangeAsync ( assetsToAdd );
        //                await UnitOfWork.SaveChangeAsync ( );

        //                if ( dto.SupplierIds != null && dto.SupplierIds.Any ( ) )
        //                {
        //                    foreach ( var addedAsset in assetsToAdd )
        //                    {
        //                        await AddOrUpdateAssetSuppliers ( addedAsset.Id, dto.SupplierIds );
        //                    }
        //                    await UnitOfWork.SaveChangeAsync ( );
        //                }

        //                var auditTrail = new AuditTrail
        //                {
        //                    AddedOn = DateTime.Now,
        //                    Action = "Add",
        //                    EntityType = "Asset",
        //                    EntityName = $"{assetsToAdd.Count} Assets of type {dto.AssetType} added",
        //                    UserId = UserId ?? "System"
        //                };
        //                await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
        //                await UnitOfWork.SaveChangeAsync ( );

        //                await UnitOfWork.CommitTransactionAsync ( );
        //                return generatedBarcodes;
        //            }
        //            catch
        //            {
        //                await UnitOfWork.RollbackTransactionAsync ( );
        //                throw;
        //            }
        //        }
        #endregion


        #region Add Asset
        public async Task<List<string>> AddAssetAsync ( AddAssetRequestDTO dto )
        {
            // 1. Validation
            await ValidateForeignKeysAsync ( dto.LocationId ?? 0, dto.CategoryId, dto.ManufacturerId, dto.AssignedUserId, dto.SupplierIds );

            // 2. Check Duplicate Serials (Batch Check)
            if ( dto.SerialNumbers != null && dto.SerialNumbers.Any ( ) )
            {
                if ( dto.SerialNumbers.Distinct ( ).Count ( ) != dto.SerialNumbers.Count )
                    throw new InvalidOperationException ( "Duplicate serial numbers found in the request list." );

                var existingAssets = await UnitOfWork.readRepository<Asset> ( ).GetAllAsync ( a => dto.SerialNumbers.Contains ( a.SerialNumber )
                                                                                          && ( a.IsDeleted == false || a.IsDeleted == null ) );
                if ( existingAssets.Any ( ) )
                {
                    var duplicates = string.Join ( ", ", existingAssets.Select ( a => a.SerialNumber ) );
                    throw new InvalidOperationException ( $"The following Serial Numbers already exist in database: {duplicates}" );
                }
            }

            // 🆕 2.5 Check Provided Barcodes Validation
            if ( dto.IsBarcodeProvided )
            {
                if ( dto.ProvidedBarcodes == null || !dto.ProvidedBarcodes.Any ( ) )
                    throw new InvalidOperationException ( "Provided barcodes list cannot be empty when IsBarcodeProvided is true." );

                int expectedBarcodeCount = dto.AssetType == AssetType.Consumable ? 1 : dto.Quantity;
                if ( dto.ProvidedBarcodes.Count != expectedBarcodeCount )
                    throw new InvalidOperationException ( $"Expected {expectedBarcodeCount} barcodes, but received {dto.ProvidedBarcodes.Count}." );

                if ( dto.ProvidedBarcodes.Distinct ( ).Count ( ) != dto.ProvidedBarcodes.Count )
                    throw new InvalidOperationException ( "Duplicate barcodes found in the provided request list." );

                var existingBarcodes = await UnitOfWork.readRepository<Asset> ( ).GetAllAsync ( a => dto.ProvidedBarcodes.Contains ( a.Barcode ) );
                if ( existingBarcodes.Any ( ) )
                {
                    var duplicates = string.Join ( ", ", existingBarcodes.Select ( a => a.Barcode ) );
                    throw new InvalidOperationException ( $"The following Barcodes already exist in database: {duplicates}" );
                }
            }

            // 3. Prepare Barcode Prefix
            var category = await UnitOfWork.readRepository<Category> ( ).GetAsync ( c => c.Id == dto.CategoryId, include: s => s.Include ( x => x.ParentCategory ) );
            if ( category == null ) throw new InvalidOperationException ( "Category not found." );

            string barcodePrefix = category.ParentCategory != null
                ? $"{category.ParentCategory.SerialCode}-{category.SerialCode}"
                : category.SerialCode;

            int currentSequence = 0;

            // 🆕 Skip querying the database for sequence if Barcodes are provided by frontend
            if ( !dto.IsBarcodeProvided )
            {
                int expectedLength = barcodePrefix.Length + 1 + 6;
                var lastAssetList = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
                    predicate: a => a.Barcode.StartsWith ( barcodePrefix + "-" )
                                 && a.Barcode.Length == expectedLength,
                    orderby: q => q.OrderByDescending ( a => a.Barcode ),
                    currentPage: 1,
                    pageSize: 1 );

                var lastAsset = lastAssetList.FirstOrDefault ( );
                if ( lastAsset != null )
                {
                    var parts = lastAsset.Barcode.Split ( '-' );
                    if ( parts.Length > 0 && int.TryParse ( parts.Last ( ), out int seq ) ) currentSequence = seq;
                }
            }

            var assetsToAdd = new List<Asset> ( );
            var generatedBarcodes = new List<string> ( );

            // -------------------------------------------------------
            // 🔥 Core Logic: AssetType Decision (Bulk vs Individual)
            // -------------------------------------------------------
            if ( dto.AssetType == AssetType.Consumable )
            {
                // === Case 1: Consumable (Bulk - 1 Row) ===
                string newBarcode;

                // 🆕 Check if Provided
                if ( dto.IsBarcodeProvided )
                {
                    newBarcode = dto.ProvidedBarcodes [0];
                }
                else
                {
                    currentSequence++;
                    newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";
                }

                var asset = MapDtoToAsset ( dto );
                asset.Barcode = newBarcode;
                asset.Quantity = dto.Quantity;        // Full Quantity
                asset.SerialNumber = null;            // No Serial
                asset.MinQuantityLimit = dto.MinQuantityLimit;

                assetsToAdd.Add ( asset );
                generatedBarcodes.Add ( newBarcode );
            }
            else
            {
                // === Case 2: Fixed Assets (IT & Non-IT) - Loop ===
                for ( int i = 0; i < dto.Quantity; i++ )
                {
                    string newBarcode;

                    // 🆕 Check if Provided
                    if ( dto.IsBarcodeProvided )
                    {
                        newBarcode = dto.ProvidedBarcodes [i];
                    }
                    else
                    {
                        currentSequence++;
                        newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";
                    }

                    var asset = MapDtoToAsset ( dto );
                    asset.Barcode = newBarcode;
                    asset.Quantity = 1; // Always 1 per row
                    asset.MinQuantityLimit = null;

                    // --- Serial Logic ---
                    if ( dto.AssetType == AssetType.IT )
                    {
                        if ( dto.SerialNumbers != null && dto.SerialNumbers.Count > i )
                            asset.SerialNumber = dto.SerialNumbers [i];
                        else
                            throw new InvalidOperationException ( $"Serial Number is required for IT Asset #{i + 1}." );
                    }
                    else // Non-IT
                    {
                        asset.SerialNumber = ( dto.SerialNumbers != null && dto.SerialNumbers.Count > i ) ? dto.SerialNumbers [i] : null;
                    }

                    assetsToAdd.Add ( asset );
                    generatedBarcodes.Add ( newBarcode );
                }
            }

            // 4. Save Transaction
            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                await UnitOfWork.writeRepository<Asset> ( ).AddRangeAsync ( assetsToAdd );
                await UnitOfWork.SaveChangeAsync ( );

                if ( dto.SupplierIds != null && dto.SupplierIds.Any ( ) )
                {
                    foreach ( var addedAsset in assetsToAdd )
                    {
                        await AddOrUpdateAssetSuppliers ( addedAsset.Id, dto.SupplierIds );
                    }
                    await UnitOfWork.SaveChangeAsync ( );
                }

                var auditTrail = new AuditTrail
                {
                    AddedOn = DateTime.Now,
                    Action = "Add",
                    EntityType = "Asset",
                    EntityName = $"{assetsToAdd.Count} Assets of type {dto.AssetType} added",
                    UserId = UserId ?? "System"
                };
                await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
                await UnitOfWork.SaveChangeAsync ( );

                await UnitOfWork.CommitTransactionAsync ( );
                return generatedBarcodes;
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Update Asset
        public async Task<GetAssetResponseDTO> UpdateAssetAsync ( string barcode, UpdateAssetRequestDTO dto )
        {
            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                var existingAsset = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.Barcode == barcode && ( a.IsDeleted == false || a.IsDeleted == null ) );
                if ( existingAsset == null ) throw new KeyNotFoundException ( $"Asset '{barcode}' not found." );

                await ValidateForeignKeysAsync ( dto.LocationId??0, dto.CategoryId, dto.ManufacturerId, dto.AssignedUserId, dto.SupplierIds );

                // -------------------------------------------------------
                // 🔥 Business Rules Enforcement
                // -------------------------------------------------------
                string newStatusString = dto.Status.ToString ( );

                // Rule 1: Prevent direct assignment change via Update
                if ( dto.AssignedUserId.HasValue && dto.AssignedUserId != existingAsset.AssignedUserId )
                {
                    throw new InvalidOperationException ( "Changing 'Assigned User' directly is not allowed. Please use the Transfer feature." );
                }

                // Rule 2: InUse requires User
                if ( newStatusString == AssetStatus.InUse.ToString ( ) && existingAsset.AssignedUserId == null && dto.AssignedUserId == null )
                {
                    throw new InvalidOperationException ( "Asset cannot be 'In Use' without an assigned user." );
                }

                // Rule 3: Available implies No User (Auto-Fix)
                if ( newStatusString == AssetStatus.Available.ToString ( ) )
                {
                    existingAsset.AssignedUserId = null; // Force un-assign if available
                }

                // Rule 4: Consumables Logic
                if ( existingAsset.AssetType == AssetType.Consumable && newStatusString == AssetStatus.InUse.ToString ( ) )
                {
                    throw new InvalidOperationException ( "Consumables cannot be marked as 'In Use'." );
                }

                // Applying Updates
                existingAsset.Name = dto.Name;
                existingAsset.ModelNumber = dto.ModelNumber;
                existingAsset.Description = dto.Description;
                existingAsset.PurchaseDate = dto.PurchaseDate;
                existingAsset.PurchasePrice = dto.PurchasePrice;
                existingAsset.WarrantyExpiryDate = dto.WarrantyExpiryDate;
                existingAsset.DepreciationDate = dto.DepreciationDate;
                existingAsset.Status = newStatusString;
                existingAsset.LocationId = dto.LocationId;
                existingAsset.CategoryId = dto.CategoryId;
                existingAsset.ManufacturerId = dto.ManufacturerId;
                existingAsset.MinQuantityLimit = dto.MinQuantityLimit;
                existingAsset.UpdatedDate = DateTime.Now;

                // Unique Serial Check
                if ( existingAsset.SerialNumber != dto.SerialNumber && !string.IsNullOrEmpty ( dto.SerialNumber ) )
                {
                    var duplicateCheck = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.SerialNumber == dto.SerialNumber && a.Id != existingAsset.Id );
                    if ( duplicateCheck != null ) throw new InvalidOperationException ( $"Serial Number '{dto.SerialNumber}' is already taken." );
                    existingAsset.SerialNumber = dto.SerialNumber;
                }

                await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( existingAsset.Id, existingAsset );
                await UnitOfWork.SaveChangeAsync ( );

                if ( dto.SupplierIds != null )
                {
                    await DeleteAssetSuppliers ( existingAsset.Id );
                    if ( dto.SupplierIds.Any ( ) ) await AddOrUpdateAssetSuppliers ( existingAsset.Id, dto.SupplierIds );
                    await UnitOfWork.SaveChangeAsync ( );
                }

                await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( new AuditTrail
                {
                    AddedOn = DateTime.Now,
                    Action = "Update",
                    EntityType = "Asset",
                    EntityName = existingAsset.Barcode,
                    UserId = UserId ?? "System"
                } );
                await UnitOfWork.SaveChangeAsync ( );

                await UnitOfWork.CommitTransactionAsync ( );
                return await GetAssetByBarcodeAsync ( existingAsset.Barcode );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Withdraw Quantity
        public async Task<GetAssetResponseDTO> WithdrawQuantityAsync ( int assetId, int quantityToWithdraw )
        {
            if ( quantityToWithdraw <= 0 ) throw new ArgumentException ( "Quantity must be > 0." );

            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.Id == assetId && ( a.IsDeleted == false || a.IsDeleted == null ) );
            if ( asset == null ) throw new KeyNotFoundException ( "Asset not found." );

            if ( asset.AssetType != AssetType.Consumable )
                throw new InvalidOperationException ( "Withdrawal is only for Consumable assets." );

            if ( asset.Quantity < quantityToWithdraw )
                throw new InvalidOperationException ( $"Insufficient stock. Available: {asset.Quantity}" );

            asset.Quantity -= quantityToWithdraw;
            asset.UpdatedDate = DateTime.Now;

            await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );
            await UnitOfWork.SaveChangeAsync ( );

            // Low Stock Alert Logic
            if ( asset.Quantity <= asset.MinQuantityLimit )
            {
                _logger.LogWarning ( "LOW STOCK ALERT: {AssetName} is at {Qty}", asset.Name, asset.Quantity );

                // Read URL from AppSettings
                var notificationUrl = _configuration ["NotificationSettings:BaseUrl"];

                if ( !string.IsNullOrEmpty ( notificationUrl ) )
                {
                    try
                    {
                        var client = _httpClientFactory.CreateClient ( );
                        // ... (Your existing notification logic here) ...
                        // For brevity, I'm keeping the placeholder
                    }
                    catch ( Exception ex )
                    {
                        _logger.LogError ( ex, "Failed to send notification." );
                    }
                }
            }

            return MapToGetAssetDTO ( asset );
        }
        #endregion

        #region Delete Asset
        public async Task<bool> DeleteAssetAsync ( string barcode )
        {
            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.Barcode == barcode && ( a.IsDeleted == false || a.IsDeleted == null ) );
            if ( asset == null ) throw new KeyNotFoundException ( "Asset not found." );

            // Strict Check: Cannot delete assigned assets
            if ( asset.AssignedUserId != null )
            {
                throw new InvalidOperationException ( $"Cannot delete asset '{asset.Name}' because it is currently assigned to a user. Return it to stock first." );
            }

            asset.IsDeleted = true;
            asset.Status = AssetStatus.Retired.ToString ( );
            asset.UpdatedDate = DateTime.Now;

            await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );

            await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( new AuditTrail
            {
                AddedOn = DateTime.Now,
                Action = "Delete (Retire)",
                EntityType = "Asset",
                EntityName = asset.Barcode,
                UserId = UserId ?? "System"
            } );

            await UnitOfWork.SaveChangeAsync ( );
            return true;
        }
        #endregion

        #region Private Helpers (For Clean Code)

        private Asset MapDtoToAsset ( AddAssetRequestDTO dto )
        {
            return new Asset
            {
                Name = dto.Name,
                ModelNumber = dto.ModelNumber,
                Description = dto.Description,
                AssetType = dto.AssetType,
                Status = dto.Status.ToString ( ),
                PurchaseDate = dto.PurchaseDate,
                PurchasePrice = dto.PurchasePrice,
                WarrantyExpiryDate = dto.WarrantyExpiryDate,
                DepreciationDate = dto.DepreciationDate,
                LocationId = dto.LocationId==0?null: dto.LocationId,
                CategoryId = dto.CategoryId,
                ManufacturerId = dto.ManufacturerId,
                AssignedUserId = dto.AssignedUserId,
                AddedOnDate = DateTime.Now
            };
        }

        private static GetAssetResponseDTO MapToGetAssetDTO ( Asset a )
        {
            return new GetAssetResponseDTO
            {
                Id = a.Id,
                Name = a.Name,
                Barcode = a.Barcode,
                AssetType = a.AssetType.ToString ( ),
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate??new DateOnly(),
                PurchasePrice = a.PurchasePrice??0,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                DepreciationDate = a.DepreciationDate,
                Status = a.Status,
                Description = a.Description,
                LocationName = a.Location?.Name ?? "Unknown",
                LocationId = a.LocationId ?? 0,
                LocationBarcode = a.Location?.Barcode ?? "",
                AssignedUserName = a.AssignedUser != null ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}" : "In Stock",
                AssignedUserId = a.AssignedUserId,
                CategoryName = a.Category?.Name ?? "Unknown",
                CategoryId = a.CategoryId,
                ManufacturerName = a.Manufacturer?.Name ?? "N/A",
                ManufacturerId = a.ManufacturerId,
                SupplierNames = a.AssetsSuppliers?.Select ( s => s.Supplier?.CompanyName ).ToList ( ) ?? new List<string> ( ),
                AddedOnDate = a.AddedOnDate,
                UpdatedDate = a.UpdatedDate,
                Quantity = a.Quantity,
                MinQuantityLimit = a.MinQuantityLimit
            };
        }

        private async Task ValidateForeignKeysAsync ( int locId, int catId, int? manId, Guid? userId, List<int>? supIds )
        {
            //var loc = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Id == locId && ( l.IsDeleted == false || l.IsDeleted == null ) );
            //if ( loc == null ) throw new KeyNotFoundException ( $"Location ID {locId} not found" );

            var cat = await UnitOfWork.readRepository<Category> ( ).GetAsync ( c => c.Id == catId && ( c.IsDeleted == false || c.IsDeleted == null ) );
            if ( cat == null ) throw new KeyNotFoundException ( $"Category ID {catId} not found" );

            if ( manId.HasValue )
            {
                var man = await UnitOfWork.readRepository<Manufacturer> ( ).GetAsync ( m => m.Id == manId && ( m.IsDeleted == false || m.IsDeleted == null ) );
                if ( man == null ) throw new KeyNotFoundException ( $"Manufacturer ID {manId} not found" );
            }

            if ( userId.HasValue )
            {
                var user = await UnitOfWork.readRepository<User> ( ).GetAsync ( u => u.Id == userId && ( u.IsDeleted == false || u.IsDeleted == null ) );
                if ( user == null ) throw new KeyNotFoundException ( $"User ID {userId} not found" );
            }

            //if ( supIds != null && supIds.Any ( ) )
            //{
            //    var distinctIds = supIds.Distinct ( ).ToList ( );
            //    var count = await UnitOfWork.readRepository<Supplier> ( ).CountAsync ( s => distinctIds.Contains ( s.Id ) && ( s.IsDeleted == false || s.IsDeleted == null ) );
            //    if ( count != distinctIds.Count ) throw new KeyNotFoundException ( "One or more Supplier IDs invalid." );
            //}
        }

        private async Task AddOrUpdateAssetSuppliers ( int assetId, ICollection<int> supplierIds )
        {
            if ( supplierIds == null || !supplierIds.Any ( ) ) return;
            var distinctSupplierIds = supplierIds.Distinct ( ).ToList ( );
            var suppliersToAdd = distinctSupplierIds.Select ( sid => new AssetsSuppliers { AssetId = assetId, SupplierId = sid } ).ToList ( );
            await UnitOfWork.writeRepository<AssetsSuppliers> ( ).AddRangeAsync ( suppliersToAdd );
        }

        private async Task DeleteAssetSuppliers ( int assetId )
        {
            var existing = await UnitOfWork.readRepository<AssetsSuppliers> ( ).GetAllAsync ( x => x.AssetId == assetId );
            if ( existing != null && existing.Any ( ) )
                await UnitOfWork.writeRepository<AssetsSuppliers> ( ).DeleteRangeAsync ( existing );
        }
        #endregion
    }
}

