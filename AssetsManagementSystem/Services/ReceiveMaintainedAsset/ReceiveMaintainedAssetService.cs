
using AssetsManagementSystem.DTOs.ReceiveMaintainedDeviceDTOs;
using AssetsManagementSystem.Models.DbSets;
using AssetsManagementSystem.Services.Document;

namespace AssetsManagementSystem.Services.ReceiveMaintainedAsset
{
    public class ReceiveMaintainedAssetService : BaseClassForServices
    {
        private readonly UserManager<User> userManager;
        private readonly DocumentService documentService;

        public ReceiveMaintainedAssetService(IUnitOfWork unitOfWork, 
                                             Others.Interfaces.IAutoMapper.IMapper mapper,
                                             IHttpContextAccessor httpContextAccessor,
                                             UserManager<User> userManager,
                                             DocumentService documentService)
            : base(unitOfWork, mapper, httpContextAccessor)
        {
            this.userManager = userManager;
            this.documentService = documentService;
        }

        public async Task AddReceiveMaintainedAsset(AddReceiveMaintainedAssetRequest addReceiveMaintainedAsset)
        {

            #region Asset
            Asset asset = await UnitOfWork.readRepository<Asset>().GetAsync(predicate:
                                            a => a.SerialNumber == addReceiveMaintainedAsset.AssetSerialNumber &&
                                            a.Status == AssetStatus.InRepair.ToString() &&
                                            (a.IsDeleted == null || a.IsDeleted == false)
                                            );

            if (asset is null)
                throw new KeyNotFoundException("THIS Asset Is deleted or not found");
            #endregion

            #region location
            Location location = await UnitOfWork.readRepository<Location>().GetAsync(predicate:
                                           l => l.Id == addReceiveMaintainedAsset.NewLocationAssignedId &&
                                           (l.IsDeleted == null || l.IsDeleted == false)
                                           );

            
            
            
            
            
            
            
            
            if (location is null)
                throw new KeyNotFoundException("THIS location Is deleted or not found");

            #endregion

            #region User
            User NewUserAssigned = await userManager.FindByIdAsync(addReceiveMaintainedAsset.NewUserAssignedId.ToString());
            User UserRecieveDevFromSupplier = await userManager.FindByIdAsync(addReceiveMaintainedAsset.UserRecieveDevFromSupplierId.ToString());

            if (NewUserAssigned is null || UserRecieveDevFromSupplier is null)
                throw new KeyNotFoundException("THIS new Assigned User or recieved user  not found");

            if (NewUserAssigned.UserStatus != UserStatus.Active.ToString() || UserRecieveDevFromSupplier.UserStatus != UserStatus.Active.ToString())
                throw new KeyNotFoundException("THIS new Assigned User or recieved user  is still inactive or deleted");

            #endregion


            if ( addReceiveMaintainedAsset.ReceiveMaintainedAssetDoc != null )
            {
                var UploadedDoc = await documentService.AddDocumentAsync ( new DTOs.DocumentDTOs.AddDocumentRequestDTO ( )
                {
                    Title = addReceiveMaintainedAsset.Title,
                    AssetId = asset.Id,
                    PdfFile = addReceiveMaintainedAsset.ReceiveMaintainedAssetDoc
                } );
            }
            

            #region Mapping
            var receiveMaintainedAsset = new Models.DbSets.ReceiveMaintainedAsset()
            {
                AssetId = asset.Id,
                DateOfRecieve = DateOnly.FromDateTime(DateTime.Now),
                DocumentId = null,
                NewUserAssignedId = addReceiveMaintainedAsset.NewUserAssignedId,
                NewLocationAssigned = addReceiveMaintainedAsset.NewLocationAssignedId,
                UserRecieveDevFromSupplierId = addReceiveMaintainedAsset.UserRecieveDevFromSupplierId,

            };

            #endregion

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                await UnitOfWork.writeRepository<Models.DbSets.ReceiveMaintainedAsset>().AddAsync(receiveMaintainedAsset);
                asset.LocationId = addReceiveMaintainedAsset.NewLocationAssignedId;
                asset.AssignedUserId = addReceiveMaintainedAsset.NewUserAssignedId;
                asset.Status=AssetStatus.Active.ToString();
                await UnitOfWork.writeRepository<Asset>().UpdateAsync(asset.Id, asset);

                await UnitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }

        }

    }
}


 




 