using AssetsManagementSystem.DTOs.SupplierDTOs;
using AssetsManagementSystem.Services.Suppliers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route("api/[controller]/[action]")]
    //[ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly SupplierService _supplierService;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(SupplierService supplierService, ILogger<SupplierController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        #region GET: api/Supplier/{id}
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetSupplierById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid Supplier ID passed to GetSupplierById.");
                return BadRequest(new { Error = "Invalid Supplier ID" });
            }

            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                return Ok(supplier);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Supplier with ID {id} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving supplier.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the supplier.", Details = ex.Message });
            }
        }
        #endregion

        #region GET: api/Supplier
        [HttpGet]
        //[Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetAllSuppliers()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving suppliers.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the suppliers.", Details = ex.Message });
            }
        }
        #endregion


        #region GET: api/Supplier
        [HttpGet]
        //[Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetSuppliersByPagination(int currentPage, int pageSize)
        {
            try
            {
                var suppliers = await _supplierService.GetAllByPaginationSuppliersAsync(currentPage, pageSize);
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving suppliers.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the suppliers.", Details = ex.Message });
            }
        }
        #endregion

        #region POST: api/Supplier
        [HttpPost]
        //[Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> AddSupplier([FromBody] AddSupplierRequestDTO addSupplierRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for AddSupplier request.");
                return BadRequest(ModelState);
            }

            try
            {
                await _supplierService.AddSupplierAsync(addSupplierRequest);
                _logger.LogInformation($"Supplier '{addSupplierRequest.CompanyName}' added successfully.");
                return Ok(new { Message = "Supplier added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Attempt to add a duplicate supplier.");
                return Conflict(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding supplier.");
                return StatusCode(500, new { Error = "An error occurred while adding the supplier.", Details = ex.Message });
            }
        }
        #endregion

        #region PUT: api/Supplier/{id}
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequestDTO updateSupplierRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateSupplier request.");
                return BadRequest(ModelState);
            }

            if (id <= 0)
            {
                _logger.LogWarning("Invalid Supplier ID passed to UpdateSupplier.");
                return BadRequest(new { Error = "Invalid Supplier ID" });
            }

            try
            {
                await _supplierService.UpdateSupplierAsync(id, updateSupplierRequest);
                _logger.LogInformation($"Supplier with ID {id} updated successfully.");
                return Ok(new { Message = "Supplier updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Supplier with ID {id} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Attempt to update supplier with a duplicate name.");
                return Conflict(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating supplier.");
                return StatusCode(500, new { Error = "An error occurred while updating the supplier.", Details = ex.Message });
            }
        }
        #endregion

        #region DELETE: api/Supplier/{id}
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> DeleteSupplier(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid Supplier ID passed to DeleteSupplier.");
                return BadRequest(new { Error = "Invalid Supplier ID" });
            }

            try
            {
                await _supplierService.DeleteSupplierAsync(id);
                _logger.LogInformation($"Supplier with ID {id} deleted successfully (soft delete).");
                return Ok(new { Message = "Supplier deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Supplier with ID {id} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting supplier.");
                return StatusCode(500, new { Error = "An error occurred while deleting the supplier.", Details = ex.Message });
            }
        }
        #endregion
    }
}
