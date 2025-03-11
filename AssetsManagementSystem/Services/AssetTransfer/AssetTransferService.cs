using AssetsManagementSystem.DTOs.AssetTransferDTOs;
using AssetsManagementSystem.Models.DbSets;
using Microsoft.AspNetCore.Identity;

namespace AssetsManagementSystem.Services.AssetTransfer
{
    public class AssetTransferService : BaseClassForServices
    {
        private readonly UserManager<User> _userManager;

        public AssetTransferService(IUnitOfWork unitOfWork,
                                    Others.Interfaces.IAutoMapper.IMapper mapper,
                                    IHttpContextAccessor httpContextAccessor,
                                    UserManager<User> userManager)
            : base(unitOfWork, mapper, httpContextAccessor)
        {
            _userManager = userManager;
        }


        #region Location to Location Transfer
        public async Task<GetAssetTransferRecordResponseDTO> TransferLocationToLocationAsync(LocationToLocationTransferDTO dto)
         {
            var fromlocation = await UnitOfWork.readRepository<Location>().GetAsync(l => l.Barcode == dto.FromLocationBarcode);
            var Tolocation = await UnitOfWork.readRepository<Location>().GetAsync(l => l.Barcode == dto.ToLocationBarcode&&
                                                                    (l.IsDeleted == false || l.IsDeleted == null));
            if (Tolocation is null)
                throw new InvalidOperationException("Cannot transfer asset to the Deleted or not found location.");
             


            await ValidateLocationTransfer(fromlocation.Id, Tolocation.Id);
              


            var asset = await UnitOfWork.readRepository<Asset>().GetAsync(a => a.SerialNumber == dto.AssetSerialNumber &&
                                                                           (a.IsDeleted==false || a.IsDeleted==null)&&
                                                                            a.Status!=AssetStatus.Retired.ToString()&&
                                                                            a.LocationId==fromlocation.Id);

            if (asset == null) throw new KeyNotFoundException("Asset not found or has deleted.");
             

            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromLocationId = fromlocation.Id,
                ToLocationId = Tolocation.Id,
                FromUserId = asset.AssignedUserId,
                ToUserId = asset.AssignedUserId,
                Status = TransferStatus.Approved.ToString(),
                AddedOnDate=DateTime.Now,
                ApprovalDate= DateOnly.FromDateTime(DateTime.Now),
                IsUserTransfer = false,
            };

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords>().AddAsync(transferRecord);
                asset.LocationId = Tolocation.Id;
                asset.UpdatedDate = DateTime.Now;
                await UnitOfWork.writeRepository<Asset>().UpdateAsync(asset.Id, asset);

                await UnitOfWork.SaveChangeAsync();
                await UnitOfWork.CommitTransactionAsync();

                return await GetAssetTransferByIdAsync(transferRecord.Id);
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion
         

        #region User to User Transfer
        public async Task<GetAssetTransferRecordResponseDTO> TransferUserToUserAsync(UserToUserTransferDTO dto)
        {
            await ValidateUserTransfer(dto.FromUserId, dto.ToUserId);

            User user = await _userManager.FindByIdAsync(dto.ToUserId.ToString());
            if (user is null)
                throw new InvalidOperationException("Cannot transfer asset to the Deleted or not found User.");


            var asset = await UnitOfWork.readRepository<Asset>().GetAsync(a => a.SerialNumber == dto.AssetSerialNumber &&
                                                                           (a.IsDeleted == false || a.IsDeleted == null) &&
                                                                            a.Status != AssetStatus.Retired.ToString()&&
                                                                            a.AssignedUserId==dto.FromUserId
                                                                            );

            

            if (asset == null) throw new KeyNotFoundException("Asset not found or has deleted.");

            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromUserId = dto.FromUserId,
                ToUserId = dto.ToUserId,
                FromLocationId=asset.LocationId,
                ToLocationId = asset.LocationId,
                AddedOnDate = DateTime.Now,
                Status = TransferStatus.Pending.ToString(),
                IsUserTransfer=true
                
            };

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords>().AddAsync(transferRecord);
                await UnitOfWork.SaveChangeAsync();
                await UnitOfWork.CommitTransactionAsync();

                return await GetAssetTransferByIdAsync(transferRecord.Id);
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion


        #region User and Location Transfer
        public async Task<GetAssetTransferRecordResponseDTO> TransferUserAndLocationAsync(UserAndLocationTransferDTO dto)
        {
            ValidateUserTransfer(dto.FromUserId, dto.ToUserId);

            var fromlocation = await UnitOfWork.readRepository<Location>().GetAsync(l => l.Barcode == dto.FromLocationBarcode);
            var Tolocation = await UnitOfWork.readRepository<Location>().GetAsync(l => l.Barcode == dto.ToLocationBarcode &&
                                                                    (l.IsDeleted == false || l.IsDeleted == null));

            await ValidateLocationTransfer(fromlocation.Id, Tolocation.Id);

            var asset = await UnitOfWork.readRepository<Asset>().GetAsync(a => a.SerialNumber == dto.AssetSerialNumber &&
                                                                           (a.IsDeleted == false || a.IsDeleted == null) &&
                                                                            a.Status != AssetStatus.Retired.ToString());
            if (asset == null) throw new KeyNotFoundException("Asset not found or deleted.");

            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromUserId = dto.FromUserId,
                ToUserId = dto.ToUserId,
                FromLocationId = fromlocation.Id,
                ToLocationId = Tolocation.Id,
                Status = TransferStatus.Pending.ToString(),
                IsUserTransfer=true,
                AddedOnDate =DateTime.Now
            };

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords>().AddAsync(transferRecord);
                await UnitOfWork.SaveChangeAsync();
                await UnitOfWork.CommitTransactionAsync();

                return await GetAssetTransferByIdAsync(transferRecord.Id);
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion

          
        #region Approve Transfer
        public async Task ApproveTransferAsync(int transferId)
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords>().GetAsync(r => r.Id == transferId && (r.IsDeleted ==false||r.IsDeleted==null)
                                                                                                ,enableTracing:false);

            if (transferRecord == null) 
                throw new KeyNotFoundException("Transfer record not found or has been deleted.");
            //changed to ! string.Equal()
            if (!string.Equals(UserId, transferRecord.ToUserId.ToString()))
                throw new InvalidOperationException("Your are not authorized for perform this operation.");

            if (transferRecord.Status != TransferStatus.Pending.ToString()) 
                    throw new InvalidOperationException("Transfer is not in a pending state.");

            transferRecord.Status = TransferStatus.Approved.ToString();
            transferRecord.ApprovalDate = DateOnly.FromDateTime(DateTime.Now);  

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                var asset = await UnitOfWork.readRepository<Asset>().GetAsync(a => a.Id == transferRecord.AssetId,enableTracing:false);
                if (asset == null) throw new KeyNotFoundException("Asset not found.");

                if (transferRecord.IsUserTransfer)
                {
                    asset.AssignedUserId = transferRecord.ToUserId;
                }
                asset.LocationId = transferRecord.ToLocationId;
                asset.UpdatedDate = DateTime.Now;

                await UnitOfWork.writeRepository<AssetTransferRecords>().UpdateAsync(transferRecord.Id, transferRecord);

                await UnitOfWork.SaveChangeAsync();
                await UnitOfWork.writeRepository<Asset>().UpdateAsync(asset.Id, asset);
                await UnitOfWork.SaveChangeAsync();

                await UnitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion

         
        #region Reject Transfer
        public async Task RejectTransferAsync(int transferId, string rejectionReason)
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords>().GetAsync(r => r.Id == transferId 
                                                                                                    && (r.IsDeleted==null || r.IsDeleted ==false));
            if (transferRecord == null) throw new KeyNotFoundException("Transfer record not found.");
            if (transferRecord.Status != TransferStatus.Pending.ToString()) throw new InvalidOperationException("Transfer is not in a pending state.");

            transferRecord.Status = TransferStatus.Rejected.ToString();
            transferRecord.RejectionReason = rejectionReason;
            transferRecord.ApprovalDate = null;

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords>().UpdateAsync(transferRecord.Id, transferRecord);
                await UnitOfWork.SaveChangeAsync();
                await UnitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion


        #region Get Asset Transfer Record by ID
        public async Task<GetAssetTransferRecordResponseDTO> GetAssetTransferByIdAsync(int id)
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords>().GetAsync(tr => tr.Id == id&& (tr.IsDeleted == null || tr.IsDeleted == false));
            if (transferRecord == null) throw new KeyNotFoundException("Transfer record not found.");

            var getAssetTransferByIdAsync = new GetAssetTransferRecordResponseDTO
            {
                Id = transferRecord.Id,
                AssetId = transferRecord.AssetId,
                AssetName = transferRecord.Asset.Name,  

                FromUserId = transferRecord.FromUserId,
                FromUserName = transferRecord.FromUser != null ? string.Concat(transferRecord.FromUser.FirstName, transferRecord.FromUser.LastName) : string.Empty,  

                ToUserId = transferRecord.ToUserId,
                ToUserName = transferRecord.ToUser != null ? string.Concat(transferRecord.ToUser.FirstName, transferRecord.ToUser.LastName) : string.Empty, 

                FromLocationId = transferRecord.FromLocationId,
                FromLocationName = transferRecord.FromLocation != null ? transferRecord.FromLocation.Name : string.Empty, 

                ToLocationId = transferRecord.ToLocationId,
                ToLocationName = transferRecord.ToLocation != null ? transferRecord.ToLocation.Name : string.Empty, 

                Status = transferRecord.Status,
                ApprovalDate = transferRecord.ApprovalDate.HasValue ? transferRecord.ApprovalDate : null,
                RejectionReason = transferRecord.RejectionReason,

                AddedOnDate = transferRecord.AddedOnDate,
                UpdatedDate = transferRecord.UpdatedDate, 

                IsUserTransfer = transferRecord.IsUserTransfer,
                
            };

  

            return getAssetTransferByIdAsync;
        }
        #endregion

        #region Get Asset Transfer Record by Current User
        public async Task<IList<GetAssetTransferRecordResponseDTO>> GetAssetTransferForCurrentUserByIdAsync()
        {
            var transferRecords = await UnitOfWork.readRepository<AssetTransferRecords>()
                .GetAllAsync(tr => tr.ToUserId == Guid.Parse(UserId)&&tr.Status== TransferStatus.Pending.ToString() );

            if (transferRecords == null) throw new KeyNotFoundException("there  are no Transfer record for you.");


            var getAssetTransferCurrentUSerByIdAsync = transferRecords.Select
               (
               tr => {
                   var transferRecord = new GetAssetTransferRecordResponseDTO
                   {
                       Id = tr.Id,
                       AssetId = tr.AssetId,
                       AssetName = tr.Asset.Name,

                       FromUserId = tr.FromUserId,
                       FromUserName = tr.FromUser != null ? tr.FromUser.UserName : string.Empty,

                       ToUserId = tr.ToUserId,
                       ToUserName = tr.ToUser != null ? string.Concat(tr.ToUser.FirstName, tr.ToUser.LastName) : string.Empty,

                       FromLocationId = tr.FromLocationId,
                       FromLocationName = tr.FromLocation != null ? string.Concat(tr.FromUser.FirstName, tr.FromUser.LastName) : string.Empty,

                       ToLocationId = tr.ToLocationId,
                       ToLocationName = tr.ToLocation != null ? tr.ToLocation.Name : string.Empty,

                       Status = tr.Status,
                       ApprovalDate = tr.ApprovalDate.HasValue ?tr.ApprovalDate : null,
                       RejectionReason = tr.RejectionReason,

                       AddedOnDate = tr.AddedOnDate,
                       UpdatedDate = tr.UpdatedDate
                   };
                    return transferRecord;
               }
               ).ToList();
             
            return getAssetTransferCurrentUSerByIdAsync;
        }
        #endregion

        #region Get All Asset Transfers
        public async Task<IList<GetAssetTransferRecordResponseDTO>> GetAllAssetTransfersAsync()
        {
            var transferRecords = await UnitOfWork.readRepository<AssetTransferRecords>()
                .GetAllAsync(predicate:tr=>(tr.IsDeleted==null||tr.IsDeleted==false));

            var getAssetTransferRecordResponseDTO=transferRecords.Select
                (
                tr=> {
                    var transferRecord = new GetAssetTransferRecordResponseDTO
                    {
                        Id = tr.Id,
                        AssetId = tr.AssetId,
                        AssetName = tr.Asset.Name,

                        FromUserId = tr.FromUserId,
                        FromUserName = tr.FromUser != null ? tr.FromUser.UserName : string.Empty,

                        ToUserId = tr.ToUserId,
                        ToUserName = tr.ToUser != null ? string.Concat(tr.ToUser.FirstName, tr.ToUser.LastName) : string.Empty,

                        FromLocationId = tr.FromLocationId,
                        FromLocationName = tr.FromLocation != null ? string.Concat(tr.FromUser.FirstName, tr.FromUser.LastName) : string.Empty,

                        ToLocationId = tr.ToLocationId,
                        ToLocationName = tr.ToLocation != null ? tr.ToLocation.Name : string.Empty,

                        Status = tr.Status,
                        ApprovalDate = tr.ApprovalDate.HasValue ? tr.ApprovalDate : null,
                        RejectionReason = tr.RejectionReason,

                        AddedOnDate = tr.AddedOnDate,
                        UpdatedDate = tr.UpdatedDate,
                        IsUserTransfer=tr.IsUserTransfer
                    };

                    return transferRecord;
                    }
                ).ToList();




            return getAssetTransferRecordResponseDTO;
        }
        #endregion

        #region Get All By Pagination Asset Transfers
        public async Task<IList<GetAssetTransferRecordResponseDTO>> GetAllByPaginationAssetTransfersAsync(int currentPage = 1, int pageSize = 10)
        {
            var transferRecords = await UnitOfWork.readRepository<AssetTransferRecords>()
                .GetAllByPagningAsync(predicate: tr => (tr.IsDeleted == null || tr.IsDeleted == false), pageSize: pageSize, currentPage: currentPage);

            var getAssetTransferRecordResponseDTO = transferRecords.Select
                (
                tr => {
                    var transferRecord = new GetAssetTransferRecordResponseDTO
                    {
                        Id = tr.Id,
                        AssetId = tr.AssetId,
                        AssetName = tr.Asset.Name,

                        FromUserId = tr.FromUserId,
                        FromUserName = tr.FromUser != null ? tr.FromUser.UserName : string.Empty,

                        ToUserId = tr.ToUserId,
                        ToUserName = tr.ToUser != null ? string.Concat(tr.ToUser.FirstName, tr.ToUser.LastName) : string.Empty,

                        FromLocationId = tr.FromLocationId,
                        FromLocationName = tr.FromLocation != null ? string.Concat(tr.FromUser.FirstName, tr.FromUser.LastName) : string.Empty,

                        ToLocationId = tr.ToLocationId,
                        ToLocationName = tr.ToLocation != null ? tr.ToLocation.Name : string.Empty,

                        Status = tr.Status,
                        ApprovalDate = tr.ApprovalDate.HasValue ? tr.ApprovalDate : null,
                        RejectionReason = tr.RejectionReason,

                        AddedOnDate = tr.AddedOnDate,
                        UpdatedDate = tr.UpdatedDate,
                        IsUserTransfer = tr.IsUserTransfer
                    };

                    return transferRecord;
                }
                ).ToList();




            return getAssetTransferRecordResponseDTO;
        }
        #endregion


        #region Update User to User Transfer
        public async Task<GetAssetTransferRecordResponseDTO> UpdateUserToUserTransferAsync(int id, UpdateUserToUserTransferDTO dto)
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords>().GetAsync(r => r.Id == id && 
                                                                                                (r.IsDeleted == false || r.IsDeleted == null)
                                                                                                &&r.Status==TransferStatus.Pending.ToString());
            if (transferRecord == null) throw new KeyNotFoundException("Transfer record not found.");

           await ValidateUserTransfer(transferRecord.FromUserId, dto.ToUserId);
            
            User user = await _userManager.FindByIdAsync(dto.ToUserId.ToString());
            if (user is null)
                throw new InvalidOperationException("Cannot transfer asset to the Deleted or not found User.");

            var asset = await UnitOfWork.readRepository<Asset>().GetAsync(a => a.Id == transferRecord.AssetId &&
                                                                           (a.IsDeleted == false || a.IsDeleted == null) &&
                                                                            a.Status != AssetStatus.Retired.ToString()
                                                                            && a.AssignedUserId == transferRecord.FromUserId);

            if (asset == null) throw new KeyNotFoundException("Asset not found or has deleted.");

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                transferRecord.FromUserId = transferRecord.FromUserId;
                transferRecord.ToUserId = dto.ToUserId;
                transferRecord.Status = TransferStatus.Pending.ToString(); 

                await UnitOfWork.writeRepository<AssetTransferRecords>().UpdateAsync(transferRecord.Id, transferRecord);
                await UnitOfWork.SaveChangeAsync();
                await UnitOfWork.CommitTransactionAsync();

                return await GetAssetTransferByIdAsync(transferRecord.Id);
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion

        #region Update User and Location Transfer
        public async Task<GetAssetTransferRecordResponseDTO> UpdateUserAndLocationTransferAsync(int id, UpdateUserAndLocationTransferDTO dto)
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords>().GetAsync(r => r.Id == id 
                                                                                                    && (r.IsDeleted == false || r.IsDeleted == null)
                                                                                                    &&r.Status==TransferStatus.Pending.ToString());
            
            if (transferRecord == null) throw new KeyNotFoundException("Transfer record not found.");


             var Tolocation = await UnitOfWork.readRepository<Location>().GetAsync(l => l.Barcode == dto.ToLocationBarcode&&
                                                                    (l.IsDeleted == false || l.IsDeleted == null));

 


            await ValidateUserTransfer(transferRecord.FromUserId, dto.ToUserId);
            await ValidateLocationTransfer(transferRecord.FromLocationId, Tolocation.Id);

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                transferRecord.FromUserId = transferRecord.FromUserId;
                transferRecord.ToUserId = dto.ToUserId;
                transferRecord.FromLocationId = transferRecord.FromLocationId;
                transferRecord.ToLocationId = Tolocation.Id;
                transferRecord.Status = TransferStatus.Pending.ToString();  

                await UnitOfWork.writeRepository<AssetTransferRecords>().UpdateAsync(transferRecord.Id, transferRecord);
                await UnitOfWork.SaveChangeAsync();
                await UnitOfWork.CommitTransactionAsync();

                return await GetAssetTransferByIdAsync(transferRecord.Id);
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion

        #region Delete Asset Transfer Record
        public async Task DeleteAssetTransferAsync(int id)
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords>()
                                            .GetAsync(r => r.Id == id && (r.IsDeleted == false || r.IsDeleted == null)&&
                                                        r.Status==TransferStatus.Pending.ToString());

            if (transferRecord == null) throw new KeyNotFoundException("Transfer record not found or has deleted.");

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                transferRecord.DeletedDate = DateTime.Now;
                transferRecord.IsDeleted = true;

                await UnitOfWork.writeRepository<AssetTransferRecords>().UpdateAsync(transferRecord.Id,transferRecord);
                await UnitOfWork.SaveChangeAsync();
                await UnitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion

        #region Private Methods

        private async Task ValidateLocationTransfer(int fromLocationId, int toLocationId)
        {
          

            if (fromLocationId == toLocationId)
                throw new InvalidOperationException("Cannot transfer asset to the same location.");
             
        }

        private async Task ValidateUserTransfer(Guid fromUserId, Guid toUserId)
        {  
            if (fromUserId == toUserId)
                throw new InvalidOperationException("Cannot transfer asset to the same user.");
           
        }

        #endregion
    }
}
