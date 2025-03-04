using AssetsManagementSystem.DTOs.ManufacturerDTOs;
using AssetsManagementSystem.Services.Locations;
using AssetsManagementSystem.Services.Manfacture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ManufactureController : ControllerBase
    {
        private readonly ManfactureService _manfactureService;
        private readonly ILogger<ManufactureController> _logger;

        public ManufactureController(ManfactureService manfactureService, ILogger<ManufactureController> logger)
        {
            _manfactureService = manfactureService;
            _logger = logger;
        }

        [HttpPost]
        //[Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> AddManfacture([FromBody] AddManufacturerRequestDTO addManufacturerRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for AddManfacture");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Attempting to add a new manufacturer.");
                var result = await _manfactureService.AddManfacture(addManufacturerRequest);
                return CreatedAtAction(nameof(GetManfactureById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Error occurred while adding manufacturer: {ex.Message}");
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        #region GetAllManufactutre

        [HttpGet("all")]
        //[Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetAllManufacture()
        {
            try
            {
                var Manufactures = await _manfactureService.GetAll();
                _logger.LogInformation("All Manufactures retrieved successfully.");
                return Ok(Manufactures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all Manufactures.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the Manufactures", Details = ex.Message });
            }
        }

        #endregion


        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> GetManfactureById(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching manufacturer with ID {id}");
                var result = await _manfactureService.GetManfactureBtId(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Error fetching manufacturer: {ex.Message}");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("update/{id}")]
        //[Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> UpdateManfacture(int id, [FromBody] UpdateManufacturerRequestDTO updateManufacturerRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateManfacture");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation($"Attempting to update manufacturer with ID {id}");
                await _manfactureService.UpdateManfacuture(id, updateManufacturerRequest);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Error updating manufacturer: {ex.Message}");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> DeleteManfacture(int id)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete manufacturer with ID {id}");
                await _manfactureService.DeleteManfacute(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Error deleting manufacturer: {ex.Message}");
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
