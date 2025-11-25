using AssetsManagementSystem.DTOs.CategoryDTOs;
using AssetsManagementSystem.Services.Assets;
using AssetsManagementSystem.Services.Categories;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AssetsManagementSystem.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(CategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }


        #region AddCategory 
        [HttpPost("add")]
        //[Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequestDTO addCategoryRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for AddCategory request");
                return BadRequest(ModelState);
            }

            try
            {
                await _categoryService.AddCategoryAsync(addCategoryRequest);
                _logger.LogInformation($"Category '{addCategoryRequest.Name}' added successfully.");
                return Ok(new { Message = "Category added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Attempt to add a duplicate category: {addCategoryRequest.Name}");
                return Conflict(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding category.");
                return StatusCode(500, new { Error = "An error occurred while adding the category", Details = ex.Message });
            }
        }
        #endregion

        #region GetCategoryById
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetCategoryById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid category ID in GetCategoryById request");
                return BadRequest(new { Error = "Invalid category ID" });
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                _logger.LogInformation($"Category with ID {id} retrieved successfully.");
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Category with ID {id} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving category.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the category", Details = ex.Message });
            }
        }
        #endregion

        #region GetAllCategories
        [HttpGet("all")]
       // [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                _logger.LogInformation("All categories retrieved successfully.");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all categories.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the categories", Details = ex.Message });
            }
        }
        #endregion

        #region GetAllCategories
        [HttpGet("MainCategories")]
       // [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetMainCategory()
        {
            try
            {
                var categories = await _categoryService.GetMainCategory();
                _logger.LogInformation("All categories retrieved successfully.");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all categories.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the categories", Details = ex.Message });
            }
        }
        #endregion

        #region GetSubCategory
        [HttpGet("GetSubCategory")]
       // [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetSubCategory(int id)
        {
            try
            {
                var categories = await _categoryService.GetSubCategoriesAsync ( id);
                _logger.LogInformation("All categories retrieved successfully.");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all categories.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the categories", Details = ex.Message });
            }
        }
        #endregion

        #region Get Categories By Asset Type
        [HttpGet ( "ByType/{assetType}" )] // Example: api/Category/GetCategoriesByType/0
        public async Task<IActionResult> GetCategoriesByType ( AssetType assetType )
        {
            try
            {
                // 1. استدعاء السيرفيس (لازم نضيف الدالة دي في السيرفيس الأول)
                var categories = await _categoryService.GetCategoriesByTypeAsync ( assetType );
                return Ok ( categories );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error retrieving categories for type {AssetType}", assetType );
                return StatusCode ( 500, new { Error = "An error occurred while retrieving categories." } );
            }
        }
        #endregion

        #region GetByPaginationCategories
        [HttpGet ("ByPagination")]
        [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetCategoriesByPagination(int currentPage, int pageSize)
        {
            try
            {
                var categories = await _categoryService.GetAllByPaginationCategoriesAsync(currentPage, pageSize);
                _logger.LogInformation("All categories retrieved successfully.");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all categories.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the categories", Details = ex.Message });
            }
        }
        #endregion


        #region UpdateCategory
        [HttpPut("update/{id:int}")]
        [Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequestDTO updateCategoryRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateCategory request");
                return BadRequest(ModelState);
            }

            if (id <= 0)
            {
                _logger.LogWarning("Invalid category ID in UpdateCategory request");
                return BadRequest(new { Error = "Invalid category ID" });
            }

            try
            {
                await _categoryService.UpdateCategoryAsync(id, updateCategoryRequest);
                _logger.LogInformation($"Category with ID {id} updated successfully.");
                return Ok(new { Message = "Category updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Attempt to update category with duplicate name: {updateCategoryRequest.Name}");
                return Conflict(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Category with ID {id} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category.");
                return StatusCode(500, new { Error = "An error occurred while updating the category", Details = ex.Message });
            }
        }
        #endregion

        #region DeleteCategory

        [HttpDelete("delete/{id:int}")]
       // [Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid category ID in DeleteCategory request");
                return BadRequest(new { Error = "Invalid category ID" });
            }

            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                _logger.LogInformation($"Category with ID {id} deleted successfully.");
                return Ok(new { Message = "Category deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Category with ID {id} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category.");
                return StatusCode(500, new { Error = "An error occurred while deleting the category", Details = ex.Message });
            }
        }
        #endregion


        [HttpPost("ReadCategoriesFromExcel")]
        public async Task<IActionResult> ReadCategoriesFromExcel(IFormFile file)
        {
            var targetColor = XLColor.Yellow; // اللون الأصفر المستهدف

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // تجاوز العنوان (الصف الأول)

                    foreach (var row in rows)
                    {
                        var cellColor = row.Cell(1).Style.Fill.BackgroundColor;

                        if (cellColor == targetColor)
                        {
                            // إيجاد آخر خلية غير فارغة في الصف
                            int lastNonEmptyCellIndex = 0;
                            for (int i = row.LastCellUsed().Address.ColumnNumber; i >= 1; i--)
                            {
                                if (!row.Cell(i).IsEmpty())
                                {
                                    lastNonEmptyCellIndex = i;
                                    break;
                                }
                            }

                            // استخراج SerialCode (الخلية قبل الأخيرة)
                            string serialCode = lastNonEmptyCellIndex >= 2 ? row.Cell(lastNonEmptyCellIndex).GetString() : null;

                            // استخراج ParentCategoryId (الخلية الثالثة قبل الأخيرة)
                            int parentCategoryId = lastNonEmptyCellIndex >= 3 && !string.IsNullOrEmpty(row.Cell(lastNonEmptyCellIndex - 1).GetString())
                                ? int.Parse(row.Cell(lastNonEmptyCellIndex - 1).GetString())
                                : 101; // قيمة افتراضية إذا كانت البيانات غير متوفرة

                            // إضافة الفئة بالـ SerialCode و ParentCategoryId
                            await _categoryService.AddCategoryAsync(new AddCategoryRequestDTO
                            {
                                Name = row.Cell(1).GetString(), // اسم الفئة (العمود الأول)
                                Description = lastNonEmptyCellIndex==5?"مستوى 4":"مستوى 5",
                                SerialCode = serialCode, // SerialCode من الخلية قبل الأخيرة
                                ParentCategoryId = parentCategoryId // ParentCategoryId من الخلية التي قبلها
                            });
                        }
                        else
                        {
                            continue; // تخطي الصفوف التي لا تطابق اللون المستهدف
                        }
                    }
                }
            }

            return Ok(new { Message = "تم تحميل الفئات ذات اللون الأصفر بنجاح" });
        }



        #region File Excel
        [HttpGet("ExportExcelOfAsset")]
        public async Task<IActionResult> ExportExcelOfAsset()
        {

            using (XLWorkbook xLWorkbook = new XLWorkbook())
            {

                xLWorkbook.AddWorksheet(GetDataOfAssets().Result, $"SheetOfCategories");
                using (MemoryStream ms = new MemoryStream())
                {
                    xLWorkbook.SaveAs(ms);
                    return File(ms.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"Sheet Of Categories for Date {DateOnly.FromDateTime(DateTime.Now)}");

                }

            }


        }


        [NonAction]
        private async Task<DataTable> GetDataOfAssets()
        {

            var data = await _categoryService.GetAllCategoriesAsync();

            DataTable dt = new DataTable();

            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Category Name", typeof(string));
            dt.Columns.Add("Description", typeof(string));

            dt.Columns.Add("Parent Category Id", typeof(int));
            dt.Columns.Add("Parent Category Name", typeof(string));

            dt.Columns.Add("AddedOnDate", typeof(DateOnly));
            dt.Columns.Add("UpdatedDate", typeof(DateOnly));            
                foreach (var d in data)
                {
                    dt.Rows.Add(d.Id, 
                                d.Name,
                                d.Description,
                                d.ParentCategoryId,
                                d.ParentCategoryName,
                                DateOnly.FromDateTime(d.AddedOnDate),
                                DateOnly.FromDateTime(d.UpdatedDate ?? new DateTime())

                               );
                }


          

            return dt;

        }

        #endregion


    }
}
//   SUPER_ADMIN@GMAIL.COM



 