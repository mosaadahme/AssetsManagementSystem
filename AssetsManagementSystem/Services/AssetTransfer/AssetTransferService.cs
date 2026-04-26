using AssetsManagementSystem.DTOs.AssetTransferDTOs;

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

        #region 1. Assign Asset (Stock -> User) [Check-Out]
        
        public async Task<GetAssetTransferRecordResponseDTO> AssignAssetToUserAsync ( string barcode, Guid toUserId )
        {
            // 1. التحقق من المستخدم
            var toUser = await _userManager.FindByIdAsync ( toUserId.ToString ( ) );
            if ( toUser == null ) throw new InvalidOperationException ( "Target user not found." );

            // 2. جلب الأصل والتأكد من حالته
            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync (
                a => a.Barcode == barcode && ( a.IsDeleted == false || a.IsDeleted == null ) &&
                     a.Status != AssetStatus.Retired.ToString ( ) );

            if ( asset == null ) throw new KeyNotFoundException ( $"Asset with barcode '{barcode}' not found." );

            // شرط: لازم يكون في المخزن (غير مربوط بموظف)
            if ( asset.AssignedUserId != null )
                throw new InvalidOperationException ( $"Asset is currently assigned to another user. Use 'Transfer' instead." );

            // 3. إنشاء السجل
            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromUserId = null, // جاي من المخزن
                ToUserId = toUserId,
                FromLocationId = asset.LocationId,
                ToLocationId = asset.LocationId??0, // المكان ثابت (أو ممكن يتغير لو الموظف في مكان تاني)
                Status = TransferStatus.Approved.ToString ( ), // موافقة فورية
                ApprovalDate = DateOnly.FromDateTime ( DateTime.Now ),
                AddedOnDate = DateTime.Now,
                IsUserTransfer = true
            };

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );

                // تحديث الأصل
                asset.AssignedUserId = toUserId;
                asset.Status = AssetStatus.InUse.ToString ( ); // أصبح مستخدم
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

        #region 2. Return Asset (User -> Stock) [Check-In]
        public async Task<GetAssetTransferRecordResponseDTO> ReturnAssetToStockAsync ( string barcode )
        {
            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync (
                a => a.Barcode == barcode && ( a.IsDeleted == false || a.IsDeleted == null ) );

            if ( asset == null ) throw new KeyNotFoundException ( $"Asset '{barcode}' not found." );

            if ( asset.AssignedUserId == null )
                throw new InvalidOperationException ( "Asset is already in stock (not assigned to anyone)." );

            Guid previousOwnerId = asset.AssignedUserId.Value;

            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromUserId = previousOwnerId,
                ToUserId = null, // راجع للمخزن
                FromLocationId = asset.LocationId,
                ToLocationId = asset.LocationId??0,
                Status = TransferStatus.Approved.ToString ( ),
                ApprovalDate = DateOnly.FromDateTime ( DateTime.Now ),
                AddedOnDate = DateTime.Now,
                IsUserTransfer = true
            };

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );

                // تحرير الأصل
                asset.AssignedUserId = null;
                asset.Status = AssetStatus.Available.ToString ( ); // رجع متاح
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

        #region 3. Transfer Asset (User -> User) [Handover]
        public async Task<GetAssetTransferRecordResponseDTO> TransferUserToUserAsync ( UserToUserTransferDTO dto )
        {
            if ( dto.FromUserId == dto.ToUserId )
                throw new InvalidOperationException ( "Source and destination users cannot be the same." );

            var toUser = await _userManager.FindByIdAsync ( dto.ToUserId.ToString ( ) );
            if ( toUser == null ) throw new InvalidOperationException ( "Destination user not found." );

            // البحث بالباركود
            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync (
                a => a.Barcode == dto.AssetBarcode &&
                     ( a.IsDeleted == false || a.IsDeleted == null ) &&
                     a.AssignedUserId == dto.FromUserId ); // التأكد من الملكية الحالية

            if ( asset == null ) throw new KeyNotFoundException ( "Asset not found or does not belong to the sender." );

            // التحقق من وجود طلب معلق
            var pendingTransfer = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAsync ( t => t.AssetId == asset.Id && t.Status == TransferStatus.Pending.ToString ( ) );

            if ( pendingTransfer != null ) throw new InvalidOperationException ( "This asset already has a pending transfer request." );

            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromUserId = dto.FromUserId,
                ToUserId = dto.ToUserId,
                FromLocationId = asset.LocationId,
                ToLocationId = asset.LocationId ?? 0,
                Status = TransferStatus.Pending.ToString ( ), 
                AddedOnDate = DateTime.Now,
                IsUserTransfer = true
            };

            await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );
            await UnitOfWork.SaveChangeAsync ( );

            return await GetAssetTransferByIdAsync ( transferRecord.Id );
        }
        #endregion

        #region 4. Relocate Asset (Location -> Location)
        public async Task<GetAssetTransferRecordResponseDTO> RelocateAssetAsync ( LocationToLocationTransferDTO dto )
        {
            var fromLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == dto.FromLocationBarcode );
            var toLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == dto.ToLocationBarcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( toLocation == null ) throw new InvalidOperationException ( "Destination location not found." );
            if ( fromLocation.Id == toLocation.Id ) throw new InvalidOperationException ( "Source and Destination are the same." );

            var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync (
                a => a.Barcode == dto.AssetBarcode &&
                     ( a.IsDeleted == false || a.IsDeleted == null ) &&
                     a.LocationId == fromLocation.Id );

            if ( asset == null ) throw new KeyNotFoundException ( "Asset not found in the source location." );

            var transferRecord = new AssetTransferRecords
            {
                AssetId = asset.Id,
                FromLocationId = fromLocation.Id,
                ToLocationId = toLocation.Id,
                FromUserId = asset.AssignedUserId, // يظل الموظف كما هو (إن وجد)
                ToUserId = asset.AssignedUserId  ,
                Status = TransferStatus.Approved.ToString ( ), // يتم فوراً
                AddedOnDate = DateTime.Now,
                ApprovalDate = DateOnly.FromDateTime ( DateTime.Now ),
                IsUserTransfer = false
            };

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );

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

        #region 5. Approve / Reject Transfer
        public async Task ApproveTransferAsync ( int transferId )
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAsync ( r => r.Id == transferId && ( r.IsDeleted == false || r.IsDeleted == null ) );

            if ( transferRecord == null ) throw new KeyNotFoundException ( "Transfer record not found." );

            // تحقق من أن المستخدم الحالي هو المستلم
            if ( transferRecord.ToUserId != null && !string.Equals ( UserId, transferRecord.ToUserId.ToString ( ), StringComparison.OrdinalIgnoreCase ) )
                throw new InvalidOperationException ( "You are not authorized to approve this transfer." );

            if ( transferRecord.Status != TransferStatus.Pending.ToString ( ) )
                throw new InvalidOperationException ( "Transfer is not Pending." );

            transferRecord.Status = TransferStatus.Approved.ToString ( );
            transferRecord.ApprovalDate = DateOnly.FromDateTime ( DateTime.Now );

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                var asset = await UnitOfWork.readRepository<Asset> ( ).GetAsync ( a => a.Id == transferRecord.AssetId );
                if ( asset == null ) throw new KeyNotFoundException ( "Asset not found." );

                // تنفيذ النقل
                if ( transferRecord.IsUserTransfer )
                {
                    asset.AssignedUserId = transferRecord.ToUserId;
                    // الحالة بتفضل InUse لأنها اتنقلت من موظف لموظف
                }

                asset.UpdatedDate = DateTime.Now;

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

        public async Task RejectTransferAsync ( int transferId, string reason )
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( )
                .GetAsync ( r => r.Id == transferId && ( r.IsDeleted == false || r.IsDeleted == null ) );

            if ( transferRecord == null ) throw new KeyNotFoundException ( "Transfer record not found." );
            if ( transferRecord.Status != TransferStatus.Pending.ToString ( ) ) throw new InvalidOperationException ( "Transfer is not Pending." );

            transferRecord.Status = TransferStatus.Rejected.ToString ( );
            transferRecord.RejectionReason = reason;
            transferRecord.ApprovalDate = null;

            await UnitOfWork.writeRepository<AssetTransferRecords> ( ).UpdateAsync ( transferRecord.Id, transferRecord );
            await UnitOfWork.SaveChangeAsync ( );
        }
        #endregion

        #region 6. Get Queries (Helper Methods)

        public async Task<GetAssetTransferRecordResponseDTO> GetAssetTransferByIdAsync ( int id )
        {
            var transferRecord = await UnitOfWork.readRepository<AssetTransferRecords> ( ).GetAsync (
                predicate: tr => tr.Id == id,
                include: source => source
                    .Include ( t => t.Asset )
                    .Include ( t => t.FromUser )
                    .Include ( t => t.ToUser )
                    .Include ( t => t.FromLocation )
                    .Include ( t => t.ToLocation )
            );

            if ( transferRecord == null ) throw new KeyNotFoundException ( "Record not found." );
            return MapToTransferDTO ( transferRecord );
        }

        public async Task<IList<GetAssetTransferRecordResponseDTO>> GetMyPendingTransfersAsync ( )
        {
            var currentUserId = Guid.Parse ( UserId );
            var records = await UnitOfWork.readRepository<AssetTransferRecords> ( ).GetAllAsync (
                predicate: tr => tr.ToUserId == currentUserId && tr.Status == TransferStatus.Pending.ToString ( ),
                include: source => source.Include ( t => t.Asset ).Include ( t => t.FromUser ).Include ( t => t.ToUser )
            );
            return records.Select ( MapToTransferDTO ).ToList ( );
        }

        public async Task<IList<GetAssetTransferRecordResponseDTO>> GetAllTransfersAsync ( )
        {
            var records = await UnitOfWork.readRepository<AssetTransferRecords> ( ).GetAllAsync (
                predicate: tr => tr.IsDeleted == false || tr.IsDeleted == null,
                include: source => source.Include ( t => t.Asset ).Include ( t => t.FromUser ).Include ( t => t.ToUser ).Include ( t => t.FromLocation ).Include ( t => t.ToLocation )
            );
            return records.Select ( MapToTransferDTO ).ToList ( );
        }

        private static GetAssetTransferRecordResponseDTO MapToTransferDTO ( AssetTransferRecords tr )
        {
            return new GetAssetTransferRecordResponseDTO
            {
                Id = tr.Id,
                AssetId = tr.AssetId,
                AssetName = tr.Asset?.Name ?? "Unknown",
                AssetBarcode = tr.Asset?.Barcode,

                FromUserId = tr.FromUserId?? Guid.Empty,
                FromUserName = tr.FromUser != null ? $"{tr.FromUser.FirstName} {tr.FromUser.LastName}" : "Main Stock",

                ToUserId = tr.ToUserId ?? Guid.Empty,
                ToUserName = tr.ToUser != null ? $"{tr.ToUser.FirstName} {tr.ToUser.LastName}" : "Main Stock",

                FromLocationName = tr.FromLocation?.Name,
                ToLocationName = tr.ToLocation?.Name,

                Status = tr.Status,
                AddedOnDate = tr.AddedOnDate,
                ApprovalDate = tr.ApprovalDate,
                RejectionReason = tr.RejectionReason
            };
        }
        #endregion

        #region 7. Relocation in Seperated Steps
        #region 5. Bulk Relocate Asset (One Source -> Multiple Destinations)
        public async Task<BulkRelocationResponseDTO> RelocateAssetsBulkAsync ( BulkRelocationRequestDTO dto )
        {
            // ============================================================
            // 1. Validation Logic (مرحلة التحقق الصارمة)
            // ============================================================

            // أ. التأكد من وجود بيانات
            if ( dto.PickedAssetBarcodes == null || !dto.PickedAssetBarcodes.Any ( ) )
                throw new InvalidOperationException ( "No assets scanned from source." );

            if ( dto.Distributions == null || !dto.Distributions.Any ( ) )
                throw new InvalidOperationException ( "No destination locations specified." );

            // ب. التأكد من تطابق العدد والهوية (اللي خرج = اللي اتوزع)
            // بنجمع كل الباركودات اللي رايحة للوجهات المختلفة في ليستة واحدة
            var allDistributedAssets = dto.Distributions.SelectMany ( d => d.AssetBarcodesToPlace ).ToList ( );

            // 1. التحقق من العدد
            if ( dto.PickedAssetBarcodes.Count != allDistributedAssets.Count )
                throw new InvalidOperationException ( $"Mismatch count! You picked {dto.PickedAssetBarcodes.Count} items but distributed {allDistributedAssets.Count}." );

            // 2. التحقق من الهوية (هل الباركودات هي هي؟)
            // بنستخدم HashSet عشان السرعة والمقارنة
            // 2. التحقق من الهوية (هل الباركودات هي هي؟)
            var pickedSet = new HashSet<string> ( dto.PickedAssetBarcodes );
            var distributedSet = new HashSet<string> ( allDistributedAssets );

            // التحقق المبدئي لو المجموعتين مش متطابقتين
            if ( !pickedSet.SetEquals ( distributedSet ) )
            {
                // عشان نعرف إيه المختلف، هنعمل نسخة من الليستة الأولى
                var differences = new HashSet<string> ( pickedSet );

                // الدالة دي بتمسح المتشابه وتبقي المختلف فقط (Symmetric Difference)
                // بتعدل على differences مباشرة
                differences.SymmetricExceptWith ( distributedSet );

                // دلوقتي differences فيها الباركودات الغلط (يا إما زيادة يا إما ناقصة)
                throw new InvalidOperationException ( $"Barcode Mismatch! The items distributed do not match the items picked. Check these barcodes: {string.Join ( ", ", differences )}" );
            }

            // ج. التأكد من صحة المخزن المصدر
            var sourceLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == dto.SourceLocationBarcode );
            if ( sourceLocation == null )
                throw new KeyNotFoundException ( "Source Location not found." );

            // د. جلب كل الأصول من الداتابيز دفعة واحدة للتأكد من وجودها ومكانها الحالي
            var assetsInDb = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync ( a => dto.PickedAssetBarcodes.Contains ( a.Barcode ) && ( a.IsDeleted == false || a.IsDeleted == null ) );

            if ( assetsInDb.Count ( ) != dto.PickedAssetBarcodes.Count )
                throw new KeyNotFoundException ( "Some assets do not exist in the system." );

            // هـ. التأكد إن كل الأصول دي فعلاً موجودة جوة المخزن المصدر حالياً
            foreach ( var asset in assetsInDb )
            {
                if ( asset.LocationId != sourceLocation.Id )
                    throw new InvalidOperationException ( $"Asset {asset.Name} ({asset.Barcode}) is NOT currently in the source location {sourceLocation.Name}." );
            }

            // ============================================================
            // 2. Execution Logic (مرحلة التنفيذ داخل Transaction)
            // ============================================================

            var response = new BulkRelocationResponseDTO
            {
                TransactionIds = new List<string> ( ),
                TotalMoved = 0
            };

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                // نلف على كل وجهة (Destination)
                foreach ( var distribution in dto.Distributions )
                {
                    // 1. نجيب المخزن الوجهة
                    var destLocation = await UnitOfWork.readRepository<Location> ( ).GetAsync ( l => l.Barcode == distribution.ToLocationBarcode );
                    if ( destLocation == null )
                        throw new KeyNotFoundException ( $"Destination Location with barcode {distribution.ToLocationBarcode} not found." );

                    // 2. نلف على الأصول اللي رايحة المخزن ده
                    foreach ( var assetBarcode in distribution.AssetBarcodesToPlace )
                    {
                        // نجيب الأصل من اللي سحبناهم فوق (من الميموري عشان نوفر داتابيز)
                        var asset = assetsInDb.First ( a => a.Barcode == assetBarcode );

                        // تجهيز سجل النقل
                        var transferRecord = new AssetTransferRecords
                        {
                            AssetId = asset.Id,
                            FromLocationId = sourceLocation.Id,
                            ToLocationId = destLocation.Id,
                            FromUserId = asset.AssignedUserId,
                            ToUserId = asset.AssignedUserId ,
                            Status = TransferStatus.Approved.ToString ( ),
                            AddedOnDate = DateTime.Now,
                            ApprovalDate = DateOnly.FromDateTime ( DateTime.Now ),
                            IsUserTransfer = false
                        };

                        // حفظ السجل
                        await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );

                        // تحديث مكان الأصل
                        asset.LocationId = destLocation.Id;
                        asset.UpdatedDate = DateTime.Now;
                        await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );

                        // حفظ الـ ID للرد
                        // (ملحوظة: الـ Id بيتكون بعد الـ SaveChange، بس هنا ممكن نستخدم Guid لو النظام بيسمح، أو نعد العدد بس)
                        response.TotalMoved++;
                    }
                }

                // حفظ كل التغييرات مرة واحدة
                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );

                response.Success = true;
                response.Message = "Bulk relocation completed successfully.";

                return response;
            }
            catch ( Exception ex )
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                // Log error here
                throw; // نعيد رمي الخطأ للكنترولر
            }
        }
        #endregion
        #endregion


        //#region 7. Bulk Move Assets to One Location (Direct Move)
        //public async Task<int> BulkMoveAssetsToLocationAsync ( BulkMoveToLocationDTO dto, Guid processedByUserId )
        //{
        //    // 1. التأكد من وجود المكان الهدف
        //    var targetLocation = await UnitOfWork.readRepository<Location> ( )
        //        .GetAsync ( l => l.Barcode == dto.TargetLocationBarcode && ( l.IsDeleted == false || l.IsDeleted == null ) );

        //    if ( targetLocation == null )
        //        throw new KeyNotFoundException ( $"Target Location with barcode '{dto.TargetLocationBarcode}' not found." );

        //    // 2. جلب الأصول من الداتابيز
        //    // بنستخدم Distinct عشان لو الموظف ضرب نفس الباركود مرتين بالغلط
        //    var uniqueBarcodes = dto.AssetBarcodes.Distinct ( ).ToList ( );

        //    var assets = await UnitOfWork.readRepository<Asset> ( )
        //        .GetAllAsync ( a => uniqueBarcodes.Contains ( a.Barcode ) && ( a.IsDeleted == false || a.IsDeleted == null ) );

        //    if ( assets.Count ( ) != uniqueBarcodes.Count )
        //    {
        //        // اختياري: ممكن تطلعله إيه اللي ناقص بالظبط
        //        throw new KeyNotFoundException ( "Some scanned assets do not exist in the system." );
        //    }

        //    // 3. التنفيذ داخل Transaction
        //    int movedCount = 0;
        //    await UnitOfWork.BeginTransactionAsync ( );
        //    try
        //    {
        //        foreach ( var asset in assets )
        //        {
        //            // لو الأصل أصلاً في نفس المكان، ملوش لزمة ننقله ونعمل هيستوري
        //            if ( asset.LocationId == targetLocation.Id )
        //                continue;

        //            // حفظ المكان القديم للسجل
        //            int? oldLocationId = asset.LocationId;

        //            // تحديث المكان الجديد
        //            asset.LocationId = targetLocation.Id;
        //            asset.UpdatedDate = DateTime.Now;

        //            // تسجيل الحركة (Audit Trail)
        //            var transferRecord = new AssetTransferRecords
        //            {
        //                AssetId = asset.Id,
        //                FromLocationId = oldLocationId,
        //                ToLocationId = targetLocation.Id,
        //                FromUserId = asset.AssignedUserId, // الموظف المسؤول عنه (لو فيه)
        //                ToUserId = asset.AssignedUserId ?? Guid.Empty, // بيفضل مع نفس الموظف، بس المكان اتغير
        //                Status = "Moved", // حالة تعبر عن النقل المباشر
        //                AddedOnDate = DateTime.Now,
        //                ApprovalDate = DateOnly.FromDateTime ( DateTime.Now ),
        //                IsUserTransfer = false,
        //                // ممكن نسجل مين الـ Admin اللي عمل الحركة دي لو عندك في الداتابيز
        //                // CreatedByUserId = processedByUserId 
        //            };

        //            await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );
        //            await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );

        //            movedCount++;
        //        }

        //        await UnitOfWork.SaveChangeAsync ( );
        //        await UnitOfWork.CommitTransactionAsync ( );

        //        return movedCount; // بنرجع عدد الحاجات اللي اتنقلت فعلاً
        //    }
        //    catch
        //    {
        //        await UnitOfWork.RollbackTransactionAsync ( );
        //        throw;
        //    }
        //}
        //#endregion


        #region 8. Bulk Move Assets to Multiple Locations (Batch Process)
        public async Task<int> BulkMoveAssetsToMultipleLocationsAsync ( BulkMoveRequestDTO dto, Guid processedByUserId )
        {
            // ==========================================
            // 1. التحقق من صحة البيانات (Validation)
            // ==========================================

            // تجميع كل الباركودات المطلوبة في العملية كلها
            var allAssetBarcodes = dto.Assignments.SelectMany ( a => a.AssetBarcodes ).ToList ( );

            // التأكد من عدم وجود تكرار للأصل في أماكن مختلفة داخل نفس الريكوست
            if ( allAssetBarcodes.Count != allAssetBarcodes.Distinct ( ).Count ( ) )
            {
                throw new InvalidOperationException ( "Duplicate assets found! An asset cannot be moved to two different locations in the same request." );
            }

            // تجميع كل باركودات الأماكن المطلوبة
            var allLocationBarcodes = dto.Assignments.Select ( a => a.TargetLocationBarcode ).Distinct ( ).ToList ( );

            // ==========================================
            // 2. جلب البيانات من الداتابيز (Fetching)
            // ==========================================

            // جلب كل الأماكن المستهدفة دفعة واحدة
            var locations = await UnitOfWork.readRepository<Location> ( )
                .GetAllAsync ( l => allLocationBarcodes.Contains ( l.Barcode ) && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( locations.Count ( ) != allLocationBarcodes.Count )
                throw new KeyNotFoundException ( "One or more Target Locations were not found." );

            // جلب كل الأصول المستهدفة دفعة واحدة
            var assets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync ( a => allAssetBarcodes.Contains ( a.Barcode ) && ( a.IsDeleted == false || a.IsDeleted == null ) );

            if ( assets.Count ( ) != allAssetBarcodes.Distinct ( ).Count ( ) )
                throw new KeyNotFoundException ( "One or more Assets were not found." );

            // ==========================================
            // 3. التنفيذ (Execution)
            // ==========================================

            int totalMovedCount = 0;

            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                // نلف على كل مجموعة (Assignment)
                foreach ( var group in dto.Assignments )
                {
                    // نطلع مكان الهدف من الليستة اللي جبناها
                    var targetLocation = locations.First ( l => l.Barcode == group.TargetLocationBarcode );

                    // نلف على أصول المجموعة دي
                    foreach ( var barcode in group.AssetBarcodes )
                    {
                        var asset = assets.First ( a => a.Barcode == barcode );

                        // لو الأصل أصلاً في نفس المكان، تخطاه
                        if ( asset.LocationId == targetLocation.Id )
                            continue;

                        // حفظ المكان القديم (مع معالجة الـ null بـ 0 أو null حسب تصميم جدولك)
                        // لو جدول الـ History بيقبل null شيل الـ (?? 0)
                        int? oldLocationId = asset.LocationId;

                        // تحديث بيانات الأصل
                        asset.LocationId = targetLocation.Id;
                        asset.UpdatedDate = DateTime.Now;

                        // تسجيل الهيستوري
                        var transferRecord = new AssetTransferRecords
                        {
                            AssetId = asset.Id,
                            FromLocationId = oldLocationId, // حل مشكلة الـ Nullable
                            ToLocationId = targetLocation.Id,
                            FromUserId = asset.AssignedUserId,
                            ToUserId = asset.AssignedUserId ,
                            Status = "Moved",
                            AddedOnDate = DateTime.Now,
                            ApprovalDate = DateOnly.FromDateTime ( DateTime.Now ),
                            IsUserTransfer = false
                        };

                        await UnitOfWork.writeRepository<AssetTransferRecords> ( ).AddAsync ( transferRecord );
                        await UnitOfWork.writeRepository<Asset> ( ).UpdateAsync ( asset.Id, asset );

                        totalMovedCount++;
                    }
                }

                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );

                return totalMovedCount;
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }
        }
        #endregion
    }
}