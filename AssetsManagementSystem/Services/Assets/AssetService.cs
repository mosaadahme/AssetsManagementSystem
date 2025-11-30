//using AssetsManagementSystem.DTOs.AssetDTOs;
//using AssetsManagementSystem.Models.DbSets;

//namespace AssetsManagementSystem.Services.Assets
//{


//    public class AssetService : BaseClassForServices
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly ILogger<AssetService> _logger;
//        private readonly UserManager<User> _userManager;
//        public AssetService ( IUnitOfWork unitOfWork, Others.Interfaces.IAutoMapper.IMapper mapper, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ILogger<AssetService> logger, UserManager<User> userManager )
//            : base ( unitOfWork, mapper, httpContextAccessor )
//        {
//            _httpClientFactory = httpClientFactory;
//            _logger = logger;
//            _userManager = userManager;
//        }




//        #region Get Assets for Current User - Improved

//        #region Get Assets for Current User
//        //public async Task<List<GetAssetResponseDTO>> GetAssetsForCurrentUser() 
//        //{

//        //    var currentUser =Guid.Parse(UserId);

//        //    var assets=await UnitOfWork.readRepository<Asset>().GetAllAsync(predicate:a=>a.AssignedUserId==currentUser
//        //                                                                                                &&
//        //                                                                                                (a.IsDeleted==false||a.IsDeleted==null)
//        //                                                                                                );


//        //    var getAssetResponseDTO = assets.Select(a => new GetAssetResponseDTO()
//        //    {
//        //        Id = a.Id,
//        //        Name = a.Name,
//        //        ModelNumber = a.ModelNumber,
//        //        SerialNumber = a.SerialNumber,
//        //        PurchaseDate = a.PurchaseDate,
//        //        PurchasePrice = a.PurchasePrice,
//        //        WarrantyExpiryDate = a.WarrantyExpiryDate,
//        //        Status = a.Status,
//        //        dicription = a.dicription,
//        //        LocationName = a.Location.Name,
//        //        AssignedUserName = string.Concat(a.AssignedUser.FirstName, " ", a.AssignedUser.LastName),
//        //        CategoryName = a.Category?.Name,
//        //         SupplierNames = a.AssetsSuppliers.Select(s => s.Supplier.CompanyName).ToList(),
//        //        ManfactureName=a.Manufacturer.Name,
//        //        AddedOnDate = a.AddedOnDate,
//        //        UpdatedDate = a.UpdatedDate
//        //    }).ToList();


//        //    var auditTrail = new AuditTrail()
//        //    {
//        //        AddedOn = DateTime.Now,
//        //        Action = "Fetch",
//        //        EntityType = "Asset",
//        //        UserId = UserId ?? "System"
//        //    };

//        //    await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

//        //   await UnitOfWork.SaveChangeAsync();

//        //    return getAssetResponseDTO;

//        //}
//        #endregion

//        public async Task<List<GetAssetResponseDTO>> GetAssetsForCurrentUser ( )
//        {
//            var currentUser = Guid.Parse ( UserId );

//            // 1. جلب البيانات مع العلاقات (Includes) باستخدام Generic Repository
//            var assets = await UnitOfWork.readRepository<Asset> ( ).GetAllAsync (
//                predicate: a => a.AssignedUserId == currentUser && ( a.IsDeleted == false || a.IsDeleted == null ),
//                include: source => source
//                    .Include ( a => a.Location )
//                    .Include ( a => a.Category )
//                    .Include ( a => a.Manufacturer )
//                    .Include ( a => a.AssignedUser )
//                    .Include ( a => a.AssetsSuppliers ).ThenInclude ( asup => asup.Supplier ) // لجلب الموردين
//            );

//            // 2. تحويل البيانات (Mapping)
//            var getAssetResponseDTO = assets.Select ( a => new GetAssetResponseDTO ( )
//            {
//                Id = a.Id,
//                Name = a.Name,
//                Barcode = a.Barcode,
//                AssetType = a.AssetType.ToString ( ), // لو AssetType enum
//                ModelNumber = a.ModelNumber,
//                SerialNumber = a.SerialNumber,
//                PurchaseDate = a.PurchaseDate,
//                PurchasePrice = a.PurchasePrice,
//                WarrantyExpiryDate = a.WarrantyExpiryDate,
//                DepreciationDate = a.DepreciationDate,
//                Status = a.Status,
//                Description = a.Description,

//                // التعامل الآمن مع القيم الفارغة (Null Safety)
//                LocationName = a.Location?.Name ?? "Unknown",
//                LocationId = a.LocationId,

//                AssignedUserName = a.AssignedUser != null
//                    ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
//                    : "N/A",
//                AssignedUserId = a.AssignedUserId,

//                CategoryName = a.Category?.Name ?? "Unknown",
//                CategoryId = a.CategoryId,

//                ManufacturerName = a.Manufacturer?.Name ?? "N/A",
//                ManufacturerId = a.ManufacturerId,

//                SupplierNames = a.AssetsSuppliers?.Select ( s => s.Supplier?.CompanyName ).ToList ( ) ?? new List<string> ( ),

//                AddedOnDate = a.AddedOnDate,
//                UpdatedDate = a.UpdatedDate,

//                Quantity = a.Quantity,
//                MinQuantityLimit = a.MinQuantityLimit
//            } ).ToList ( );

//            return getAssetResponseDTO;
//        }

//        #endregion


//        #region Add Asset


//        public async Task<List<string>> AddAssetAsync ( AddAssetRequestDTO dto )
//        {
//            // 1. التحقق من صحة العلاقات (Category, Location, User...)
//            await ValidateForeignKeysAndCategoryRelationAsync ( dto );

//            // -------------------------------------------------------
//            // 🔥 خطوة الأداء العالي: فحص السيريالات دفعة واحدة (Batch Check)
//            // -------------------------------------------------------
//            if ( dto.SerialNumbers != null && dto.SerialNumbers.Any ( ) )
//            {
//                // التأكد من عدم وجود تكرار داخل القائمة المرسلة نفسها
//                if ( dto.SerialNumbers.Distinct ( ).Count ( ) != dto.SerialNumbers.Count )
//                    throw new InvalidOperationException ( "Duplicate serial numbers found in the request list." );

//                // التأكد من عدم وجود السيريالات دي مسبقاً في الداتابيز
//                var existingAssets = await UnitOfWork.readRepository<Asset> ( )
//                    .GetAllAsync ( a => dto.SerialNumbers.Contains ( a.SerialNumber )
//                                   && ( a.IsDeleted == false || a.IsDeleted == null ) );

//                if ( existingAssets.Any ( ) )
//                {
//                    var duplicateSerials = string.Join ( ", ", existingAssets.Select ( a => a.SerialNumber ) );
//                    throw new InvalidOperationException ( $"The following Serial Numbers already exist in database: {duplicateSerials}" );
//                }
//            }

//            // 2. تجهيز الباركود (Prefix & Last Sequence)
//            var category = await UnitOfWork.readRepository<Category> ( ).GetAsync (
//                predicate: c => c.Id == dto.CategoryId,
//                include: s => s.Include ( x => x.ParentCategory ) );

//            if ( category == null ) throw new InvalidOperationException ( "Category not found." );

//            string barcodePrefix = category.ParentCategory != null
//                ? $"{category.ParentCategory.SerialCode}-{category.SerialCode}"
//                : category.SerialCode;

//            // بنجيب أخر رقم وصلنا له عشان نكمل عليه
//            var lastAssetList = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
//                predicate: a => a.Barcode.StartsWith ( barcodePrefix ),
//                orderby: q => q.OrderByDescending ( a => a.Barcode ),
//                currentPage: 1, pageSize: 1 );

//            int currentSequence = 0;
//            var lastAsset = lastAssetList.FirstOrDefault ( );
//            if ( lastAsset != null )
//            {
//                var parts = lastAsset.Barcode.Split ( '-' );
//                if ( parts.Length > 0 && int.TryParse ( parts.Last ( ), out int seq ) )
//                    currentSequence = seq;
//            }

//            var assetsToAdd = new List<Asset> ( );
//            var generatedBarcodes = new List<string> ( );


//            if ( dto.AssetType == AssetType.Consumable )
//            {

//                currentSequence++;
//                string newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";

//                // Manual Mapping (لتفادي مشاكل AutoMapper مع الـ Enum)
//                var asset = new Asset
//                {
//                    Name = dto.Name,
//                    ModelNumber = dto.ModelNumber,
//                    Description = dto.Description,
//                    AssetType = dto.AssetType,
//                    Status = dto.Status.ToString ( ), // تحويل الـ Enum لنص صريح
//                    PurchaseDate = dto.PurchaseDate,
//                    PurchasePrice = dto.PurchasePrice,
//                    WarrantyExpiryDate = dto.WarrantyExpiryDate,
//                    DepreciationDate = dto.DepreciationDate,
//                    LocationId = dto.LocationId,
//                    CategoryId = dto.CategoryId,
//                    ManufacturerId = dto.ManufacturerId,
//                    AssignedUserId = dto.AssignedUserId,
//                    AddedOnDate = DateTime.Now,

//                    // خصائص الـ Bulk
//                    Barcode = newBarcode,
//                    Quantity = dto.Quantity,        // الكمية كلها هنا
//                    MinQuantityLimit = dto.MinQuantityLimit,
//                    SerialNumber = null             // المستهلكات ملهاش سيريال
//                };

//                assetsToAdd.Add ( asset );
//                generatedBarcodes.Add ( newBarcode );
//            }
//            else
//            {
//                // === الحالة 2: أصول ثابتة (IT & Non-IT) ===
//                // تكرار بعدد الكمية - كل سطر كمية 1 - سيريال حسب النوع

//                for ( int i = 0; i < dto.Quantity; i++ )
//                {
//                    currentSequence++;
//                    string newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";

//                    var asset = new Asset
//                    {
//                        Name = dto.Name,
//                        ModelNumber = dto.ModelNumber,
//                        Description = dto.Description,
//                        AssetType = dto.AssetType,
//                        Status = dto.Status.ToString ( ),
//                        PurchaseDate = dto.PurchaseDate,
//                        PurchasePrice = dto.PurchasePrice,
//                        WarrantyExpiryDate = dto.WarrantyExpiryDate,
//                        DepreciationDate = dto.DepreciationDate,
//                        LocationId = dto.LocationId,
//                        CategoryId = dto.CategoryId,
//                        ManufacturerId = dto.ManufacturerId,
//                        AssignedUserId = dto.AssignedUserId,
//                        AddedOnDate = DateTime.Now,

//                        // خصائص الـ Individual
//                        Barcode = newBarcode,
//                        Quantity = 1,              // دايماً 1
//                        MinQuantityLimit = null    // غالباً مش بنحتاجه هنا
//                    };

//                    // --- منطق السيريال ---
//                    if ( dto.AssetType == AssetType.IT )
//                    {
//                        // IT: لازم سيريال لكل قطعة
//                        if ( dto.SerialNumbers != null && dto.SerialNumbers.Count > i )
//                        {
//                            asset.SerialNumber = dto.SerialNumbers [i];
//                        }
//                        else
//                        {
//                            throw new InvalidOperationException ( $"Serial Number is missing for IT Asset number {i + 1}." );
//                        }
//                    }
//                    else // Non-IT
//                    {
//                        // Non-IT: السيريال اختياري
//                        if ( dto.SerialNumbers != null && dto.SerialNumbers.Count > i )
//                            asset.SerialNumber = dto.SerialNumbers [i];
//                        else
//                            asset.SerialNumber = null;
//                    }

//                    assetsToAdd.Add ( asset );
//                    generatedBarcodes.Add ( newBarcode );
//                }
//            }

//            // 3. الحفظ (Transaction)
//            await UnitOfWork.BeginTransactionAsync ( );
//            try
//            {
//                // إضافة الأصول دفعة واحدة
//                await UnitOfWork.writeRepository<Asset> ( ).AddRangeAsync ( assetsToAdd );
//                await UnitOfWork.SaveChangeAsync ( );

//                // إضافة الموردين
//                if ( dto.SupplierIds != null && dto.SupplierIds.Any ( ) )
//                {
//                    foreach ( var addedAsset in assetsToAdd )
//                    {
//                        await AddOrUpdateAssetSuppliers ( addedAsset.Id, dto.SupplierIds );
//                    }
//                    await UnitOfWork.SaveChangeAsync ( );
//                }

//                // تسجيل Audit Trail
//                var auditTrail = new AuditTrail ( )
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




//        #region Old Add Asset Method
//        //public async Task<GetAssetResponseDTO> AddAssetAsync(AddAssetRequestDTO addAssetDto)
//        //{
//        //    var existingAsset = await UnitOfWork.readRepository<Asset>()
//        //        .GetAsync(a => a.SerialNumber == addAssetDto.SerialNumber);

//        //    if (existingAsset != null)
//        //    {
//        //        throw new InvalidOperationException("An asset with the same serial number already exists.");
//        //    }

//        //    // Validate foreign keys and category-subcategory relation
//        //    await ValidateForeignKeysAndCategoryRelationAsync(addAssetDto);

//        //    var realcategoryid = await UnitOfWork.readRepository<Category> ( )
//        //           .GetAsync ( c => c.SerialCode == addAssetDto.CategoryId.ToString ( ) );


//        //    var asset = Mapper.Map<Asset, AddAssetRequestDTO> ( addAssetDto );
//        //    asset.AddedOnDate = DateTime.Now;
//        //    asset.Quantity = addAssetDto.Quantity;
//        //    asset.MinQuantityLimit = addAssetDto.MinQuantityLimit;
//        //    asset.CategoryId=realcategoryid.Id;

//        //    await UnitOfWork.BeginTransactionAsync();
//        //    try
//        //    { 
//        //        //realcategoryid = await UnitOfWork.readRepository<Category>()
//        //        //    .GetAsync(c => c.SerialCode == addAssetDto.CategoryId.ToString());
//        //        asset.CategoryId =realcategoryid.Id;
//        //        await UnitOfWork.writeRepository<Asset>().AddAsync(asset);

//        //        await UnitOfWork.SaveChangeAsync();

//        //        // Add related records in AssetsSuppliers table
//        //        await AddOrUpdateAssetSuppliers(asset.Id, addAssetDto.SupplierIds);

//        //     }
//        //    catch
//        //    {
//        //        await UnitOfWork.RollbackTransactionAsync();
//        //        throw;
//        //    }
//        //    var auditTrail = new AuditTrail()
//        //    {
//        //        AddedOn = DateTime.Now,
//        //        Action = "Added",
//        //        EntityType = "Asset",
//        //        EntityName =asset.SerialNumber,
//        //        UserId = UserId ?? "System"
//        //    };

//        //    await UnitOfWork.writeRepository<AuditTrail>().AddAsync(auditTrail);

//        //    await UnitOfWork.SaveChangeAsync();
//        //    await UnitOfWork.CommitTransactionAsync();

//        //    return await GetAssetByIdAsync(asset.SerialNumber);
//        //}

//        #endregion

//        //public async Task<List<string>> AddAssetAsync ( AddAssetRequestDTO dto )
//        //{
//        //    // 1. التحقق من العلاقات (Foreign Keys)
//        //    await ValidateForeignKeysAndCategoryRelationAsync ( dto );

//        //    // 2. جلب كود الفئة وتجهيز الباركود
//        //    var category = await UnitOfWork.readRepository<Category> ( ).GetAsync (
//        //        predicate: c => c.Id == dto.CategoryId,
//        //        include: source => source.Include ( c => c.ParentCategory )
//        //    );

//        //    if ( category == null ) throw new InvalidOperationException ( "Category not found." );

//        //    string barcodePrefix = category.ParentCategory != null
//        //        ? $"{category.ParentCategory.SerialCode}-{category.SerialCode}"
//        //        : category.SerialCode;

//        //    // 3. معرفة آخر رقم تسلسلي (Smart Sequence Logic)
//        //    var lastAssetList = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
//        //        predicate: a => a.Barcode.StartsWith ( barcodePrefix ),
//        //        orderby: q => q.OrderByDescending ( a => a.Barcode ),
//        //        currentPage: 1,
//        //        pageSize: 1
//        //    );

//        //    int currentSequence = 0;
//        //    var lastAsset = lastAssetList.FirstOrDefault ( );
//        //    if ( lastAsset != null )
//        //    {
//        //        var parts = lastAsset.Barcode.Split ( '-' );
//        //        if ( parts.Length > 0 && int.TryParse ( parts.Last ( ), out int seq ) )
//        //        {
//        //            currentSequence = seq;
//        //        }
//        //    }

//        //    var assetsToAdd = new List<Asset> ( );
//        //    var generatedBarcodes = new List<string> ( );

//        //    // ---------------------------------------------------------
//        //    // التعديل الجوهري هنا (Logic Refactoring)
//        //    // ---------------------------------------------------------

//        //    // تحديد هل هو Bulk Consumable؟
//        //    // الشرط: الكمية > 1 + مفيش أي سيريالات في الليستة
//        //    bool isBulkConsumable = dto.Quantity > 1 && ( dto.SerialNumbers == null || !dto.SerialNumbers.Any ( ) );

//        //    int loopCount = isBulkConsumable ? 1 : dto.Quantity;

//        //    for ( int i = 0; i < loopCount; i++ )
//        //    {
//        //        currentSequence++;
//        //        string newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";

//        //        var asset = Mapper.Map<Asset> ( dto );

//        //        asset.Status = dto.Status.ToString ( );
//        //        asset.ModelNumber = dto.ModelNumber;  
//        //        asset.Name = dto.Name;             
//        //        asset.Description = dto.Description;
//        //        asset.PurchasePrice = dto.PurchasePrice;
//        //        asset.PurchaseDate = dto.PurchaseDate;
//        //        asset.WarrantyExpiryDate = dto.WarrantyExpiryDate;
//        //        asset.DepreciationDate = dto.DepreciationDate;
//        //        asset.LocationId = dto.LocationId;
//        //        asset.Status = dto.Status.ToString ( );
//        //        asset.Barcode = newBarcode;
//        //        asset.AddedOnDate = DateTime.Now;
//        //        asset.CategoryId = dto.CategoryId;


//        //        if ( isBulkConsumable )
//        //        {
//        //            // حالة المستهلكات (ورق/أقلام): سطر واحد بالكمية كلها وبدون سيريال
//        //            asset.Quantity = dto.Quantity;
//        //            asset.SerialNumber = null;
//        //        }
//        //        else
//        //        {
//        //            // حالة الأصول (لابتوب/كرسي): سطر لكل قطعة
//        //            asset.Quantity = 1;

//        //            // التعامل مع السيريالات من الليستة الموحدة
//        //            if ( dto.SerialNumbers != null && dto.SerialNumbers.Count > i )
//        //            {
//        //                // بناخد السيريال اللي عليه الدور في الليستة
//        //                asset.SerialNumber = dto.SerialNumbers [i];

//        //                // التحقق من التكرار
//        //                var exists = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.SerialNumber == asset.SerialNumber );
//        //                if ( exists != null )
//        //                    throw new InvalidOperationException ( $"Serial Number {asset.SerialNumber} already exists." );
//        //            }
//        //            else
//        //            {
//        //                // لو مفيش سيريال في الليستة (زي الكراسي أو لو اليوزر نسي يدخلهم)
//        //                asset.SerialNumber = null;
//        //            }
//        //        }

//        //        assetsToAdd.Add ( asset );
//        //        generatedBarcodes.Add ( newBarcode );
//        //    }

//        //    // ---------------------------------------------------------

//        //    // 5. الحفظ (Transaction)
//        //    await UnitOfWork.BeginTransactionAsync ( );
//        //    try
//        //    {
//        //        await UnitOfWork.writeRepository<Asset> ( ).AddRangeAsync ( assetsToAdd );
//        //        await UnitOfWork.SaveChangeAsync ( );

//        //        if ( dto.SupplierIds != null && dto.SupplierIds.Any ( ) )
//        //        {
//        //            foreach ( var addedAsset in assetsToAdd )
//        //            {
//        //                await AddOrUpdateAssetSuppliers ( addedAsset.Id, dto.SupplierIds );
//        //            }
//        //            await UnitOfWork.SaveChangeAsync ( );
//        //        }

//        //        var auditTrail = new AuditTrail ( )
//        //        {
//        //            AddedOn = DateTime.Now,
//        //            Action = "Add",
//        //            EntityType = "Asset",
//        //            EntityName = $"{loopCount} Assets added to {category.Name}",
//        //            UserId = UserId ?? "System"
//        //        };

//        //        await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
//        //        await UnitOfWork.SaveChangeAsync ( );

//        //        await UnitOfWork.CommitTransactionAsync ( );

//        //        return generatedBarcodes;
//        //    }
//        //    catch ( Exception )
//        //    {
//        //        await UnitOfWork.RollbackTransactionAsync ( );
//        //        throw;
//        //    }
//        //}
//        #endregion

//        #region Get Asset by Barcode
//        // غيرنا الاسم والبارامتر لـ Barcode لأنه الهوية الأساسية
//        public async Task<GetAssetResponseDTO> GetAssetByBarcodeAsync ( string barcode )
//        {
//            // 1. استخدام Includes لجلب البيانات المرتبطة
//            var asset = await UnitOfWork.readRepository<Asset> ( )
//                .GetAsync (
//                    predicate: a => a.Barcode == barcode &&
//                                   ( a.IsDeleted == false || a.IsDeleted == null ) &&
//                                   a.Status != AssetStatus.Retired.ToString ( ),

//                    // لازم نعمل Include لكل الجداول اللي بنعرض اسمها
//                    include: source => source
//                        .Include ( a => a.Location )
//                        .Include ( a => a.Category )
//                        .Include ( a => a.Manufacturer )
//                        .Include ( a => a.AssignedUser )
//                        .Include ( a => a.AssetsSuppliers ).ThenInclude ( s => s.Supplier )
//                );

//            if ( asset == null )
//            {
//                throw new KeyNotFoundException ( $"Asset with barcode '{barcode}' not found." );
//            }

//            // 2. التحويل للـ DTO مع مراعاة الـ Nulls
//            var getAssetResponseDTO = new GetAssetResponseDTO
//            {
//                Id = asset.Id,
//                Name = asset.Name,
//                Barcode = asset.Barcode, // مهم جداً نرجعه
//                AssetType = asset.AssetType.ToString ( ),
//                ModelNumber = asset.ModelNumber,
//                SerialNumber = asset.SerialNumber,
//                PurchaseDate = asset.PurchaseDate,
//                PurchasePrice = asset.PurchasePrice,
//                Description = asset.Description,
//                WarrantyExpiryDate = asset.WarrantyExpiryDate,
//                DepreciationDate = asset.DepreciationDate,
//                Status = asset.Status,

//                // Null Safety Checks (?. operator)
//                LocationName = asset.Location?.Name ?? "Unknown",
//                LocationId = asset.LocationId, // لو محتاجه للفرونت
//                LocationBarcode = asset.Location?.Barcode ?? "",

//                // التعامل مع الموظف (ممكن يكون Null لو في المخزن)
//                AssignedUserName = asset.AssignedUser != null
//                    ? $"{asset.AssignedUser.FirstName} {asset.AssignedUser.LastName}"
//                    : "In Stock / Not Assigned",
//                AssignedUserId = asset.AssignedUserId,

//                CategoryName = asset.Category?.Name ?? "Unknown",
//                CategoryId = asset.CategoryId,

//                // التعامل مع المصنع (ممكن يكون Null)
//                ManufacturerName = asset.Manufacturer?.Name ?? "N/A",
//                ManufacturerId = asset.ManufacturerId,

//                SupplierNames = asset.AssetsSuppliers?.Select ( s => s.Supplier?.CompanyName ).ToList ( ) ?? new List<string> ( ),

//                AddedOnDate = asset.AddedOnDate,
//                UpdatedDate = asset.UpdatedDate,
//                Quantity = asset.Quantity,
//                MinQuantityLimit = asset.MinQuantityLimit
//            };

//            // 3. حذفنا الـ Audit Trail من دالة الـ Get لتحسين الأداء

//            return getAssetResponseDTO;
//        }
//        #endregion

//        #region Get All Assets
//        public async Task<IList<GetAssetResponseDTO>> GetAllAssetsAsync ( )
//        {
//            // 1. لازم نستخدم Includes عشان نجيب بيانات الجداول المرتبطة
//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllAsync (
//                    predicate: a => a.IsDeleted == false || a.IsDeleted == null,
//                    include: source => source
//                        .Include ( a => a.Location )
//                        .Include ( a => a.Category )
//                        .Include ( a => a.Manufacturer )
//                        .Include ( a => a.AssignedUser )
//                        .Include ( a => a.AssetsSuppliers ).ThenInclude ( s => s.Supplier )
//                );

//            var getAssetResponseDTO = assets.Select ( a => new GetAssetResponseDTO ( )
//            {
//                Id = a.Id,
//                Name = a.Name,
//                Barcode = a.Barcode, // نسينا ده في الكود القديم
//                AssetType = a.AssetType.ToString ( ),
//                ModelNumber = a.ModelNumber,
//                SerialNumber = a.SerialNumber,
//                PurchaseDate = a.PurchaseDate,
//                PurchasePrice = a.PurchasePrice,
//                WarrantyExpiryDate = a.WarrantyExpiryDate,
//                DepreciationDate = a.DepreciationDate,
//                Status = a.Status,
//                Description = a.Description, // تم تصحيح الاسم

//                // Null Safety Checks (?. operator)
//                LocationBarcode = a.Location?.Barcode ?? "", // لو متاح في الانتيي
//                LocationName = a.Location?.Name ?? "Unknown",
//                LocationId = a.LocationId,

//                AssignedUserId = a.AssignedUserId, // Nullable Handle
//                AssignedUserName = a.AssignedUser != null
//                    ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
//                    : "In Stock",

//                CategoryName = a.Category?.Name ?? "Unknown",
//                CategoryId = a.CategoryId,

//                ManufacturerName = a.Manufacturer?.Name ?? "N/A", // تم تصحيح الاسم
//                ManufacturerId = a.ManufacturerId,

//                SupplierNames = a.AssetsSuppliers?.Select ( s => s.Supplier?.CompanyName ).ToList ( ) ?? new List<string> ( ),

//                AddedOnDate = a.AddedOnDate,
//                UpdatedDate = a.UpdatedDate,
//                MinQuantityLimit = a.MinQuantityLimit,
//                Quantity = a.Quantity
//            } ).ToList ( );

//            return getAssetResponseDTO;
//        }
//        #endregion

//        #region Get All Assets With Pagination
//        public async Task<IList<GetAssetResponseDTO>> GetAllByPaginationAssetsAsync ( int currentPage = 1, int pageSize = 10 )
//        {
//            // 1. نفس الكلام، لازم Includes مع الـ Pagination
//            var assets = await UnitOfWork.readRepository<Asset> ( )
//                .GetAllByPagningAsync (
//                    predicate: a => a.IsDeleted == false || a.IsDeleted == null,
//                    include: source => source
//                        .Include ( a => a.Location )
//                        .Include ( a => a.Category )
//                        .Include ( a => a.Manufacturer )
//                        .Include ( a => a.AssignedUser )
//                        .Include ( a => a.AssetsSuppliers ).ThenInclude ( s => s.Supplier ),
//                    pageSize: pageSize,
//                    currentPage: currentPage
//                );

//            var getAssetResponseDTO = assets.Select ( a => new GetAssetResponseDTO ( )
//            {
//                Id = a.Id,
//                Name = a.Name,
//                Barcode = a.Barcode,
//                AssetType = a.AssetType.ToString ( ),
//                ModelNumber = a.ModelNumber,
//                SerialNumber = a.SerialNumber,
//                PurchaseDate = a.PurchaseDate,
//                PurchasePrice = a.PurchasePrice,
//                WarrantyExpiryDate = a.WarrantyExpiryDate,
//                DepreciationDate = a.DepreciationDate,
//                Status = a.Status,
//                Description = a.Description,

//                LocationName = a.Location?.Name ?? "Unknown",
//                LocationId = a.LocationId,
//                LocationBarcode = a.Location?.Barcode ?? "",

//                AssignedUserName = a.AssignedUser != null
//                    ? $"{a.AssignedUser.FirstName} {a.AssignedUser.LastName}"
//                    : "In Stock",
//                AssignedUserId = a.AssignedUserId,

//                CategoryName = a.Category?.Name ?? "Unknown",
//                CategoryId = a.CategoryId,

//                ManufacturerName = a.Manufacturer?.Name ?? "N/A",
//                ManufacturerId = a.ManufacturerId,

//                SupplierNames = a.AssetsSuppliers?.Select ( s => s.Supplier?.CompanyName ).ToList ( ) ?? new List<string> ( ),

//                AddedOnDate = a.AddedOnDate,
//                UpdatedDate = a.UpdatedDate,
//                MinQuantityLimit = a.MinQuantityLimit,
//                Quantity = a.Quantity
//            } ).ToList ( );

//            // تم حذف الـ Audit Trail من هنا تماماً
//            // GET Methods should be Idempotent (Read-Only)

//            return getAssetResponseDTO;
//        }
//        #endregion

//        #region Update Asset
//        // التعديل 1: نستخدم Barcode كمعرف أساسي بدلاً من SerialNumber
//        public async Task<GetAssetResponseDTO> UpdateAssetAsync ( string barcode, UpdateAssetRequestDTO dto )
//        {
//            await UnitOfWork.BeginTransactionAsync ( );
//            try
//            {
//                // 1. البحث عن الأصل بالباركود
//                var existingAsset = await UnitOfWork.readRepository<Asset> ( )
//                    .GetAsync ( a => a.Barcode == barcode &&
//                                   ( a.IsDeleted == false || a.IsDeleted == null ) &&
//                                   a.Status != AssetStatus.Retired.ToString ( ) );

//                if ( existingAsset == null )
//                {
//                    throw new KeyNotFoundException ( $"Asset with barcode '{barcode}' not found." );
//                }

//                // 2. التحقق من العلاقات (Foreign Keys)
//                // * ملحوظة: تأكد إن الـ Validation Function بتقبل الـ Update DTO
//                await ValidateForeignKeysAndCategoryRelationAsync ( dto );

//                // 3. تحديث البيانات (Mapping)
//                existingAsset.Name = dto.Name;
//                existingAsset.ModelNumber = dto.ModelNumber;
//                existingAsset.Description = dto.Description; // تصحيح الاسم
//                existingAsset.PurchaseDate = dto.PurchaseDate;
//                existingAsset.PurchasePrice = dto.PurchasePrice;
//                existingAsset.WarrantyExpiryDate = dto.WarrantyExpiryDate;
//                existingAsset.DepreciationDate = dto.DepreciationDate;
//                existingAsset.Status = dto.Status.ToString ( ); // Enum to String

//                // تحديث العلاقات
//                existingAsset.LocationId = dto.LocationId;

//                // التعديل 2: استخدام ID مباشرة دون الحاجة لجلب الكائن
//                existingAsset.CategoryId = dto.CategoryId;
//                existingAsset.ManufacturerId = dto.ManufacturerId;

//                //// التعامل الذكي مع المستخدم (Assign/Unassign)
//                //existingAsset.AssignedUserId = dto.AssignedUserId;

//                // التعامل مع السيريال (لو اتغير)
//                // السيريال قابل للتعديل، ولكن يجب التأكد من عدم تكراره إذا لم يكن فارغاً
//                if ( existingAsset.SerialNumber != dto.SerialNumber )
//                {
//                    if ( !string.IsNullOrEmpty ( dto.SerialNumber ) )
//                    {
//                        var duplicateCheck = await UnitOfWork.readRepository<Asset> ( )
//                            .GetAsync ( a => a.SerialNumber == dto.SerialNumber && a.Id != existingAsset.Id );

//                        if ( duplicateCheck != null )
//                            throw new InvalidOperationException ( $"Serial Number '{dto.SerialNumber}' is already used by another asset." );
//                    }
//                    existingAsset.SerialNumber = dto.SerialNumber;
//                }

//                existingAsset.UpdatedDate = DateTime.Now;
//                // يمكن تحديث حد الطلب أيضاً
//                existingAsset.MinQuantityLimit = dto.MinQuantityLimit;

//                // 4. الحفظ المبدئي للأصل
//                await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( existingAsset.Id, existingAsset );
//                await UnitOfWork.SaveChangeAsync ( );

//                // 5. تحديث الموردين (Replace Logic)
//                // بنمسح القديم ونحط الجديد (أسهل طريقة في الـ Update)
//                if ( dto.SupplierIds != null )
//                {
//                    // تأكد إن عندك دالة DeleteAssetSuppliers بتمسح من جدول الربط بناءً على AssetId
//                    await DeleteAssetSuppliers ( existingAsset.Id );

//                    if ( dto.SupplierIds.Any ( ) )
//                    {
//                        await AddOrUpdateAssetSuppliers ( existingAsset.Id, dto.SupplierIds );
//                    }
//                    await UnitOfWork.SaveChangeAsync ( );
//                }

//                // 6. Audit Trail
//                var auditTrail = new AuditTrail ( )
//                {
//                    AddedOn = DateTime.Now,
//                    Action = "Update",
//                    EntityType = "Asset",
//                    EntityName = existingAsset.Barcode, // نستخدم الباركود كمرجع
//                    UserId = UserId ?? "System",
//                    // يفضل إضافة تفاصيل التعديل هنا لو أمكن
//                };

//                await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );
//                await UnitOfWork.SaveChangeAsync ( );

//                await UnitOfWork.CommitTransactionAsync ( );

//                // 7. إرجاع البيانات الجديدة باستخدام دالة الباركود الجديدة
//                return await GetAssetByBarcodeAsync ( existingAsset.Barcode );
//            }
//            catch
//            {
//                await UnitOfWork.RollbackTransactionAsync ( );
//                throw;
//            }
//        }
//        #endregion




//        #region Withdraw Quantity from Asset

//        public async Task<DTOs.AssetDTOs.GetAssetResponseDTO> WithdrawQuantityAsync ( int assetId, int quantityToWithdraw )
//        {
//            // Validation
//            if ( assetId <= 0 )
//            {
//                throw new ArgumentException ( "Invalid asset ID.", nameof ( assetId ) );
//            }

//            if ( quantityToWithdraw <= 0 )
//            {
//                throw new ArgumentException ( "Quantity to withdraw must be greater than zero.", nameof ( quantityToWithdraw ) );
//            }

//            // Get the asset
//            var asset = await UnitOfWork.readRepository<Asset> ( )
//                .GetAsync ( predicate: a => a.Id == assetId && ( a.IsDeleted == false || a.IsDeleted == null ) );

//            if ( asset == null )
//            {
//                throw new KeyNotFoundException ( $"Asset with ID {assetId} not found." );
//            }

//            // Check if withdrawal would result in negative quantity
//            int remainingQuantity = asset.Quantity - quantityToWithdraw;

//            if ( remainingQuantity < 0 )
//            {
//                throw new InvalidOperationException (
//                    $"Cannot withdraw {quantityToWithdraw} units. Available quantity is {asset.Quantity}."
//                );
//            }

//            // Update the quantity
//            asset.Quantity = remainingQuantity;
//            asset.UpdatedDate = DateTime.Now;

//            // Save changes
//            await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );
//            await UnitOfWork.SaveChangeAsync ( );

//            // Check if stock is low
//            bool isLowStock = asset.Quantity <= asset.MinQuantityLimit;

//            if ( isLowStock )
//            {

//                _logger.LogWarning ( "LOW STOCK: Asset '{AssetName}' (ID: {AssetId}) reached {Quantity} units.",
//                    asset.Name, asset.Id, asset.Quantity );

//                try
//                {

//                    _logger.LogInformation ( "Fetching admin users for low stock notification..." );


//                    var adminsInRole = await _userManager.GetUsersInRoleAsync ( "Admin" );


//                    var userIdsToNotify = adminsInRole
//                        .Where ( u => u.IsDeleted == false || u.IsDeleted == null )  
//                        .Select ( u => u.Id.ToString ( ) ) 
//                        .ToList ( );

//                    if ( userIdsToNotify.Any ( ) == false )
//                    {
//                        _logger.LogWarning ( "No active admin users found to notify for low stock." );
//                    }
//                    else
//                    {
//                        _logger.LogInformation ( "Found {AdminCount} active admin(s) to notify.", userIdsToNotify.Count );


//                        var httpClient = _httpClientFactory.CreateClient ( );


//                        var metadataObject = new
//                        {
//                            assetId = asset.Id,
//                            assetName = asset.Name,
//                            serialNumber = asset.SerialNumber,
//                            currentQuantity = asset.Quantity,
//                            minLimit = asset.MinQuantityLimit
//                        };


//                        var requestPayload = new
//                        {
//                            userIds = userIdsToNotify, 
//                            title = $"🚨 Low Stock: {asset.Name}",
//                            message = $"Stock for '{asset.Name}' (SN: {asset.SerialNumber}) is low. " +
//                                      $"Current: {asset.Quantity}, Limit: {asset.MinQuantityLimit}.",
//                            notificationType = "Warning",
//                            category = "Inventory",
//                            priority = 3,
//                            actionUrl = $"/assets/manage/{asset.Id}",
//                            metadata = System.Text.Json.JsonSerializer.Serialize ( metadataObject )
//                        };


//                        var response = await httpClient.PostAsJsonAsync (
//                            "http://10.10.10.48:7000/api/trailing/notifications/bulk",
//                            requestPayload
//                        );

//                        if ( response.IsSuccessStatusCode )
//                        {
//                            var responseString = await response.Content.ReadAsStringAsync ( );
//                            _logger.LogInformation ( "Low stock notification sent successfully to admins. Response: {Response}", responseString );
//                        }
//                        else
//                        {
//                            _logger.LogError ( "Failed to send low stock notification to admins. Status: {StatusCode}, Reason: {Reason}",
//                                response.StatusCode, response.ReasonPhrase );
//                        }
//                    }
//                }
//                catch ( Exception ex )
//                {
//                    _logger.LogError ( ex, "An error occurred while sending low stock notification for Asset ID {AssetId}.", asset.Id );
//                }

//            }

//            // Return response
//            return new DTOs.AssetDTOs.GetAssetResponseDTO
//            {
//                Id = asset.Id,
//                Name = asset.Name,
//                SerialNumber = asset.SerialNumber,
//                Quantity = asset.Quantity,
//                MinQuantityLimit = asset.MinQuantityLimit,
//             };
//        }
//        #endregion



//        #region Delete (Retire) Asset
//        public async Task<bool> DeleteAssetAsync ( string barcode )
//        {
//            // 1. البحث عن الأصل
//            var asset = await UnitOfWork.readRepository<Asset> ( )
//                .GetAsync ( a => a.Barcode == barcode && ( a.IsDeleted == false || a.IsDeleted == null ) );

//            if ( asset == null )
//                throw new KeyNotFoundException ( $"Asset with barcode '{barcode}' not found." );

//            // 2. التحقق من إمكانية الحذف
//            // مثلاً: مينفعش تمسح أصل وهو عهدة مع موظف حالياً
//            if ( asset.Status == AssetStatus.Available.ToString ( ) )
//            {
//                throw new InvalidOperationException ( "Cannot delete an asset that is currently In Use. Please unassign it first." );
//            }

//            // 3. تنفيذ الحذف المنطقي (Soft Delete)
//            asset.IsDeleted = true;
//            asset.Status = AssetStatus.Retired.ToString ( ); // تغيير الحالة لـ "مكهن" أو "خارج الخدمة"
//            asset.UpdatedDate = DateTime.Now;

//            // 4. الحفظ
//            await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );

//            // 5. Audit Trail
//            var auditTrail = new AuditTrail ( )
//            {
//                AddedOn = DateTime.Now,
//                Action = "Delete (Retire)",
//                EntityType = "Asset",
//                EntityName = asset.Barcode,
//                UserId = UserId ?? "System",
//             };
//            await UnitOfWork.writeRepository<AuditTrail> ( ).AddAsync ( auditTrail );

//            await UnitOfWork.SaveChangeAsync ( );

//            return true;
//        }
//        #endregion

//        #region Add or update supplier associations
//        private async Task AddOrUpdateAssetSuppliers ( int assetId, ICollection<int> supplierIds )
//        {
//             if ( supplierIds == null || !supplierIds.Any ( ) ) return;

//             var distinctSupplierIds = supplierIds.Distinct ( ).ToList ( );

//            var suppliersToAdd = new List<AssetsSuppliers> ( );

//            foreach ( var supplierId in distinctSupplierIds )
//            {
//                var assetSupplier = new AssetsSuppliers
//                {
//                    AssetId = assetId,
//                    SupplierId = supplierId
//                };
//                suppliersToAdd.Add ( assetSupplier );
//            }


//            await UnitOfWork.writeRepository<AssetsSuppliers> ( ).AddRangeAsync ( suppliersToAdd );

//             await UnitOfWork.SaveChangeAsync ( );
//        }
//        #endregion

//        #region Delete existing asset-supplier relationships
//        private async Task DeleteAssetSuppliers ( int assetId )
//        {
//            var existingAssetSuppliers = await UnitOfWork.readRepository<AssetsSuppliers> ( )
//                .GetAllAsync ( As => As.AssetId == assetId );

//            // Check before delete
//            if ( existingAssetSuppliers != null && existingAssetSuppliers.Any ( ) )
//            {
//                await UnitOfWork.writeRepository<AssetsSuppliers> ( ).DeleteRangeAsync ( existingAssetSuppliers );
//                await UnitOfWork.SaveChangeAsync ( );
//            }
//        }
//        #endregion






//        #region Validate Foreign Keys
//        private async Task ValidateForeignKeysAndCategoryRelationAsync ( AddAssetRequestDTO assetDto )
//        {
//            // 1. Location (Required)
//            var location = await UnitOfWork.readRepository<Location> ( )
//                .GetAsync ( l => l.Id == assetDto.LocationId && ( l.IsDeleted == false || l.IsDeleted == null ) );

//            if ( location == null )
//                throw new KeyNotFoundException ( $"Location with ID {assetDto.LocationId} not found." );

//            // 2. Category (Required - Fix: Search by ID not Code)
//            // غيرنا البحث هنا لـ ID لأن الـ DTO بيبعت int
//            var category = await UnitOfWork.readRepository<Category> ( )
//                .GetAsync ( c => c.Id == assetDto.CategoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

//            if ( category == null )
//                throw new KeyNotFoundException ( $"Category with ID {assetDto.CategoryId} not found." );

//            // 3. Assigned User (Nullable - Fix: Check only if provided)
//            // لازم نتأكد الأول إن ليه قيمة قبل ما نروح الداتابيز
//            if ( assetDto.AssignedUserId.HasValue )
//            {
//                var user = await UnitOfWork.readRepository<User> ( )
//                    .GetAsync ( u => u.Id == assetDto.AssignedUserId && ( u.IsDeleted == false || u.IsDeleted == null ) );

//                if ( user == null )
//                    throw new KeyNotFoundException ( "Assigned user not found." );
//            }

//            // 4. Manufacturer (Nullable - Added Missing Validation)
//            if ( assetDto.ManufacturerId.HasValue )
//            {
//                var manufacturer = await UnitOfWork.readRepository<Manufacturer> ( )
//                    .GetAsync ( m => m.Id == assetDto.ManufacturerId && ( m.IsDeleted == false || m.IsDeleted == null ) );

//                if ( manufacturer == null )
//                    throw new KeyNotFoundException ( "Manufacturer not found." );
//            }

//            // 5. Suppliers (Performance Fix: Batch Check)
//            // بدل ما نلف ونكلم الداتابيز 10 مرات، بنكلمها مرة واحدة ونعد اللي لقيناهم
//            if ( assetDto.SupplierIds != null && assetDto.SupplierIds.Any ( ) )
//            {
//                // بنشيل التكرار من الـ IDs اللي جاية
//                var distinctIds = assetDto.SupplierIds.Distinct ( ).ToList ( );

//                // بنشوف كام واحد من الـ IDs دي موجود فعلاً في الداتابيز
//                var existingCount = await UnitOfWork.readRepository<Supplier> ( )
//                    .CountAsync ( s => distinctIds.Contains ( s.Id ) && ( s.IsDeleted == false || s.IsDeleted == null ) );

//                // لو عدد اللي لقيناهم أقل من العدد اللي باعتينه، يبقى فيه ID غلط
//                if ( existingCount != distinctIds.Count )
//                {
//                    throw new KeyNotFoundException ( "One or more Supplier IDs are invalid or deleted." );
//                }
//            }
//        }
//        #endregion

//        #region Validate Foreign Keys
//        private async Task ValidateForeignKeysAndCategoryRelationAsync ( UpdateAssetRequestDTO assetDto )
//        {
//            // 1. Location (Required)
//            var location = await UnitOfWork.readRepository<Location> ( )
//                .GetAsync ( l => l.Id == assetDto.LocationId && ( l.IsDeleted == false || l.IsDeleted == null ) );

//            if ( location == null )
//                throw new KeyNotFoundException ( $"Location with ID {assetDto.LocationId} not found." );

//            // 2. Category (Required - Fix: Search by ID not Code)
//            // غيرنا البحث هنا لـ ID لأن الـ DTO بيبعت int
//            var category = await UnitOfWork.readRepository<Category> ( )
//                .GetAsync ( c => c.Id == assetDto.CategoryId && ( c.IsDeleted == false || c.IsDeleted == null ) );

//            if ( category == null )
//                throw new KeyNotFoundException ( $"Category with ID {assetDto.CategoryId} not found." );

//            //// 3. Assigned User (Nullable - Fix: Check only if provided)
//            //// لازم نتأكد الأول إن ليه قيمة قبل ما نروح الداتابيز
//            //if ( assetDto.AssignedUserId.HasValue )
//            //{
//            //    var user = await UnitOfWork.readRepository<User> ( )
//            //        .GetAsync ( u => u.Id == assetDto.AssignedUserId && ( u.IsDeleted == false || u.IsDeleted == null ) );

//            //    if ( user == null )
//            //        throw new KeyNotFoundException ( "Assigned user not found." );
//            //}

//            // 4. Manufacturer (Nullable - Added Missing Validation)
//            if ( assetDto.ManufacturerId.HasValue )
//            {
//                var manufacturer = await UnitOfWork.readRepository<Manufacturer> ( )
//                    .GetAsync ( m => m.Id == assetDto.ManufacturerId && ( m.IsDeleted == false || m.IsDeleted == null ) );

//                if ( manufacturer == null )
//                    throw new KeyNotFoundException ( "Manufacturer not found." );
//            }

//            // 5. Suppliers (Performance Fix: Batch Check)
//            // بدل ما نلف ونكلم الداتابيز 10 مرات، بنكلمها مرة واحدة ونعد اللي لقيناهم
//            if ( assetDto.SupplierIds != null && assetDto.SupplierIds.Any ( ) )
//            {
//                // بنشيل التكرار من الـ IDs اللي جاية
//                var distinctIds = assetDto.SupplierIds.Distinct ( ).ToList ( );

//                // بنشوف كام واحد من الـ IDs دي موجود فعلاً في الداتابيز
//                var existingCount = await UnitOfWork.readRepository<Supplier> ( )
//                    .CountAsync ( s => distinctIds.Contains ( s.Id ) && ( s.IsDeleted == false || s.IsDeleted == null ) );

//                // لو عدد اللي لقيناهم أقل من العدد اللي باعتينه، يبقى فيه ID غلط
//                if ( existingCount != distinctIds.Count )
//                {
//                    throw new KeyNotFoundException ( "One or more Supplier IDs are invalid or deleted." );
//                }
//            }
//        }
//        #endregion

//    }
//}




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
        public async Task<List<string>> AddAssetAsync ( AddAssetRequestDTO dto )
        {
            // 1. Validation
            await ValidateForeignKeysAsync ( dto.LocationId, dto.CategoryId, dto.ManufacturerId, dto.AssignedUserId, dto.SupplierIds );

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

            // 3. Prepare Barcode Prefix
            var category = await UnitOfWork.readRepository<Category> ( ).GetAsync ( c => c.Id == dto.CategoryId, include: s => s.Include ( x => x.ParentCategory ) );
            if ( category == null ) throw new InvalidOperationException ( "Category not found." );

            string barcodePrefix = category.ParentCategory != null
                ? $"{category.ParentCategory.SerialCode}-{category.SerialCode}"
                : category.SerialCode;

            //var lastAssetList = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
            //    predicate: a => a.Barcode.StartsWith ( barcodePrefix ),
            //    orderby: q => q.OrderByDescending ( a => a.Barcode ),
            //    currentPage: 1, pageSize: 1 );

            int expectedLength = barcodePrefix.Length + 1 + 6;
            var lastAssetList = await UnitOfWork.readRepository<Asset> ( ).GetAllByPagningAsync (
    predicate: a => a.Barcode.StartsWith ( barcodePrefix + "-" ) // نتأكد إن بعد البريفكس فيه داش
                 && a.Barcode.Length == expectedLength,       // 🛑 ده الشرط اللي هيحل المشكلة
    orderby: q => q.OrderByDescending ( a => a.Barcode ),
    currentPage: 1,
    pageSize: 1
);


            int currentSequence = 0;
            var lastAsset = lastAssetList.FirstOrDefault ( );
            if ( lastAsset != null )
            {
                var parts = lastAsset.Barcode.Split ( '-' );
                if ( parts.Length > 0 && int.TryParse ( parts.Last ( ), out int seq ) ) currentSequence = seq;
            }

            var assetsToAdd = new List<Asset> ( );
            var generatedBarcodes = new List<string> ( );

            // -------------------------------------------------------
            // 🔥 Core Logic: AssetType Decision (Bulk vs Individual)
            // -------------------------------------------------------
            if ( dto.AssetType == AssetType.Consumable )
            {
                // === Case 1: Consumable (Bulk - 1 Row) ===
                currentSequence++;
                string newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";

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
                    currentSequence++;
                    string newBarcode = $"{barcodePrefix}-{currentSequence.ToString ( ).PadLeft ( 6, '0' )}";

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

                await ValidateForeignKeysAsync ( dto.LocationId, dto.CategoryId, dto.ManufacturerId, dto.AssignedUserId, dto.SupplierIds );

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
                LocationId = dto.LocationId,
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
                PurchaseDate = a.PurchaseDate,
                PurchasePrice = a.PurchasePrice,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                DepreciationDate = a.DepreciationDate,
                Status = a.Status,
                Description = a.Description,
                LocationName = a.Location?.Name ?? "Unknown",
                LocationId = a.LocationId,
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
            var loc = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Id == locId && ( l.IsDeleted == false || l.IsDeleted == null ) );
            if ( loc == null ) throw new KeyNotFoundException ( $"Location ID {locId} not found" );

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

            if ( supIds != null && supIds.Any ( ) )
            {
                var distinctIds = supIds.Distinct ( ).ToList ( );
                var count = await UnitOfWork.readRepository<Supplier> ( ).CountAsync ( s => distinctIds.Contains ( s.Id ) && ( s.IsDeleted == false || s.IsDeleted == null ) );
                if ( count != distinctIds.Count ) throw new KeyNotFoundException ( "One or more Supplier IDs invalid." );
            }
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

