using AssetsManagementSystem.DTOs.AssetDTOs;
using AssetsManagementSystem.Models.DbSets;

namespace AssetsManagementSystem.Services.Assets
{

 
    public class AssetService : BaseClassForServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AssetService> _logger;
        private readonly UserManager<User> _userManager;
        public AssetService ( IUnitOfWork unitOfWork, Others.Interfaces.IAutoMapper.IMapper mapper, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ILogger<AssetService> logger, UserManager<User> userManager )
            : base ( unitOfWork, mapper, httpContextAccessor )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _userManager = userManager;
        }




        #region Get Assets for Current User - Improved

        #region Get Assets for Current User
        //public async Task<List<GetAssetResponseDTO>> GetAssetsForCurrentUser() 
        //{

        //    var currentUser =Guid.Parse(UserId);

        //    var assets=await UnitOfWork.readRepository<Asset>().GetAllAsync(predicate:a=>a.AssignedUserId==currentUser
        //                                                                                                &&
        //                                                                                                (a.IsDeleted==false||a.IsDeleted==null)
        //                                                                                                );


        //    var getAssetResponseDTO = assets.Select(a => new GetAssetResponseDTO()
        //    {
        //        Id = a.Id,
        //        Name = a.Name,
        //        ModelNumber = a.ModelNumber,
        //        SerialNumber = a.SerialNumber,
        //        PurchaseDate = a.PurchaseDate,
        //        PurchasePrice = a.PurchasePrice,
        //        WarrantyExpiryDate = a.WarrantyExpiryDate,
        //        Status = a.Status,
        //        dicription = a.dicription,
        //        LocationName = a.Location.Name,
        //        AssignedUserName = string.Concat(a.AssignedUser.FirstName, " ", a.AssignedUser.LastName),
        //        CategoryName = a.Category?.Name,
        //         SupplierNames = a.AssetsSuppliers.Select(s => s.Supplier.CompanyName).ToList(),
        //        ManfactureName=a.Manufacturer.Name,
        //        AddedOnDate = a.AddedOnDate,
        //        UpdatedDate = a.UpdatedDate
        //    }).ToList();


        //    var auditTrail = new AuditTrail()
        //    {
        //        AddedOn = DateTime.Now,
        //        Action = "Fetch",
        //        EntityType = "Asset",
        //        UserId = UserId ?? "System"
        //    };

        //    await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

        //   await UnitOfWork.SaveChangeAsync();

        //    return getAssetResponseDTO;

        //}
        #endregion

        public async Task<List<GetAssetResponseDTO>> GetAssetsForCurrentUser ( )
        {
            var currentUser = Guid.Parse ( UserId );

            // 1. جلب البيانات مع العلاقات (Includes) باستخدام Generic Repository
            var assets = await UnitOfWork.readRepository<Asset> ( ).GetAllAsync (
                predicate: a => a.AssignedUserId == currentUser && ( a.IsDeleted == false || a.IsDeleted == null ),
                include: source => source
                    .Include ( a => a.Location )
                    .Include ( a => a.Category )
                    .Include ( a => a.Manufacturer )
                    .Include ( a => a.AssignedUser )
                    .Include ( a => a.AssetsSuppliers ).ThenInclude ( asup => asup.Supplier ) // لجلب الموردين
            );

            // 2. تحويل البيانات (Mapping)
            var getAssetResponseDTO = assets.Select ( a => new GetAssetResponseDTO ( )
            {
                Id = a.Id,
                Name = a.Name,
                Barcode = a.Barcode,
                AssetType = a.AssetType.ToString ( ), // لو AssetType enum
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchasePrice = a.PurchasePrice,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                DepreciationDate = a.DepreciationDate,
                Status = a.Status,
                Description = a.Description,

                // التعامل الآمن مع القيم الفارغة (Null Safety)
                LocationName = a.Location?.Name ?? "Unknown",
                LocationId = a.LocationId,

                AssignedUserName = a.AssignedUser != null
                    ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                    : "N/A",
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
            } ).ToList ( );

            return getAssetResponseDTO;
        }

        #endregion


        #region Add Asset

        #region Old Add Asset Method
        //public async Task<GetAssetResponseDTO> AddAssetAsync(AddAssetRequestDTO addAssetDto)
        //{
        //    var existingAsset = await UnitOfWork.readRepository<Asset>()
        //        .GetAsync(a => a.SerialNumber == addAssetDto.SerialNumber);

        //    if (existingAsset != null)
        //    {
        //        throw new InvalidOperationException("An asset with the same serial number already exists.");
        //    }

        //    // Validate foreign keys and category-subcategory relation
        //    await ValidateForeignKeysAndCategoryRelationAsync(addAssetDto);

        //    var realcategoryid = await UnitOfWork.readRepository<Category> ( )
        //           .GetAsync ( c => c.SerialCode == addAssetDto.CategoryId.ToString ( ) );


        //    var asset = Mapper.Map<Asset, AddAssetRequestDTO> ( addAssetDto );
        //    asset.AddedOnDate = DateTime.Now;
        //    asset.Quantity = addAssetDto.Quantity;
        //    asset.MinQuantityLimit = addAssetDto.MinQuantityLimit;
        //    asset.CategoryId=realcategoryid.Id;

        //    await UnitOfWork.BeginTransactionAsync();
        //    try
        //    { 
        //        //realcategoryid = await UnitOfWork.readRepository<Category>()
        //        //    .GetAsync(c => c.SerialCode == addAssetDto.CategoryId.ToString());
        //        asset.CategoryId =realcategoryid.Id;
        //        await UnitOfWork.writeRepository<Asset>().AddAsync(asset);

        //        await UnitOfWork.SaveChangeAsync();

        //        // Add related records in AssetsSuppliers table
        //        await AddOrUpdateAssetSuppliers(asset.Id, addAssetDto.SupplierIds);

        //     }
        //    catch
        //    {
        //        await UnitOfWork.RollbackTransactionAsync();
        //        throw;
        //    }
        //    var auditTrail = new AuditTrail()
        //    {
        //        AddedOn = DateTime.Now,
        //        Action = "Added",
        //        EntityType = "Asset",
        //        EntityName =asset.SerialNumber,
        //        UserId = UserId ?? "System"
        //    };

        //    await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

        //    await UnitOfWork.SaveChangeAsync();
        //    await UnitOfWork.CommitTransactionAsync();

        //    return await GetAssetByIdAsync(asset.SerialNumber);
        //}

        #endregion

        public async Task<List<string>> AddAssetAsync ( AddAssetRequestDTO dto )
        {
            // 1. التحقق من العلاقات (Foreign Keys)
            await ValidateForeignKeysAndCategoryRelationAsync ( dto );

            // 2. جلب كود الفئة (Category Code) وتجهيز بادئة الباركود
            var category = await UnitOfWork.readRepository<Category> ( ).GetAsync (
                predicate: c => c.Id == dto.CategoryId,
                include: source => source.Include ( c => c.ParentCategory )
            );

            if ( category == null ) throw new InvalidOperationException ( "Category not found." );

            // مثال: IT-LAP أو FUR-CHR
            string barcodePrefix = category.ParentCategory != null
                ? $"{category.ParentCategory.SerialCode}-{category.SerialCode}"
                : category.SerialCode;

            // 3. معرفة آخر رقم تسلسلي (Smart Sequence Logic)
            // بنستخدم Paging عشان نجيب سطر واحد بس (الأخير) بدل تحميل كل البيانات
            var lastAssetList = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
                predicate: a => a.Barcode.StartsWith ( barcodePrefix ),
                orderby: q => q.OrderByDescending ( a => a.Barcode ),
                currentPage: 1,
                pageSize: 1
            );

            int currentSequence = 0;
            var lastAsset = lastAssetList.FirstOrDefault ( );
            if ( lastAsset != null )
            {
                var parts = lastAsset.Barcode.Split ( '-' );
                if ( parts.Length > 0 && int.TryParse ( parts.Last ( ), out int seq ) )
                {
                    currentSequence = seq;
                }
            }

            // 4. تجهيز القوائم للإضافة
            var assetsToAdd = new List<Asset> ( );
            var generatedBarcodes = new List<string> ( );

            // تحديد طريقة الإضافة: هل هي كمية مجمعة (ورق) أم أصول فردية (لابتوب)؟
            // الشرط: لو الكمية > 1 ومفيش سيريالات جاية، نعتبرها Bulk Consumable (سطر واحد)
            // غير كده بنعمل Loop (سطور متعددة)
            bool isBulkConsumable = dto.Quantity > 1 && ( dto.SerialNumbers == null || !dto.SerialNumbers.Any ( ) ) && dto.SerialNumber == null;

            int loopCount = isBulkConsumable ? 1 : dto.Quantity;

            for ( int i = 0; i < loopCount; i++ )
            {
                currentSequence++;
                // تكوين الباركود: Prefix + 6 Digits (e.g., IT-LAP-000055)
                string newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";

                var asset = Mapper.Map<Asset> ( dto );

                // ضبط البيانات التي لا تأتي من الـ Mapper
                asset.Barcode = newBarcode;
                asset.AddedOnDate = DateTime.Now;
                asset.CategoryId = dto.CategoryId; // تأكيد الـ ID

                if ( isBulkConsumable )
                {
                    // حالة المستهلكات: سطر واحد بالكمية كلها
                    asset.Quantity = dto.Quantity;
                    asset.SerialNumber = null;
                }
                else
                {
                    // حالة الأصول: كل سطر بكمية 1
                    asset.Quantity = 1;

                    // توزيع السيريالات
                    if ( dto.SerialNumbers != null && dto.SerialNumbers.Count > i )
                    {
                        asset.SerialNumber = dto.SerialNumbers [i];

                        // تحقق سريع من تكرار السيريال (اختياري هنا لو الداتابيز عليها Index)
                        var exists = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.SerialNumber == asset.SerialNumber );
                        if ( exists != null ) throw new InvalidOperationException ( $"Serial Number {asset.SerialNumber} already exists." );
                    }
                    else
                    {
                        // لو هو عنصر واحد بس والسيريال في الحقل العادي
                        asset.SerialNumber = dto.SerialNumber;
                    }
                }

                assetsToAdd.Add ( asset );
                generatedBarcodes.Add ( newBarcode );
            }

            // 5. الحفظ في قاعدة البيانات (Transaction)
            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                // إضافة الكل مرة واحدة (Performance Boost)
                await UnitOfWork.writeRepository<Asset> ( ).AddRangeAsync ( assetsToAdd );
                await UnitOfWork.SaveChangeAsync ( ); // عشان الـ IDs تتولد

                // إضافة الموردين (Suppliers)
                if ( dto.SupplierIds != null && dto.SupplierIds.Any ( ) )
                {
                    foreach ( var addedAsset in assetsToAdd )
                    {
                        // استدعاء دالتك الخاصة بإضافة الموردين
                        await AddOrUpdateAssetSuppliers ( addedAsset.Id, dto.SupplierIds );
                    }
                    // حفظ علاقات الموردين
                    await UnitOfWork.SaveChangeAsync ( );
                }

                // تسجيل Audit Trail (سجل واحد للعملية كلها لتخفيف الحمل)
                var auditTrail = new AuditTrail ( )
                {
                    AddedOn = DateTime.Now,
                    Action = "Add",
                    EntityType = "Asset",
                    EntityName = $"{loopCount} Assets added to {category.Name} (Batch)",
                    UserId = UserId ?? "System"
                };

                await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
                await UnitOfWork.SaveChangeAsync ( );

                await UnitOfWork.CommitTransactionAsync ( );

                // إرجاع لستة الباركودات للفرونت إند (للطباعة)
                return generatedBarcodes;
            }
            catch ( Exception )
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion

        #region Get Asset by Barcode
        // غيرنا الاسم والبارامتر لـ Barcode لأنه الهوية الأساسية
        public async Task<GetAssetResponseDTO> GetAssetByBarcodeAsync ( string barcode )
        {
            // 1. استخدام Includes لجلب البيانات المرتبطة
            var asset = await UnitOfWork.readRepository<Asset> ( )
                .GetAsync (
                    predicate: a => a.Barcode == barcode &&
                                   ( a.IsDeleted == false || a.IsDeleted == null ) &&
                                   a.Status != AssetStatus.Retired.ToString ( ),

                    // لازم نعمل Include لكل الجداول اللي بنعرض اسمها
                    include: source => source
                        .Include ( a => a.Location )
                        .Include ( a => a.Category )
                        .Include ( a => a.Manufacturer )
                        .Include ( a => a.AssignedUser )
                        .Include ( a => a.AssetsSuppliers ).ThenInclude ( s => s.Supplier )
                );

            if ( asset == null )
            {
                throw new KeyNotFoundException ( $"Asset with barcode '{barcode}' not found." );
            }

            // 2. التحويل للـ DTO مع مراعاة الـ Nulls
            var getAssetResponseDTO = new GetAssetResponseDTO
            {
                Id = asset.Id,
                Name = asset.Name,
                Barcode = asset.Barcode, // مهم جداً نرجعه
                AssetType = asset.AssetType.ToString ( ),
                ModelNumber = asset.ModelNumber,
                SerialNumber = asset.SerialNumber,
                PurchaseDate = asset.PurchaseDate,
                PurchasePrice = asset.PurchasePrice,
                Description = asset.Description,
                WarrantyExpiryDate = asset.WarrantyExpiryDate,
                DepreciationDate = asset.DepreciationDate,
                Status = asset.Status,

                // Null Safety Checks (?. operator)
                LocationName = asset.Location?.Name ?? "Unknown",
                LocationId = asset.LocationId, // لو محتاجه للفرونت
                LocationBarcode = asset.Location?.Barcode ?? "",

                // التعامل مع الموظف (ممكن يكون Null لو في المخزن)
                AssignedUserName = asset.AssignedUser != null
                    ? $"{asset.AssignedUser.FirstName} {asset.AssignedUser.LastName}"
                    : "In Stock / Not Assigned",
                AssignedUserId = asset.AssignedUserId,

                CategoryName = asset.Category?.Name ?? "Unknown",
                CategoryId = asset.CategoryId,

                // التعامل مع المصنع (ممكن يكون Null)
                ManufacturerName = asset.Manufacturer?.Name ?? "N/A",
                ManufacturerId = asset.ManufacturerId,

                SupplierNames = asset.AssetsSuppliers?.Select ( s => s.Supplier?.CompanyName ).ToList ( ) ?? new List<string> ( ),

                AddedOnDate = asset.AddedOnDate,
                UpdatedDate = asset.UpdatedDate,
                Quantity = asset.Quantity,
                MinQuantityLimit = asset.MinQuantityLimit
            };

            // 3. حذفنا الـ Audit Trail من دالة الـ Get لتحسين الأداء

            return getAssetResponseDTO;
        }
        #endregion

        #region Get All Assets
        public async Task<IList<GetAssetResponseDTO>> GetAllAssetsAsync ( )
        {
            // 1. لازم نستخدم Includes عشان نجيب بيانات الجداول المرتبطة
            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync (
                    predicate: a => a.IsDeleted == false || a.IsDeleted == null,
                    include: source => source
                        .Include ( a => a.Location )
                        .Include ( a => a.Category )
                        .Include ( a => a.Manufacturer )
                        .Include ( a => a.AssignedUser )
                        .Include ( a => a.AssetsSuppliers ).ThenInclude ( s => s.Supplier )
                );

            var getAssetResponseDTO = assets.Select ( a => new GetAssetResponseDTO ( )
            {
                Id = a.Id,
                Name = a.Name,
                Barcode = a.Barcode, // نسينا ده في الكود القديم
                AssetType = a.AssetType.ToString ( ),
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchasePrice = a.PurchasePrice,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                DepreciationDate = a.DepreciationDate,
                Status = a.Status,
                Description = a.Description, // تم تصحيح الاسم

                // Null Safety Checks (?. operator)
                LocationBarcode = a.Location?.Barcode ?? "", // لو متاح في الانتيي
                LocationName = a.Location?.Name ?? "Unknown",
                LocationId = a.LocationId,

                AssignedUserId = a.AssignedUserId, // Nullable Handle
                AssignedUserName = a.AssignedUser != null
                    ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                    : "In Stock",

                CategoryName = a.Category?.Name ?? "Unknown",
                CategoryId = a.CategoryId,

                ManufacturerName = a.Manufacturer?.Name ?? "N/A", // تم تصحيح الاسم
                ManufacturerId = a.ManufacturerId,

                SupplierNames = a.AssetsSuppliers?.Select ( s => s.Supplier?.CompanyName ).ToList ( ) ?? new List<string> ( ),

                AddedOnDate = a.AddedOnDate,
                UpdatedDate = a.UpdatedDate,
                MinQuantityLimit = a.MinQuantityLimit,
                Quantity = a.Quantity
            } ).ToList ( );

            return getAssetResponseDTO;
        }
        #endregion

        #region Get All Assets With Pagination
        public async Task<IList<GetAssetResponseDTO>> GetAllByPaginationAssetsAsync ( int currentPage = 1, int pageSize = 10 )
        {
            // 1. نفس الكلام، لازم Includes مع الـ Pagination
            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllByPagningAsync (
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

            var getAssetResponseDTO = assets.Select ( a => new GetAssetResponseDTO ( )
            {
                Id = a.Id,
                Name = a.Name,
                Barcode = a.Barcode,
                AssetType = a.AssetType.ToString ( ),
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchasePrice = a.PurchasePrice,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                DepreciationDate = a.DepreciationDate,
                Status = a.Status,
                Description = a.Description,

                LocationName = a.Location?.Name ?? "Unknown",
                LocationId = a.LocationId,
                LocationBarcode = a.Location?.Barcode ?? "",

                AssignedUserName = a.AssignedUser != null
                    ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
                    : "In Stock",
                AssignedUserId = a.AssignedUserId,

                CategoryName = a.Category?.Name ?? "Unknown",
                CategoryId = a.CategoryId,

                ManufacturerName = a.Manufacturer?.Name ?? "N/A",
                ManufacturerId = a.ManufacturerId,

                SupplierNames = a.AssetsSuppliers?.Select ( s => s.Supplier?.CompanyName ).ToList ( ) ?? new List<string> ( ),

                AddedOnDate = a.AddedOnDate,
                UpdatedDate = a.UpdatedDate,
                MinQuantityLimit = a.MinQuantityLimit,
                Quantity = a.Quantity
            } ).ToList ( );

            // تم حذف الـ Audit Trail من هنا تماماً
            // GET Methods should be Idempotent (Read-Only)

            return getAssetResponseDTO;
        }
        #endregion

        #region Update Asset
        // التعديل 1: نستخدم Barcode كمعرف أساسي بدلاً من SerialNumber
        public async Task<GetAssetResponseDTO> UpdateAssetAsync ( string barcode, UpdateAssetRequestDTO dto )
        {
            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                // 1. البحث عن الأصل بالباركود
                var existingAsset = await UnitOfWork.readRepository<Asset> ( )
                    .GetAsync ( a => a.Barcode == barcode &&
                                   ( a.IsDeleted == false || a.IsDeleted == null ) &&
                                   a.Status != AssetStatus.Retired.ToString ( ) );

                if ( existingAsset == null )
                {
                    throw new KeyNotFoundException ( $"Asset with barcode '{barcode}' not found." );
                }

                // 2. التحقق من العلاقات (Foreign Keys)
                // * ملحوظة: تأكد إن الـ Validation Function بتقبل الـ Update DTO
                await ValidateForeignKeysAndCategoryRelationAsync ( dto );

                // 3. تحديث البيانات (Mapping)
                existingAsset.Name = dto.Name;
                existingAsset.ModelNumber = dto.ModelNumber;
                existingAsset.Description = dto.Description; // تصحيح الاسم
                existingAsset.PurchaseDate = dto.PurchaseDate;
                existingAsset.PurchasePrice = dto.PurchasePrice;
                existingAsset.WarrantyExpiryDate = dto.WarrantyExpiryDate;
                existingAsset.DepreciationDate = dto.DepreciationDate;
                existingAsset.Status = dto.Status.ToString ( ); // Enum to String

                // تحديث العلاقات
                existingAsset.LocationId = dto.LocationId;

                // التعديل 2: استخدام ID مباشرة دون الحاجة لجلب الكائن
                existingAsset.CategoryId = dto.CategoryId;
                existingAsset.ManufacturerId = dto.ManufacturerId;

                // التعامل الذكي مع المستخدم (Assign/Unassign)
                existingAsset.AssignedUserId = dto.AssignedUserId;

                // التعامل مع السيريال (لو اتغير)
                // السيريال قابل للتعديل، ولكن يجب التأكد من عدم تكراره إذا لم يكن فارغاً
                if ( existingAsset.SerialNumber != dto.SerialNumber )
                {
                    if ( !string.IsNullOrEmpty ( dto.SerialNumber ) )
                    {
                        var duplicateCheck = await UnitOfWork.readRepository<Asset> ( )
                            .GetAsync ( a => a.SerialNumber == dto.SerialNumber && a.Id != existingAsset.Id );

                        if ( duplicateCheck != null )
                            throw new InvalidOperationException ( $"Serial Number '{dto.SerialNumber}' is already used by another asset." );
                    }
                    existingAsset.SerialNumber = dto.SerialNumber;
                }

                existingAsset.UpdatedDate = DateTime.Now;
                // يمكن تحديث حد الطلب أيضاً
                existingAsset.MinQuantityLimit = dto.MinQuantityLimit;

                // 4. الحفظ المبدئي للأصل
                await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( existingAsset.Id, existingAsset );
                await UnitOfWork.SaveChangeAsync ( );

                // 5. تحديث الموردين (Replace Logic)
                // بنمسح القديم ونحط الجديد (أسهل طريقة في الـ Update)
                if ( dto.SupplierIds != null )
                {
                    // تأكد إن عندك دالة DeleteAssetSuppliers بتمسح من جدول الربط بناءً على AssetId
                    await DeleteAssetSuppliers ( existingAsset.Id );

                    if ( dto.SupplierIds.Any ( ) )
                    {
                        await AddOrUpdateAssetSuppliers ( existingAsset.Id, dto.SupplierIds );
                    }
                    await UnitOfWork.SaveChangeAsync ( );
                }

                // 6. Audit Trail
                var auditTrail = new AuditTrail ( )
                {
                    AddedOn = DateTime.Now,
                    Action = "Update",
                    EntityType = "Asset",
                    EntityName = existingAsset.Barcode, // نستخدم الباركود كمرجع
                    UserId = UserId ?? "System",
                    // يفضل إضافة تفاصيل التعديل هنا لو أمكن
                };

                await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
                await UnitOfWork.SaveChangeAsync ( );

                await UnitOfWork.CommitTransactionAsync ( );

                // 7. إرجاع البيانات الجديدة باستخدام دالة الباركود الجديدة
                return await GetAssetByBarcodeAsync ( existingAsset.Barcode );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
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
                
                _logger.LogWarning ( "LOW STOCK: Asset '{AssetName}' (ID: {AssetId}) reached {Quantity} units.",
                    asset.Name, asset.Id, asset.Quantity );

                try
                {
                     
                    _logger.LogInformation ( "Fetching admin users for low stock notification..." );

                    
                    var adminsInRole = await _userManager.GetUsersInRoleAsync ( "Admin" );

                     
                    var userIdsToNotify = adminsInRole
                        .Where ( u => u.IsDeleted == false || u.IsDeleted == null )  
                        .Select ( u => u.Id.ToString ( ) ) 
                        .ToList ( );

                    if ( userIdsToNotify.Any ( ) == false )
                    {
                        _logger.LogWarning ( "No active admin users found to notify for low stock." );
                    }
                    else
                    {
                        _logger.LogInformation ( "Found {AdminCount} active admin(s) to notify.", userIdsToNotify.Count );

                         
                        var httpClient = _httpClientFactory.CreateClient ( );

                         
                        var metadataObject = new
                        {
                            assetId = asset.Id,
                            assetName = asset.Name,
                            serialNumber = asset.SerialNumber,
                            currentQuantity = asset.Quantity,
                            minLimit = asset.MinQuantityLimit
                        };

                         
                        var requestPayload = new
                        {
                            userIds = userIdsToNotify, 
                            title = $"🚨 Low Stock: {asset.Name}",
                            message = $"Stock for '{asset.Name}' (SN: {asset.SerialNumber}) is low. " +
                                      $"Current: {asset.Quantity}, Limit: {asset.MinQuantityLimit}.",
                            notificationType = "Warning",
                            category = "Inventory",
                            priority = 3,
                            actionUrl = $"/assets/manage/{asset.Id}",
                            metadata = System.Text.Json.JsonSerializer.Serialize ( metadataObject )
                        };

                        
                        var response = await httpClient.PostAsJsonAsync (
                            "http://10.10.10.48:7000/api/trailing/notifications/bulk",
                            requestPayload
                        );

                        if ( response.IsSuccessStatusCode )
                        {
                            var responseString = await response.Content.ReadAsStringAsync ( );
                            _logger.LogInformation ( "Low stock notification sent successfully to admins. Response: {Response}", responseString );
                        }
                        else
                        {
                            _logger.LogError ( "Failed to send low stock notification to admins. Status: {StatusCode}, Reason: {Reason}",
                                response.StatusCode, response.ReasonPhrase );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    _logger.LogError ( ex, "An error occurred while sending low stock notification for Asset ID {AssetId}.", asset.Id );
                }
                
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



        #region Delete (Retire) Asset
        public async Task<bool> DeleteAssetAsync ( string barcode )
        {
            // 1. البحث عن الأصل
            var asset = await UnitOfWork.readRepository<Asset> ( )
                .GetAsync ( a => a.Barcode == barcode && ( a.IsDeleted == false || a.IsDeleted == null ) );

            if ( asset == null )
                throw new KeyNotFoundException ( $"Asset with barcode '{barcode}' not found." );

            // 2. التحقق من إمكانية الحذف
            // مثلاً: مينفعش تمسح أصل وهو عهدة مع موظف حالياً
            if ( asset.Status == AssetStatus.Active.ToString ( ) )
            {
                throw new InvalidOperationException ( "Cannot delete an asset that is currently In Use. Please unassign it first." );
            }

            // 3. تنفيذ الحذف المنطقي (Soft Delete)
            asset.IsDeleted = true;
            asset.Status = AssetStatus.Retired.ToString ( ); // تغيير الحالة لـ "مكهن" أو "خارج الخدمة"
            asset.UpdatedDate = DateTime.Now;

            // 4. الحفظ
            await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );

            // 5. Audit Trail
            var auditTrail = new AuditTrail ( )
            {
                AddedOn = DateTime.Now,
                Action = "Delete (Retire)",
                EntityType = "Asset",
                EntityName = asset.Barcode,
                UserId = UserId ?? "System",
             };
            await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );

            await UnitOfWork.SaveChangeAsync ( );

            return true;
        }
        #endregion

        #region Add or update supplier associations
        private async Task AddOrUpdateAssetSuppliers ( int assetId, ICollection<int> supplierIds )
        {
             if ( supplierIds == null || !supplierIds.Any ( ) ) return;

             var distinctSupplierIds = supplierIds.Distinct ( ).ToList ( );

            var suppliersToAdd = new List<AssetsSuppliers> ( );

            foreach ( var supplierId in distinctSupplierIds )
            {
                var assetSupplier = new AssetsSuppliers
                {
                    AssetId = assetId,
                    SupplierId = supplierId
                };
                suppliersToAdd.Add ( assetSupplier );
            }

           
            await UnitOfWork.writeRepository<AssetsSuppliers> ( ).AddRangeAsync ( suppliersToAdd );

             await UnitOfWork.SaveChangeAsync ( );
        }
        #endregion

        #region Delete existing asset-supplier relationships
        private async Task DeleteAssetSuppliers ( int assetId )
        {
            var existingAssetSuppliers = await UnitOfWork.readRepository<AssetsSuppliers> ( )
                .GetAllAsync ( As => As.AssetId == assetId );

            // Check before delete
            if ( existingAssetSuppliers != null && existingAssetSuppliers.Any ( ) )
            {
                await UnitOfWork.writeRepository<AssetsSuppliers> ( ).DeleteRangeAsync ( existingAssetSuppliers );
                await UnitOfWork.SaveChangeAsync ( );
            }
        }
        #endregion






        #region Validate Foreign Keys
        private async Task ValidateForeignKeysAndCategoryRelationAsync ( AddAssetRequestDTO assetDto )
        {
            // 1. Location (Required)
            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Id == assetDto.LocationId && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null )
                throw new KeyNotFoundException ( $"Location with ID {assetDto.LocationId} not found." );

            // 2. Category (Required - Fix: Search by ID not Code)
            // غيرنا البحث هنا لـ ID لأن الـ DTO بيبعت int
            var category = await UnitOfWork.readRepository<Category> ( )
                .GetAsync ( c => c.Id == assetDto.CategoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

            if ( category == null )
                throw new KeyNotFoundException ( $"Category with ID {assetDto.CategoryId} not found." );

            // 3. Assigned User (Nullable - Fix: Check only if provided)
            // لازم نتأكد الأول إن ليه قيمة قبل ما نروح الداتابيز
            if ( assetDto.AssignedUserId.HasValue )
            {
                var user = await UnitOfWork.readRepository<User> ( )
                    .GetAsync ( u => u.Id == assetDto.AssignedUserId && ( u.IsDeleted == false || u.IsDeleted == null ) );

                if ( user == null )
                    throw new KeyNotFoundException ( "Assigned user not found." );
            }

            // 4. Manufacturer (Nullable - Added Missing Validation)
            if ( assetDto.ManufacturerId.HasValue )
            {
                var manufacturer = await UnitOfWork.readRepository<Manufacturer> ( )
                    .GetAsync ( m => m.Id == assetDto.ManufacturerId && ( m.IsDeleted == false || m.IsDeleted == null ) );

                if ( manufacturer == null )
                    throw new KeyNotFoundException ( "Manufacturer not found." );
            }

            // 5. Suppliers (Performance Fix: Batch Check)
            // بدل ما نلف ونكلم الداتابيز 10 مرات، بنكلمها مرة واحدة ونعد اللي لقيناهم
            if ( assetDto.SupplierIds != null && assetDto.SupplierIds.Any ( ) )
            {
                // بنشيل التكرار من الـ IDs اللي جاية
                var distinctIds = assetDto.SupplierIds.Distinct ( ).ToList ( );

                // بنشوف كام واحد من الـ IDs دي موجود فعلاً في الداتابيز
                var existingCount = await UnitOfWork.readRepository<Supplier> ( )
                    .CountAsync ( s => distinctIds.Contains ( s.Id ) && ( s.IsDeleted == false || s.IsDeleted == null ) );

                // لو عدد اللي لقيناهم أقل من العدد اللي باعتينه، يبقى فيه ID غلط
                if ( existingCount != distinctIds.Count )
                {
                    throw new KeyNotFoundException ( "One or more Supplier IDs are invalid or deleted." );
                }
            }
        }
        #endregion

        #region Validate Foreign Keys
        private async Task ValidateForeignKeysAndCategoryRelationAsync ( UpdateAssetRequestDTO assetDto )
        {
            // 1. Location (Required)
            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Id == assetDto.LocationId && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null )
                throw new KeyNotFoundException ( $"Location with ID {assetDto.LocationId} not found." );

            // 2. Category (Required - Fix: Search by ID not Code)
            // غيرنا البحث هنا لـ ID لأن الـ DTO بيبعت int
            var category = await UnitOfWork.readRepository<Category> ( )
                .GetAsync ( c => c.Id == assetDto.CategoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

            if ( category == null )
                throw new KeyNotFoundException ( $"Category with ID {assetDto.CategoryId} not found." );

            // 3. Assigned User (Nullable - Fix: Check only if provided)
            // لازم نتأكد الأول إن ليه قيمة قبل ما نروح الداتابيز
            if ( assetDto.AssignedUserId.HasValue )
            {
                var user = await UnitOfWork.readRepository<User> ( )
                    .GetAsync ( u => u.Id == assetDto.AssignedUserId && ( u.IsDeleted == false || u.IsDeleted == null ) );

                if ( user == null )
                    throw new KeyNotFoundException ( "Assigned user not found." );
            }

            // 4. Manufacturer (Nullable - Added Missing Validation)
            if ( assetDto.ManufacturerId.HasValue )
            {
                var manufacturer = await UnitOfWork.readRepository<Manufacturer> ( )
                    .GetAsync ( m => m.Id == assetDto.ManufacturerId && ( m.IsDeleted == false || m.IsDeleted == null ) );

                if ( manufacturer == null )
                    throw new KeyNotFoundException ( "Manufacturer not found." );
            }

            // 5. Suppliers (Performance Fix: Batch Check)
            // بدل ما نلف ونكلم الداتابيز 10 مرات، بنكلمها مرة واحدة ونعد اللي لقيناهم
            if ( assetDto.SupplierIds != null && assetDto.SupplierIds.Any ( ) )
            {
                // بنشيل التكرار من الـ IDs اللي جاية
                var distinctIds = assetDto.SupplierIds.Distinct ( ).ToList ( );

                // بنشوف كام واحد من الـ IDs دي موجود فعلاً في الداتابيز
                var existingCount = await UnitOfWork.readRepository<Supplier> ( )
                    .CountAsync ( s => distinctIds.Contains ( s.Id ) && ( s.IsDeleted == false || s.IsDeleted == null ) );

                // لو عدد اللي لقيناهم أقل من العدد اللي باعتينه، يبقى فيه ID غلط
                if ( existingCount != distinctIds.Count )
                {
                    throw new KeyNotFoundException ( "One or more Supplier IDs are invalid or deleted." );
                }
            }
        }
        #endregion

    }
}
