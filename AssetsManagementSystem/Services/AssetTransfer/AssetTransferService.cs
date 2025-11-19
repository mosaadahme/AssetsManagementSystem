using AssetsManagementSystem.DTOs.AssetTransferDTOs;
using AssetsManagementSystem.Models.DbSets;
using AssetsManagementSystem.Models.Enums; // تأكد من الـ Namespace للـ Enums
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // ضروري جداً عشان الـ Includes

namespace AssetsManagementSystem.Services.AssetTransfer
{
    public class AssetTransferService : BaseClassForServices
    {
        private readonly UserManager<User> _userManager;

        public AssetTransferService ( IUnitOfWork unitOfWork,
                                    Others.Interfaces.IAutoMapper.IMapper mapper,
                                    IHttpContextAccessor httpContextAccessor,
                                    UserManager<User> userManager )
            : base ( unitOfWork, mapper, httpContextAccessor )
        {
            _userManager = userManager;
        }

        #region Location to Location Transfer (Immediate)
        public async Task<GetAssetTransferRecordResponseDTO> TransferLocationToLocationAsync ( LocationToLocationTransferDTO dto )
        {
            // 1. Get Locations
            var fromLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == dto.FromLocationBarcode );
            var toLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == dto.ToLocationBarcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( toLocation is null )
                throw new InvalidOperationException ( "Destination location not found or deleted." );

            await ValidateLocationTransfer ( fromLocation.Id, toLocation.Id );

            // 2. Get Asset By Barcode (Fix)
            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.Barcode == dto.AssetBarcode && // تم التعديل لـ Barcode
                                                                              ( a.IsDeleted == false || a.IsDeleted == null ) &&
                                                                               a.Status != AssetStatus.Retired.ToString ( ) &&
                                                                               a.LocationId == fromLocation.Id );

            if ( asset == null ) throw new KeyNotFoundException ( "Asset not found in the specified source location." );

            // 3. Create Record
            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromLocationId = fromLocation.Id,
                ToLocationId = toLocation.Id,
                FromUserId = asset.AssignedUserId??Guid.Empty ,  
                ToUserId = asset.AssignedUserId ?? Guid.Empty,
                Status = TransferStatus.Approved.ToString ( ),  
                AddedOnDate = DateTime.Now,
                ApprovalDate = DateOnly.FromDateTime ( DateTime.Now ),
                IsUserTransfer = false,
            };

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );

                // Update Asset Immediately
                asset.LocationId = toLocation.Id;
                asset.UpdatedDate = DateTime.Now;
                await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );

                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );

                return await GetAssetTransferByIdAsync ( transferRecord.Id );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region User to User Transfer (Pending Approval)
        public async Task<GetAssetTransferRecordResponseDTO> TransferUserToUserAsync ( UserToUserTransferDTO dto )
        {
            await ValidateUserTransfer ( dto.FromUserId, dto.ToUserId );

            var toUser = await _userManager.FindByIdAsync ( dto.ToUserId.ToString ( ) );
            if ( toUser is null )
                throw new InvalidOperationException ( "Destination user not found." );

            // Get Asset By Barcode and Ensure current owner is correct
            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.Barcode == dto.AssetBarcode && // تم التعديل لـ Barcode
                                                                              ( a.IsDeleted == false || a.IsDeleted == null ) &&
                                                                               a.Status != AssetStatus.Retired.ToString ( ) &&
                                                                               a.AssignedUserId == dto.FromUserId );

            if ( asset == null ) throw new KeyNotFoundException ( "Asset not found or does not belong to the sender." );

            // Check if there is already a pending transfer for this asset
            var pendingTransfer = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAsync ( t => t.AssetId == asset.Id && t.Status == TransferStatus.Pending.ToString ( ) );

            if ( pendingTransfer != null )
                throw new InvalidOperationException ( "This asset already has a pending transfer request." );

            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromUserId = dto.FromUserId,
                ToUserId = dto.ToUserId,
                FromLocationId = asset.LocationId,
                ToLocationId = asset.LocationId, // المكان ثابت مبدئياً
                AddedOnDate = DateTime.Now,
                Status = TransferStatus.Pending.ToString ( ),
                IsUserTransfer = true
            };

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );
                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );

                return await GetAssetTransferByIdAsync ( transferRecord.Id );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region User and Location Transfer (Pending Approval)
        public async Task<GetAssetTransferRecordResponseDTO> TransferUserAndLocationAsync ( UserAndLocationTransferDTO dto )
        {
            ValidateUserTransfer ( dto.FromUserId, dto.ToUserId );

            var fromLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == dto.FromLocationBarcode );
            var toLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == dto.ToLocationBarcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( toLocation == null ) throw new InvalidOperationException ( "Target location not found." );

            await ValidateLocationTransfer ( fromLocation.Id, toLocation.Id );

            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.Barcode == dto.AssetBarcode && // تم التعديل لـ Barcode
                                                                              ( a.IsDeleted == false || a.IsDeleted == null ) &&
                                                                               a.Status != AssetStatus.Retired.ToString ( ) &&
                                                                               a.LocationId == fromLocation.Id &&
                                                                               a.AssignedUserId == dto.FromUserId );

            if ( asset == null ) throw new KeyNotFoundException ( "Asset not found or details mismatch (User/Location)." );

            // Check pending
            var pendingTransfer = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAsync ( t => t.AssetId == asset.Id && t.Status == TransferStatus.Pending.ToString ( ) );
            if ( pendingTransfer != null ) throw new InvalidOperationException ( "This asset already has a pending transfer request." );

            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromUserId = dto.FromUserId,
                ToUserId = dto.ToUserId,
                FromLocationId = fromLocation.Id,
                ToLocationId = toLocation.Id,
                Status = TransferStatus.Pending.ToString ( ),
                IsUserTransfer = true,
                AddedOnDate = DateTime.Now
            };

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );
                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );

                return await GetAssetTransferByIdAsync ( transferRecord.Id );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Approve Transfer
        public async Task ApproveTransferAsync ( int transferId )
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAsync ( r => r.Id == transferId && ( r.IsDeleted == false || r.IsDeleted == null ) );

            if ( transferRecord == null )
                throw new KeyNotFoundException ( "Transfer record not found." );

            // Authorization check
            if ( !string.Equals ( UserId, transferRecord.ToUserId.ToString ( ), StringComparison.OrdinalIgnoreCase ) )
                throw new InvalidOperationException ( "You are not authorized to approve this transfer." );

            if ( transferRecord.Status != TransferStatus.Pending.ToString ( ) )
                throw new InvalidOperationException ( "Transfer is not in a pending state." );

            transferRecord.Status = TransferStatus.Approved.ToString ( );
            transferRecord.ApprovalDate = DateOnly.FromDateTime ( DateTime.Now );

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.Id == transferRecord.AssetId );
                if ( asset == null ) throw new KeyNotFoundException ( "Asset associated with this transfer was not found." );

                // Apply Changes to Asset
                if ( transferRecord.IsUserTransfer )
                {
                    asset.AssignedUserId = transferRecord.ToUserId;
                }

                asset.LocationId = transferRecord.ToLocationId; // Update location as well
                asset.UpdatedDate = DateTime.Now;

                // Update Record & Asset
                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).UpdateAsync ( transferRecord.Id, transferRecord );
                await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );

                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Reject Transfer
        public async Task RejectTransferAsync ( int transferId, string rejectionReason )
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAsync ( r => r.Id == transferId && ( r.IsDeleted == null || r.IsDeleted == false ) );

            if ( transferRecord == null ) throw new KeyNotFoundException ( "Transfer record not found." );
            if ( transferRecord.Status != TransferStatus.Pending.ToString ( ) ) throw new InvalidOperationException ( "Transfer is not in a pending state." );

            transferRecord.Status = TransferStatus.Rejected.ToString ( );
            transferRecord.RejectionReason = rejectionReason;
            transferRecord.ApprovalDate = null;

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).UpdateAsync ( transferRecord.Id, transferRecord );
                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Get Asset Transfer Record by ID (Fixed Includes)
        public async Task<GetAssetTransferRecordResponseDTO> GetAssetTransferByIdAsync ( int id )
        {
            // Fix: Added Includes to prevent Null Reference
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAsync (
                    predicate: tr => tr.Id == id && ( tr.IsDeleted == null || tr.IsDeleted == false ),
                    include: source => source
                        .Include ( t => t.Asset )
                        .Include ( t => t.FromUser )
                        .Include ( t => t.ToUser )
                        .Include ( t => t.FromLocation )
                        .Include ( t => t.ToLocation )
                );

            if ( transferRecord == null ) throw new KeyNotFoundException ( "Transfer record not found." );

            return MapToTransferDTO ( transferRecord );
        }
        #endregion

        #region Get Asset Transfer Record by Current User (Fixed Includes & Mapping)
        public async Task<IList<GetAssetTransferRecordResponseDTO>> GetAssetTransferForCurrentUserByIdAsync ( )
        {
            var currentUserId = Guid.Parse ( UserId );

            var transferRecords = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAllAsync (
                    predicate: tr => tr.ToUserId == currentUserId && tr.Status == TransferStatus.Pending.ToString ( ),
                    include: source => source
                        .Include ( t => t.Asset )
                        .Include ( t => t.FromUser )
                        .Include ( t => t.ToUser )
                        .Include ( t => t.FromLocation )
                        .Include ( t => t.ToLocation )
                );

            if ( transferRecords == null || !transferRecords.Any ( ) )
                return new List<GetAssetTransferRecordResponseDTO> ( );

            return transferRecords.Select ( MapToTransferDTO ).ToList ( );
        }
        #endregion

        #region Get All Asset Transfers (Fixed Includes & Mapping)
        public async Task<IList<GetAssetTransferRecordResponseDTO>> GetAllAssetTransfersAsync ( )
        {
            var transferRecords = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAllAsync (
                    predicate: tr => ( tr.IsDeleted == null || tr.IsDeleted == false ),
                    include: source => source
                        .Include ( t => t.Asset )
                        .Include ( t => t.FromUser )
                        .Include ( t => t.ToUser )
                        .Include ( t => t.FromLocation )
                        .Include ( t => t.ToLocation )
                );

            return transferRecords.Select ( MapToTransferDTO ).ToList ( );
        }
        #endregion

        #region Get All By Pagination Asset Transfers (Fixed Includes)
        public async Task<IList<GetAssetTransferRecordResponseDTO>> GetAllByPaginationAssetTransfersAsync ( int currentPage = 1, int pageSize = 10 )
        {
            var transferRecords = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAllByPagningAsync (
                    predicate: tr => ( tr.IsDeleted == null || tr.IsDeleted == false ),
                    include: source => source
                        .Include ( t => t.Asset )
                        .Include ( t => t.FromUser )
                        .Include ( t => t.ToUser )
                        .Include ( t => t.FromLocation )
                        .Include ( t => t.ToLocation ),
                    pageSize: pageSize,
                    currentPage: currentPage
                );

            return transferRecords.Select ( MapToTransferDTO ).ToList ( );
        }
        #endregion

        #region Update User to User Transfer
        public async Task<GetAssetTransferRecordResponseDTO> UpdateUserToUserTransferAsync ( int id, UpdateUserToUserTransferDTO dto )
        {
            // Note: Assuming DTO has Barcode now, but logic relies on existing record IDs.

            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( ).GetAsync ( r => r.Id == id &&
                                                                                                ( r.IsDeleted == false || r.IsDeleted == null ) &&
                                                                                                 r.Status == TransferStatus.Pending.ToString ( ) );
            if ( transferRecord == null ) throw new KeyNotFoundException ( "Transfer record not found or not pending." );

            await ValidateUserTransfer ( transferRecord.FromUserId, dto.ToUserId );

            var user = await _userManager.FindByIdAsync ( dto.ToUserId.ToString ( ) );
            if ( user is null ) throw new InvalidOperationException ( "Destination User not found." );

            // No need to fetch Asset again if we trust the TransferRecord, 
            // but confirming ownership is good practice.

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                transferRecord.ToUserId = dto.ToUserId;
                // transferRecord.Status is already Pending, no change needed.

                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).UpdateAsync ( transferRecord.Id, transferRecord );
                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );

                return await GetAssetTransferByIdAsync ( transferRecord.Id );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Update User and Location Transfer
        public async Task<GetAssetTransferRecordResponseDTO> UpdateUserAndLocationTransferAsync ( int id, UpdateUserAndLocationTransferDTO dto )
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( ).GetAsync ( r => r.Id == id &&
                                                                                                ( r.IsDeleted == false || r.IsDeleted == null ) &&
                                                                                                 r.Status == TransferStatus.Pending.ToString ( ) );

            if ( transferRecord == null ) throw new KeyNotFoundException ( "Transfer record not found." );

            var toLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == dto.ToLocationBarcode && ( l.IsDeleted == false || l.IsDeleted == null ) );
            if ( toLocation == null ) throw new KeyNotFoundException ( "Target location not found." );

            await ValidateUserTransfer ( transferRecord.FromUserId, dto.ToUserId );
            await ValidateLocationTransfer ( transferRecord.FromLocationId, toLocation.Id );

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                transferRecord.ToUserId = dto.ToUserId;
                transferRecord.ToLocationId = toLocation.Id;

                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).UpdateAsync ( transferRecord.Id, transferRecord );
                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );

                return await GetAssetTransferByIdAsync ( transferRecord.Id );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Delete Asset Transfer Record
        public async Task DeleteAssetTransferAsync ( int id )
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                                    .GetAsync ( r => r.Id == id && ( r.IsDeleted == false || r.IsDeleted == null ) &&
                                                    r.Status == TransferStatus.Pending.ToString ( ) );

            if ( transferRecord == null ) throw new KeyNotFoundException ( "Transfer record not found or cannot be deleted (not pending)." );

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                transferRecord.DeletedDate = DateTime.Now;
                transferRecord.IsDeleted = true;

                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).UpdateAsync ( transferRecord.Id, transferRecord );
                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Private Methods & Mappers

        private async Task ValidateLocationTransfer ( int fromLocationId, int toLocationId )
        {
            if ( fromLocationId == toLocationId )
                throw new InvalidOperationException ( "Source and Destination locations cannot be the same." );
        }

        private async Task ValidateUserTransfer ( Guid fromUserId, Guid toUserId )
        {
            if ( fromUserId == toUserId )
                throw new InvalidOperationException ( "Source and Destination users cannot be the same." );
        }

        // Helper method to centralize mapping and avoid code duplication
        private static GetAssetTransferRecordResponseDTO MapToTransferDTO ( AssetTransferRecords tr )
        {
            return new GetAssetTransferRecordResponseDTO
            {
                Id = tr.Id,
                AssetId = tr.AssetId,
                AssetName = tr.Asset?.Name ?? "Unknown", // Safe Navigation

                FromUserId = tr.FromUserId,
                FromUserName = tr.FromUser != null ? $"{tr.FromUser.FirstName} {tr.FromUser.LastName}" : "System/Stock",

                ToUserId = tr.ToUserId,
                ToUserName = tr.ToUser != null ? $"{tr.ToUser.FirstName} {tr.ToUser.LastName}" : "Unknown",

                FromLocationId = tr.FromLocationId,
                FromLocationName = tr.FromLocation?.Name ?? "Unknown",

                ToLocationId = tr.ToLocationId,
                ToLocationName = tr.ToLocation?.Name ?? "Unknown",

                Status = tr.Status,
                ApprovalDate = tr.ApprovalDate,
                RejectionReason = tr.RejectionReason,

                AddedOnDate = tr.AddedOnDate,
                UpdatedDate = tr.UpdatedDate,
                IsUserTransfer = tr.IsUserTransfer
            };
        }
        #endregion
    }
}