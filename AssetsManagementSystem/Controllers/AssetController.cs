using AssetsManagementSystem.DTOs.CategoryDTOs;
using AssetsManagementSystem.Services.Categories;
using ClosedXML.Excel;
using System.Data;

namespace AssetsManagementSystem.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AssetController : ControllerBase
    {
        private readonly AssetService _assetService;
        private readonly ILogger<AssetController> _logger;

        public AssetController(AssetService assetService, ILogger<AssetController> logger)
        {
            _assetService = assetService;
            _logger = logger;
         }

        #region Add Asset
        [HttpPost]
        // [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddAsset ( [FromForm] AddAssetRequestDTO addAssetDto )
        {
            try
            {
                _logger.LogInformation ( "Adding new assets of type: {AssetName}", addAssetDto.Name );

                // النتيجة هنا عبارة عن List<string> فيها الباركودات الجديدة
                var result = await _assetService.AddAssetAsync ( addAssetDto );

                // بنرجع 201 Created مع القائمة عشان الفرونت يطبع الباركودات
                return StatusCode ( StatusCodes.Status201Created, new { barcodes = result, message = "Assets added successfully" } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error adding assets: {Message}", ex.Message );
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion

        #region Get Asset by Barcode (Updated)
        // غيرنا الاسم والروت لـ barcode
        [HttpGet ( "{barcode}" )]
        // [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> GetAssetByBarcode ( string barcode )
        {
            try
            {
                _logger.LogInformation ( "Fetching asset with Barcode: {Barcode}", barcode );
                var result = await _assetService.GetAssetByBarcodeAsync ( barcode ); // تأكد إن السيرفيس اسمها كده
                return Ok ( result );
            }
            catch ( KeyNotFoundException ex )
            {
                _logger.LogWarning ( "Asset not found: {Barcode}", barcode );
                return NotFound ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error fetching asset: {Barcode}", barcode );
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion

        #region Get Asset for Current User
        [HttpGet]
        public async Task<IActionResult> GetAssetForCurrentUser ( )
        {
            try
            {
                var result = await _assetService.GetAssetsForCurrentUser ( );
                return Ok ( result );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error fetching user assets" );
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion

        #region Get All Assets (Pagination)
        // دمجنا الـ GetAll مع Pagination كـ Best Practice
        // لو مبعتش بارامترز هياخد الديفولت
        [HttpGet]
        // [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> GetAllAssets ( )
        {
            try
            {
                _logger.LogInformation ( "Fetching assets (Page: {Page}, Size: {Size})" );

                // هنا ممكن تنادي GetAllByPaginationAssetsAsync علطول
                var result = await _assetService.GetAllByPaginationAssetsAsync ( 1, 1000000 );
                return Ok ( result );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error fetching all assets" );
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion

        #region Get All Assets ByPagination
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetAssetsByPagination(int currentPage , int pageSize )
        {
            try
            {
                _logger.LogInformation("Fetching all assets.");
                var result = await _assetService.GetAllByPaginationAssetsAsync(currentPage,pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all assets.");
                return BadRequest(new { error = ex.Message });
            }
        }
        #endregion

        #region Update Asset
        [HttpPut ( "{barcode}" )] // الباركود بيتبعت في الـ URL
                                  // [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateAsset ( string barcode, [FromForm] UpdateAssetRequestDTO updateAssetDto )
        {
            try
            {
                _logger.LogInformation ( "Updating asset: {Barcode}", barcode );
                var result = await _assetService.UpdateAssetAsync ( barcode, updateAssetDto );
                return Ok ( result );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex ) // للـ Validation Errors
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error updating asset: {Barcode}", barcode );
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion

        #region withdraw asset 

        [HttpPost]
        public async Task<IActionResult> withdrawAsset ( int Id, int quantity )
        {
            try
            {
                _logger.LogInformation ( "Updating asset with ID: {AssetId}", Id );
                var result = await _assetService.WithdrawQuantityAsync ( Id, quantity );
                return Ok ( result );
            }
            catch ( KeyNotFoundException ex )
            {
                _logger.LogWarning ( ex, "Asset with ID: {AssetId} not found", Id );
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex )
            {
                _logger.LogWarning ( ex, "Invalid operation while updating asset with ID: {AssetId}", Id );
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "An error occurred while updating the asset with ID: {AssetId}", Id );
                return BadRequest ( new { error = ex.Message } );
            }
        }


        #endregion

        #region Delete Asset
        [HttpDelete ( "{barcode}" )]
        // [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteAsset ( string barcode )
        {
            try
            {
                _logger.LogInformation ( "Deleting asset: {Barcode}", barcode );
                await _assetService.DeleteAssetAsync ( barcode );
                return NoContent ( ); // 204 No Content
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex ) // لو الأصل محجوز ومش هينفع يتمسح
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error deleting asset: {Barcode}", barcode );
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion


        #region File Excel
        //[HttpGet("ExportExcelOfAsset")]
        //public async Task<IActionResult> ExportExcelOfAsset() 
        //{

        //    using (XLWorkbook xLWorkbook=new XLWorkbook())
        //    {

        //        xLWorkbook.AddWorksheet(GetDataOfAssets().Result,$"SheetOfAsset");
        //        using (MemoryStream ms=new MemoryStream())
        //        {
        //            xLWorkbook.SaveAs(ms);
        //            return File(ms.ToArray(),
        //                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                        $"Sheet Of Asset for Date {DateOnly.FromDateTime(DateTime.Now)}");

        //        }

        //    }


        // }


        //[NonAction]
        //private async Task<DataTable> GetDataOfAssets() 
        //{

        //    var data =await _assetService.GetAllAssetsAsync();

        //    DataTable dt = new DataTable();

        //    dt.Columns.Add("Id", typeof(int));
        //    dt.Columns.Add("Name", typeof(string));
        //    dt.Columns.Add("ModelNumber", typeof(string));
        //    dt.Columns.Add("SerialNumber", typeof(string));
        //    dt.Columns.Add("PurchaseDate", typeof(DateOnly));
        //    dt.Columns.Add("PurchasePrice", typeof(decimal));
        //    dt.Columns.Add("WarrantyExpiryDate", typeof(DateOnly));
        //    dt.Columns.Add("DepreciationDate", typeof(DateOnly));
        //    dt.Columns.Add("Status", typeof(string));
        //    dt.Columns.Add("Description", typeof(string));
        //    dt.Columns.Add("LocationName", typeof(string));
        //    dt.Columns.Add("AssignedUserName", typeof(string));
        //    dt.Columns.Add("CategoryName", typeof(string));
        //    dt.Columns.Add("ManfactureName", typeof(string));
        //    dt.Columns.Add("AddedOnDate", typeof(DateOnly));
        //    dt.Columns.Add("UpdatedDate", typeof(DateOnly));


        //    if (data.Count>0)
        //    {
        //        foreach (var d in data)
        //        {
        //            dt.Rows.Add(d.Id, d.Name, d.ModelNumber,
        //                            d.SerialNumber, d.PurchaseDate, d.PurchasePrice, d.WarrantyExpiryDate,
        //                            d.DepreciationDate, d.Status, d.dicription, d.LocationName, d.AssignedUserName,
        //                            d.ManfactureName, DateOnly.FromDateTime(d.AddedOnDate),
        //                                DateOnly.FromDateTime(d.UpdatedDate ?? new DateTime())

        //                       );
        //        }


        //    }

        //    return dt;

        //}

        #endregion

        #region ReadAssetsFromExcel
        //[HttpPost("ReadCategoriesFromExcel")]
        //public async Task<IActionResult> ReadAssetsFromExcel(IFormFile file)
        //{
        //    var targetColor = XLColor.FromArgb(146, 212, 193);

        //    using (var stream = new MemoryStream())
        //    {
        //        await file.CopyToAsync(stream);

        //        using (var workbook = new XLWorkbook(stream))
        //        {
        //            var worksheet = workbook.Worksheet(1);
        //            var rows = worksheet.RowsUsed().Skip(1); // تجاوز العنوان (الصف الأول)

        //            foreach (var row in rows)
        //            {
        //                bool isCustomColor = row.CellsUsed().All(cell => cell.Style.Fill.BackgroundColor == targetColor);

        //                if (isCustomColor)
        //                {
        //                    // إيجاد آخر خلية غير فارغة في الصف
        //                    int lastNonEmptyCellIndex = 0;
        //                    for (int i = row.LastCellUsed().Address.ColumnNumber; i >= 1; i--)
        //                    {
        //                        if (!row.Cell(i).IsEmpty())
        //                        {
        //                            lastNonEmptyCellIndex = i;
        //                            break;
        //                        }
        //                    }

        //                    // استخراج SerialNumber (الخلية الأخيرة غير الفارغة)
        //                    string serialNumber = lastNonEmptyCellIndex >= 1 ? row.Cell(lastNonEmptyCellIndex).GetString() : null;

        //                    // استخراج CategoryId (الخلية التي قبل الأخيرة)
        //                    string categoryId = lastNonEmptyCellIndex >= 2 ? row.Cell(lastNonEmptyCellIndex - 1).GetString() : null;

        //                    // هنا يمكن استخدام المتغيرات حسب الحاجة
        //                    // على سبيل المثال، يمكن طباعة أو استخدام القيم في منطق معين:
        //                    //Console.WriteLine($"Serial Number: {serialNumber}, Category ID: {categoryId}");

        //                    // إذا كنت تريد إضافة البيانات إلى قاعدة البيانات أو استخدام الخدمة
        //                    await _assetService.AddAssetAsync(new AddAssetRequestDTO()
        //                    {
        //                        Name = row.Cell(1).GetString(),
        //                        SerialNumber = serialNumber,
        //                        CategoryId = categoryId,
        //                        dicription = row.Cell(1).GetString(),
        //                        ModelNumber = serialNumber,
        //                        Status = AssetStatus.Active,
        //                        LocationId = 1,
        //                        ManufacturerId = 1,
        //                        SupplierIds = new List<int>() { 1 },
        //                        AssignedUserId = Guid.Parse("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
        //                        PurchasePrice = 0,
        //                        PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
        //                        WarrantyExpiryDate = DateOnly.FromDateTime(DateTime.Now).AddDays(1),
        //                        DepreciationDate = DateOnly.FromDateTime(DateTime.Now)


        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return Ok(new { Message = "تم استخراج SerialNumber و CategoryId بنجاح" });
        //}
        #endregion
    }
}

 