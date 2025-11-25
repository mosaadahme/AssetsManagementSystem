using AssetsManagementSystem.DTOs.InventoryDTOs;
using AssetsManagementSystem.Models.DbSets;
using AssetsManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssetsManagementSystem.Services.Inventory
{
    public class InventoryService : BaseClassForServices
    {
        public InventoryService ( IUnitOfWork unitOfWork,
            Others.Interfaces.IAutoMapper.IMapper mapper,
            IHttpContextAccessor httpContextAccessor )
            : base ( unitOfWork, mapper, httpContextAccessor )
        {
        }

        #region 1. Start Audit Session
        public async Task<int> StartAuditAsync ( StartAuditRequestDTO dto, Guid auditorId )
        {
            // 1. التأكد من وجود المكان
            var location = await UnitOfWork.readRepository<Location> ( )
                .GetAsync ( l => l.Id == dto.LocationId && ( l.IsDeleted == false || l.IsDeleted == null ) );

            if ( location == null ) throw new KeyNotFoundException ( "Location not found." );

            // 2. التأكد من عدم وجود جلسة مفتوحة لنفس المكان (لتجنب التداخل)
            var pendingAudit = await UnitOfWork.readRepository<InventoryAudit> ( )
                .GetAsync ( a => a.LocationId == dto.LocationId && a.Status == InventoryAuditStatus.Pending );

            if ( pendingAudit != null )
                throw new InvalidOperationException ( $"There is already a pending audit session (ID: {pendingAudit.Id}) for this location." );

            // 3. إنشاء الجلسة
            var audit = new InventoryAudit
            {
                LocationId = dto.LocationId,
                AuditorId = auditorId,
                StartDate = DateTime.Now,
                Status = InventoryAuditStatus.Pending,
                AddedOnDate = DateTime.Now
            };

            await UnitOfWork.writeRepository<InventoryAudit> ( ).AddAsync ( audit );
            await UnitOfWork.SaveChangeAsync ( );

            return audit.Id;
        }
        #endregion

        #region 2. Submit & Analyze Audit (The Brain 🧠)
        public async Task<AuditReportResponseDTO> SubmitAuditAsync ( SubmitAuditRequestDTO dto )
        {
            // 1. جلب بيانات الجلسة
            var audit = await UnitOfWork.readRepository<InventoryAudit> ( )
                .GetAsync ( a => a.Id == dto.AuditId,
                          include: src => src.Include ( a => a.Location ).Include ( a => a.Auditor ) );

            if ( audit == null ) throw new KeyNotFoundException ( "Audit session not found." );

            if ( audit.Status != InventoryAuditStatus.Pending )
                throw new InvalidOperationException ( "This audit session is already completed or cancelled." );

            // 2. تجهيز البيانات المدخلة (Scanned Data)
            // Distinct: لمنع التكرار اللحظي، Case Insensitive: لتوحيد الحروف
            var uniqueScannedBarcodes = dto.ScannedBarcodes
                .Distinct ( StringComparer.OrdinalIgnoreCase )
                .ToList ( );

            // تحضير قائمة التفاصيل للحفظ (History)
            var detailsToAdd = uniqueScannedBarcodes.Select ( barcode => new InventoryAuditDetail
            {
                InventoryAuditId = audit.Id,
                ScannedBarcode = barcode.ToUpper ( ), // بنحفظه Capital دايماً للتوحيد
                ScannedAt = DateTime.Now,
                IsMatched = false // القيمة الافتراضية، هتتحدث تحت
            } ).ToList ( );

            // -------------------------------------------------------------
            // مرحلة التحليل والمقارنة (Core Business Logic)
            // -------------------------------------------------------------

            // أ. إيه اللي المفروض يكون موجود في الغرفة دي؟ (System Snapshot)
            // بنستخدم enableTracing: false لتحسين الأداء لأننا مش هنعدل على الـ Assets دي
            var expectedAssets = await UnitOfWork.readRepository<Asset> ( )
                .GetAllAsync ( a => a.LocationId == audit.LocationId &&
                                  ( a.IsDeleted == false || a.IsDeleted == null ) &&
                                  a.Status != AssetStatus.Retired.ToString ( ),
                             include: src => src.Include ( c => c.Category ),
                             enableTracing: false );

            // ب. تحضير القوائم للمقارنة (HashSets & Dictionaries for O(1) lookup)
            // استخدام OrdinalIgnoreCase هنا هو "كلمة السر" لحل مشاكل السكانر
            var scannedSet = new HashSet<string> ( uniqueScannedBarcodes, StringComparer.OrdinalIgnoreCase );
            var expectedSet = expectedAssets.ToDictionary ( a => a.Barcode, StringComparer.OrdinalIgnoreCase );

            // --- 1. Matched Assets (سليم) ---
            // موجود في السيستم && موجود في الـ Scan
            var matchedAssets = expectedAssets
                .Where ( a => scannedSet.Contains ( a.Barcode ) )
                .Select ( a => new AuditAssetSummaryDTO
                {
                    Barcode = a.Barcode,
                    Name = a.Name,
                    CategoryName = a.Category?.Name ?? "N/A",
                    SerialNumber = a.SerialNumber
                } ).ToList ( );

            // تحديث حالة IsMatched للتفاصيل اللي هتتحفظ
            foreach ( var detail in detailsToAdd )
            {
                if ( expectedSet.ContainsKey ( detail.ScannedBarcode ) )
                {
                    detail.IsMatched = true;
                }
            }

            // --- 2. Missing Assets (عجز/مفقود) ---
            // موجود في السيستم && مطلعش في الـ Scan
            var missingAssets = expectedAssets
                .Where ( a => !scannedSet.Contains ( a.Barcode ) )
                .Select ( a => new AuditAssetSummaryDTO
                {
                    Barcode = a.Barcode,
                    Name = a.Name,
                    CategoryName = a.Category?.Name ?? "N/A",
                    SerialNumber = a.SerialNumber
                } ).ToList ( );

            // --- 3. Extras (زيادات) ---
            // موجود في الـ Scan && مش موجود في قائمة المتوقع
            var extraBarcodes = uniqueScannedBarcodes
                .Where ( b => !expectedSet.ContainsKey ( b ) )
                .ToList ( );

            var displacedAssets = new List<DisplacedAssetInfoDTO> ( );
            var unknownBarcodes = new List<string> ( );

            if ( extraBarcodes.Any ( ) )
            {
                // بنبحث عن الباركودات الزيادة دي في قاعدة البيانات بالكامل
                var foundExtras = await UnitOfWork.readRepository<Asset> ( )
                    .GetAllAsync ( a => extraBarcodes.Contains ( a.Barcode ),
                                 include: src => src.Include ( l => l.Location ).Include ( c => c.Category ),
                                 enableTracing: false );

                foreach ( var barcode in extraBarcodes )
                {
                    // بحث Case-Insensitive
                    var asset = foundExtras.FirstOrDefault ( a => a.Barcode.Equals ( barcode, StringComparison.OrdinalIgnoreCase ) );

                    if ( asset != null )
                    {
                        // Found but Wrong Location (Displaced)
                        displacedAssets.Add ( new DisplacedAssetInfoDTO
                        {
                            Barcode = asset.Barcode,
                            Name = asset.Name,
                            CategoryName = asset.Category?.Name ?? "N/A",
                            SerialNumber = asset.SerialNumber,
                            OriginalLocationName = asset.Location?.Name ?? "Unknown"
                        } );
                    }
                    else
                    {
                        // Not in DB at all (Unknown)
                        unknownBarcodes.Add ( barcode );
                    }
                }
            }

            // 4. الحفظ داخل Transaction لضمان تكامل البيانات
            await UnitOfWork.BeginTransactionAsync ( );
            try
            {
                // حفظ تفاصيل المسح (Bulk Insert)
                await UnitOfWork.writeRepository<InventoryAuditDetail> ( ).AddRangeAsync ( detailsToAdd );

                // تحديث حالة الجلسة (إغلاق الجرد)
                audit.Status = InventoryAuditStatus.Completed;
                audit.EndDate = DateTime.Now;
                await UnitOfWork.writeRepository<InventoryAudit> ( ).UpdateAsync ( audit.Id, audit );

                await UnitOfWork.SaveChangeAsync ( );
                await UnitOfWork.CommitTransactionAsync ( );
            }
            catch
            {
                await UnitOfWork.RollbackTransactionAsync ( );
                throw;
            }

            // 5. إرجاع التقرير النهائي
            return new AuditReportResponseDTO
            {
                AuditId = audit.Id,
                Date = audit.StartDate,
                LocationName = audit.Location.Name,
                AuditorName = audit.Auditor != null ? $"{audit.Auditor.FirstName} {audit.Auditor.LastName}" : "Unknown",
                Status = audit.Status.ToString ( ),

                // إحصائيات
                TotalExpected = expectedAssets.Count,
                TotalScanned = uniqueScannedBarcodes.Count,
                MatchCount = matchedAssets.Count,
                MissingCount = missingAssets.Count,
                DisplacedCount = displacedAssets.Count,

                // قوائم
                MatchedAssets = matchedAssets,
                MissingAssets = missingAssets,
                DisplacedAssets = displacedAssets,
                UnknownBarcodes = unknownBarcodes
            };
        }
        #endregion


        #region 3. Search & History
        public async Task<IEnumerable<AuditHistoryResponseDTO>> GetAuditHistoryAsync ( AuditSearchFilterDTO filter )
        {
            // بناء شرط البحث (Predicate)
            // بنقول: هات السجل لو (الفلتر فاضي OR السجل بيطابق الفلتر)

            var audits = await UnitOfWork.readRepository<InventoryAudit> ( )
                .GetAllByPagningAsync (
                    predicate: a =>
                        ( a.IsDeleted == false || a.IsDeleted == null ) && // شرط أساسي

                        // 1. فلتر المكان
                        ( !filter.LocationId.HasValue || a.LocationId == filter.LocationId ) &&

                        // 2. فلتر التاريخ (من - إلى)
                        ( !filter.FromDate.HasValue || a.StartDate >= filter.FromDate ) &&
                        ( !filter.ToDate.HasValue || a.StartDate <= filter.ToDate ) &&

                        // 3. فلتر الباركود (الأصعب: بحث داخل التفاصيل)
                        // لو باعت باركود، هات الجرد اللي "أي سطر في تفاصيله" بيحتوي الباركود ده
                        ( string.IsNullOrEmpty ( filter.AssetBarcode ) || a.AuditDetails.Any ( d => d.ScannedBarcode == filter.AssetBarcode ) ),

                    // Include: بنحتاج البيانات دي للعرض
                    include: src => src
                        .Include ( a => a.Location )
                        .Include ( a => a.Auditor )
                        .Include ( a => a.AuditDetails ), // بنحتاج التفاصيل عشان نعدها

                    // Pagination
                    currentPage: filter.PageNumber,
                    pageSize: filter.PageSize,

                    // الترتيب: الأحدث أولاً
                    orderby: q => q.OrderByDescending ( d => d.StartDate )
                );

            // Mapping to DTO
            return audits.Select ( a => new AuditHistoryResponseDTO
            {
                AuditId = a.Id,
                LocationName = a.Location?.Name ?? "Unknown",
                AuditorName = a.Auditor != null ? $"{a.Auditor.FirstName} {a.Auditor.LastName}" : "Unknown",
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Status = a.Status.ToString ( ),

                // عدد الحاجات اللي اتعملها Scan
                TotalScannedItems = a.AuditDetails.Count,

                // علامة سريعة لو الجرد ده كان فيه مشاكل (مش كل اللي اتقرأ كان Matched)
                // لو فيه أي سطر IsMatched == false يبقى كان فيه مشكلة
                HasDiscrepancies = a.AuditDetails.Any ( d => !d.IsMatched )
            } ).ToList ( );
        }
        #endregion




    }
}