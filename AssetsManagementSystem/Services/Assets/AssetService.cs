using AssetsManagementSystem.DTOs.AssetDTOs;
using AssetsManagementSystem.Models.DbSets;

namespace AssetsManagementSystem.Services.Assets
{

 
    public class AssetService : BaseClassForServices
    {

        public AssetService(IUnitOfWork unitOfWork, Others.Interfaces.IAutoMapper.IMapper mapper, IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, mapper, httpContextAccessor)
        {
        }



        public async Task<List<GetAssetResponseDTO>> GetAssetsForCurrentUser() 
        {
                        
            var currentUser =Guid.Parse(UserId);
            
            var assets=await UnitOfWork.readRepository<Asset>().GetAllAsync(predicate:a=>a.AssignedUserId==currentUser
                                                                                                        &&
                                                                                                        (a.IsDeleted==false||a.IsDeleted==null)
                                                                                                        );


            var getAssetResponseDTO = assets.Select(a => new GetAssetResponseDTO()
            {
                Id = a.Id,
                Name = a.Name,
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchasePrice = a.PurchasePrice,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                Status = a.Status,
                dicription = a.dicription,
                LocationName = a.Location.Name,
                AssignedUserName = string.Concat(a.AssignedUser.FirstName, " ", a.AssignedUser.LastName),
                CategoryName = a.Category?.Name,
                 SupplierNames = a.AssetsSuppliers.Select(s => s.Supplier.CompanyName).ToList(),
                ManfactureName=a.Manufacturer.Name,
                AddedOnDate = a.AddedOnDate,
                UpdatedDate = a.UpdatedDate
            }).ToList();


            var auditTrail = new AuditTrail()
            {
                AddedOn = DateTime.Now,
                Action = "Fetch",
                EntityType = "Asset",
                UserId = UserId ?? "System"
            };

            await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

           await UnitOfWork.SaveChangeAsync();

            return getAssetResponseDTO;

        }


        #region Add Asset
        public async Task<GetAssetResponseDTO> AddAssetAsync(AddAssetRequestDTO addAssetDto)
        {
            var existingAsset = await UnitOfWork.readRepository<Asset>()
                .GetAsync(a => a.SerialNumber == addAssetDto.SerialNumber);

            if (existingAsset != null)
            {
                throw new InvalidOperationException("An asset with the same serial number already exists.");
            }

            // Validate foreign keys and category-subcategory relation
            await ValidateForeignKeysAndCategoryRelationAsync(addAssetDto);

            var asset = Mapper.Map<Asset, AddAssetRequestDTO>(addAssetDto);
            asset.AddedOnDate = DateTime.Now;
            asset.Quantity = addAssetDto.Quantity;
            asset.MinQuantityLimit = addAssetDto.MinQuantityLimit;

            await UnitOfWork.BeginTransactionAsync();
            try
            { var realcategoryid = await UnitOfWork.readRepository<Category>()
                    .GetAsync(c => c.SerialCode == addAssetDto.CategoryId.ToString());
                asset.CategoryId =realcategoryid.Id;
                await UnitOfWork.writeRepository<Asset>().AddAsync(asset);
           
                await UnitOfWork.SaveChangeAsync();

                // Add related records in AssetsSuppliers table
                await AddOrUpdateAssetSuppliers(asset.Id, addAssetDto.SupplierIds);

             }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
            var auditTrail = new AuditTrail()
            {
                AddedOn = DateTime.Now,
                Action = "Added",
                EntityType = "Asset",
                EntityName =asset.SerialNumber,
                UserId = UserId ?? "System"
            };

            await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

            await UnitOfWork.SaveChangeAsync();
            await UnitOfWork.CommitTransactionAsync();

            return await GetAssetByIdAsync(asset.SerialNumber);
        }
        #endregion

        #region Get Asset by ID
        public async Task<GetAssetResponseDTO> GetAssetByIdAsync(string serialNumber)
        {
            var asset = await UnitOfWork.readRepository<Asset>()
                .GetAsync(a => a.SerialNumber == serialNumber && 
                (a.IsDeleted == false || a.IsDeleted == null)&&
                a.Status!=AssetStatus.Retired.ToString());

            if (asset == null)
            {
                throw new KeyNotFoundException("Asset not found.");
            }

            var getAssetResponseDTO = new GetAssetResponseDTO
            {
                Id = asset.Id,
                Name = asset.Name,
                ModelNumber = asset.ModelNumber,
                SerialNumber = asset.SerialNumber,
                PurchaseDate = asset.PurchaseDate,
                PurchasePrice = asset.PurchasePrice,
                dicription=asset.dicription,
                WarrantyExpiryDate = asset.WarrantyExpiryDate,
                DepreciationDate=asset.DepreciationDate,
                Status = asset.Status,
                LocationName = asset.Location.Name,
                AssignedUserName = string.Concat(asset.AssignedUser.FirstName, " ", asset.AssignedUser.LastName),
                CategoryName = asset.Category.Name,
                 SupplierNames = asset.AssetsSuppliers.Select(s => s.Supplier.CompanyName).ToList(),
                ManfactureName = asset.Manufacturer.Name,
                AddedOnDate = asset.AddedOnDate,
                UpdatedDate = asset.UpdatedDate,
                Quantity=asset.Quantity,
                MinQuantityLimit=asset.MinQuantityLimit
            };

            var auditTrail = new AuditTrail()
            {
                AddedOn = DateTime.Now,
                Action = "Fetch",
                EntityType = "Asset",
                EntityName = asset.SerialNumber,
                UserId = UserId ?? "System"
            };

            await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

            await UnitOfWork.SaveChangeAsync();

            return getAssetResponseDTO;

        }
        #endregion

        #region Get All Assets
        public async Task<IList<GetAssetResponseDTO>> GetAllAssetsAsync()
        {
            var assets = await UnitOfWork.readRepository<Asset>()
                .GetAllAsync(a => a.IsDeleted == false || a.IsDeleted == null);
             

            var getAssetResponseDTO = assets.Select(a=>new GetAssetResponseDTO() 
            {
                Id = a.Id,
                Name = a.Name,
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchasePrice = a.PurchasePrice,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                DepreciationDate = a.DepreciationDate,
                Status = a.Status,
                dicription = a.dicription,
                LocationBarcode = a.Location.Barcode,
                LocationName = a.Location.Name,
                UserId = a.AssignedUserId.ToString ( ),
                AssignedUserName =string.Concat(a.AssignedUser.FirstName," ",a.AssignedUser.LastName),
                CategoryName = a.Category?.Name, 
                 SupplierNames = a.AssetsSuppliers.Select(s => s.Supplier.CompanyName).ToList(),
                ManfactureName = a.Manufacturer.Name,
                AddedOnDate = a.AddedOnDate,
                UpdatedDate = a.UpdatedDate,
                MinQuantityLimit=a.MinQuantityLimit,
                Quantity=a.Quantity
            } ).ToList();

            return getAssetResponseDTO;
        }
        #endregion


        #region Get All Assets
        public async Task<IList<GetAssetResponseDTO>> GetAllByPaginationAssetsAsync(int currentPage = 1, int pageSize = 10)
        {
            var assets = await UnitOfWork.readRepository<Asset>()
                .GetAllByPagningAsync(predicate: (a => a.IsDeleted == false || a.IsDeleted == null), pageSize: pageSize, currentPage: currentPage);


            var getAssetResponseDTO = assets.Select(a => new GetAssetResponseDTO()
            {
                Id = a.Id,
                Name = a.Name,
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchasePrice = a.PurchasePrice,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                DepreciationDate = a.DepreciationDate,
                Status = a.Status,
                dicription = a.dicription,
                LocationName = a.Location.Name,
                AssignedUserName = string.Concat(a.AssignedUser.FirstName, " ", a.AssignedUser.LastName),
                CategoryName = a.Category?.Name,
                 SupplierNames = a.AssetsSuppliers.Select(s => s.Supplier.CompanyName).ToList(),
                AddedOnDate = a.AddedOnDate,
                UpdatedDate = a.UpdatedDate,
                MinQuantityLimit = a.MinQuantityLimit,
                Quantity = a.Quantity

            } ).ToList();
            var auditTrail = new AuditTrail()
            {
                AddedOn = DateTime.Now,
                Action = "Added",
                EntityType = "Asset",
                EntityName = "All Assets",
                UserId = UserId ?? "System"
            };

            await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

            await UnitOfWork.SaveChangeAsync();
            return getAssetResponseDTO;
        }
        #endregion


        #region Update Asset
        public async Task<GetAssetResponseDTO> UpdateAssetAsync(string serialNumber, UpdateAssetRequestDTO updateAssetDto)
        {
            await UnitOfWork.BeginTransactionAsync();
            try
            {
                var existingAsset = await UnitOfWork.readRepository<Asset>()
                    .GetAsync(a => a.SerialNumber == serialNumber && 
                    (a.IsDeleted == false || a.IsDeleted == null)&&
                    a.Status!=AssetStatus.Retired.ToString());

                if (existingAsset == null)
                {
                    throw new KeyNotFoundException("Asset not found.");
                }

                #region Serial Number Not Updatable
                //var anotherAsset = await UnitOfWork.readRepository<Asset>()
                //    .GetAsync(a => a.SerialNumber == updateAssetDto.SerialNumber && a.Id != id);

                //if (anotherAsset != null)
                //{
                //    throw new InvalidOperationException("Another asset with the same serial number already exists.");
                //}
                #endregion

                // Validate foreign keys and category-subcategory relation
                await ValidateForeignKeysAndCategoryRelationAsync(updateAssetDto);


                var category = await UnitOfWork.readRepository<Category>()
                    .GetAsync(c => c.SerialCode == updateAssetDto.CategoryId &&
                    (c.IsDeleted == false || c.IsDeleted == null));


                #region Manually assign properties from the DTO to the existing asset
                existingAsset.Name = updateAssetDto.Name;
                existingAsset.ModelNumber = updateAssetDto.ModelNumber;
                existingAsset.SerialNumber = existingAsset.SerialNumber;
                existingAsset.PurchaseDate = updateAssetDto.PurchaseDate;
                existingAsset.PurchasePrice = updateAssetDto.PurchasePrice;
                existingAsset.WarrantyExpiryDate = updateAssetDto.WarrantyExpiryDate;
                existingAsset.Status = updateAssetDto.Status.ToString();
                existingAsset.LocationId = updateAssetDto.LocationId;
                existingAsset.AssignedUserId = updateAssetDto.AssignedUserId;
                existingAsset.CategoryId =  Convert.ToInt32(category.Id);
                 #endregion


                existingAsset.UpdatedDate = DateTime.Now;

                await UnitOfWork.writeRepository<Asset>().UpdateAsync(existingAsset.Id, existingAsset);
                await UnitOfWork.SaveChangeAsync();

                // Delete old supplier associations and add the new ones
                await DeleteAssetSuppliers(existingAsset.Id);
                await AddOrUpdateAssetSuppliers(existingAsset.Id, updateAssetDto.SupplierIds);

 

                var auditTrail = new AuditTrail()
                {
                    AddedOn = DateTime.Now,
                    Action = "Update",
                    EntityType = "Asset",
                    EntityName = existingAsset.SerialNumber,
                    UserId = UserId ?? "System"
                };

                await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

                await UnitOfWork.SaveChangeAsync();

                return await GetAssetByIdAsync(existingAsset.SerialNumber);
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion


        

        #region Withdraw Quantity from Asset
     
        public async Task<DTOs.AssetDTOs.GetAssetResponseDTO> WithdrawQuantityAsync ( int assetId, int quantityToWithdraw )
        {
            // Validation
            if ( assetId <= 0 )
            {
                throw new ArgumentException ( "Invalid asset ID.", nameof ( assetId ) );
            }

            if ( quantityToWithdraw <= 0 )
            {
                throw new ArgumentException ( "Quantity to withdraw must be greater than zero.", nameof ( quantityToWithdraw ) );
            }

            // Get the asset
            var asset = await UnitOfWork.readRepository<Asset> ( )
                .GetAsync ( predicate: a => a.Id == assetId && ( a.IsDeleted == false || a.IsDeleted == null ) );

            if ( asset == null )
            {
                throw new KeyNotFoundException ( $"Asset with ID {assetId} not found." );
            }

            // Check if withdrawal would result in negative quantity
            int remainingQuantity = asset.Quantity - quantityToWithdraw;

            if ( remainingQuantity < 0 )
            {
                throw new InvalidOperationException (
                    $"Cannot withdraw {quantityToWithdraw} units. Available quantity is {asset.Quantity}."
                );
            }

            // Update the quantity
            asset.Quantity = remainingQuantity;
            asset.UpdatedDate = DateTime.Now;

            // Save changes
            await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );
            await UnitOfWork.SaveChangeAsync ( );

            // Check if stock is low
            bool isLowStock = asset.Quantity <= asset.MinQuantityLimit;

            if ( isLowStock )
            {
                // notification Call here 

            }

            // Return response
            return new DTOs.AssetDTOs.GetAssetResponseDTO
            {
                Id = asset.Id,
                Name = asset.Name,
                SerialNumber = asset.SerialNumber,
                Quantity = asset.Quantity,
                MinQuantityLimit = asset.MinQuantityLimit,
             };
        }
        #endregion




        #region Delete Asset  
        public async Task DeleteAssetAsync(string serialNumber)
        {
            var existingAsset = await UnitOfWork.readRepository<Asset>()
                .GetAsync(a => a.SerialNumber == serialNumber && (a.IsDeleted == false || a.IsDeleted == null));

            if (existingAsset == null)
            {
                throw new KeyNotFoundException("Asset not found or already deleted.");
            }

            existingAsset.IsDeleted = true;
            existingAsset.DeletedDate = DateTime.Now;
            existingAsset.Status=AssetStatus.Retired.ToString();

            await UnitOfWork.BeginTransactionAsync();
            try
            {
                await UnitOfWork.writeRepository<Asset>().UpdateAsync(existingAsset.Id, existingAsset);
               await DeleteAssetSuppliers (existingAsset.Id);
                await UnitOfWork.SaveChangeAsync();
 
                var auditTrail = new AuditTrail()
                {
                    AddedOn = DateTime.Now,
                    Action = "Added",
                    EntityType = "Asset",
                    EntityName = existingAsset.SerialNumber,
                    UserId = UserId ?? "System"
                };

                await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

                await UnitOfWork.SaveChangeAsync();
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        #endregion

         
        #region Add or update supplier associations
        private async Task AddOrUpdateAssetSuppliers(int assetId, ICollection<int> supplierIds)
        {
            foreach (var supplierId in supplierIds)
            {
                var assetSupplier = new AssetsSuppliers
                {
                    AssetId = assetId,
                    SupplierId = supplierId
                };

                await UnitOfWork.writeRepository<AssetsSuppliers>().AddAsync(assetSupplier);
            }

            await UnitOfWork.SaveChangeAsync();
        }
        #endregion

        #region Delete existing asset-supplier relationships
        private async Task DeleteAssetSuppliers(int assetId)
        {
            var existingAssetSuppliers = await UnitOfWork.readRepository<AssetsSuppliers>()
                .GetAllAsync(As => As.AssetId == assetId);

            await UnitOfWork.writeRepository<AssetsSuppliers>().DeleteRangeAsync(existingAssetSuppliers);
            await UnitOfWork.SaveChangeAsync();
        }
        #endregion

        #region Validate foreign keys and ensure that the SubCategory belongs to the correct MainCategory
        private async Task ValidateForeignKeysAndCategoryRelationAsync(IAssetDTO assetDto)
        {
            if (await UnitOfWork.readRepository<Location>().GetAsync(l => l.Id == assetDto.LocationId 
            && (l.IsDeleted == false || l.IsDeleted==null)) == null)
            {
                throw new KeyNotFoundException("Location not found.");
            }

            if (await UnitOfWork.readRepository<User>().GetAsync(u => u.Id == assetDto.AssignedUserId
            && (u.IsDeleted == false || u.IsDeleted == null)) == null)
            {
                throw new KeyNotFoundException("Assigned user not found.");
            }

            if (await UnitOfWork.readRepository<Category>().GetAsync(c => c.SerialCode == assetDto.CategoryId.ToString()
            && (c.IsDeleted == false || c.IsDeleted == null)) == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            //var subCategory = await UnitOfWork.readRepository<Models.DbSets.SubCategory>().GetAsync(sc => sc.Id == assetDto.SubCategoryId
            //&& (sc.IsDeleted == false || sc.IsDeleted == null));
            //if (subCategory == null)
            //{
            //    throw new KeyNotFoundException("SubCategory not found.");
            //}

            //if (subCategory.MainCategoryId != assetDto.CategoryId)
            //{
            //    throw new InvalidOperationException("SubCategory does not belong to the selected MainCategory.");
            //}

            foreach (var supplierId in assetDto.SupplierIds)
            {
                if (await UnitOfWork.readRepository<Supplier>().GetAsync(s => s.Id == supplierId 
                && (s.IsDeleted == false || s.IsDeleted == null)) == null)
                {
                    throw new KeyNotFoundException($"Supplier with ID {supplierId} not found.");
                }
            }
        }
        #endregion
 

    }
}
