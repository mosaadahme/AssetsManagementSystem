using AssetsManagementSystem.DTOs.AssetMaintenanceDTOs;
using AssetsManagementSystem.Services.AssetMaintenance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AssetMaintenanceController : ControllerBase
    {
        private readonly AssetMaintenanceService _assetMaintenanceService;
        private readonly ILogger<AssetMaintenanceController> _logger;

        public AssetMaintenanceController(AssetMaintenanceService assetMaintenanceService, ILogger<AssetMaintenanceController> logger)
        {
            _assetMaintenanceService = assetMaintenanceService;
            _logger = logger;
        }


        [HttpPost("AddMaintenanceRecord")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddMaintenanceRecord([FromBody] AddAssetMaintenanceRecordDTO maintenanceDto)
        {
            try
            {
                _logger.LogInformation("Attempting to add a new maintenance record for asset ID {AssetId}", maintenanceDto.AssetSerialNumber);

                await _assetMaintenanceService.AddMaintenanceRecordAsync(maintenanceDto);

                _logger.LogInformation("Successfully added maintenance record for asset ID {AssetId}", maintenanceDto.AssetSerialNumber);

                return Ok(new { message = "Maintenance record added successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to add maintenance record: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the maintenance record for asset ID {AssetId}", maintenanceDto.AssetSerialNumber);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }


        [HttpGet("GetMaintenanceRecordById/{maintenanceRecordId}")]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> GetMaintenanceRecordById(int maintenanceRecordId)
        {
            try
            {
                _logger.LogInformation("Fetching maintenance record ID {MaintenanceRecordId}", maintenanceRecordId);

                var maintenanceRecord = await _assetMaintenanceService.GetMaintenanceRecordByIdAsync(maintenanceRecordId);

                _logger.LogInformation("Successfully fetched maintenance record ID {MaintenanceRecordId}", maintenanceRecordId);
                return Ok(maintenanceRecord);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Maintenance record not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching maintenance record ID {MaintenanceRecordId}", maintenanceRecordId);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }



        [HttpGet("GetAllMaintenanceRecords")]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> GetAllMaintenanceRecords()
        {
            try
            {
                _logger.LogInformation("Fetching all maintenance records.");

                var maintenanceRecords = await _assetMaintenanceService.GetAllMaintenanceRecordsAsync();

                if (maintenanceRecords == null || maintenanceRecords.Count == 0)
                {
                    _logger.LogWarning("No maintenance records found.");
                    return NotFound(new { message = "No maintenance records found." });
                }

                _logger.LogInformation("Successfully fetched all maintenance records.");
                return Ok(maintenanceRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all maintenance records.");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpGet("GetByPaginationMaintenanceRecords")]
        [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> GetByPaginationMaintenanceRecords(int currentPage, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching all maintenance records.");

                var maintenanceRecords = await _assetMaintenanceService.GetAllByPaginationMaintenanceRecordsAsync(currentPage, pageSize);

                if (maintenanceRecords == null || maintenanceRecords.Count == 0)
                {
                    _logger.LogWarning("No maintenance records found.");
                    return NotFound(new { message = "No maintenance records found." });
                }

                _logger.LogInformation("Successfully fetched all maintenance records.");
                return Ok(maintenanceRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all maintenance records.");
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }


        [HttpPut("UpdateMaintenanceRecord")]
        [Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> UpdateMaintenanceRecord([FromBody] UpdateAssetMaintenanceRecordDTO maintenanceDto)
        {
            try
            {
                _logger.LogInformation("Attempting to update maintenance record ID {MaintenanceRecordId}", maintenanceDto.Id);

                await _assetMaintenanceService.UpdateMaintenanceRecordAsync(maintenanceDto);

                _logger.LogInformation("Successfully updated maintenance record ID {MaintenanceRecordId}", maintenanceDto.Id);

                return Ok(new { message = "Maintenance record updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to update maintenance record: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the maintenance record ID {MaintenanceRecordId}", maintenanceDto.Id);
                return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
            }
        }

        #region
        //[HttpDelete("DeleteMaintenanceRecord/{maintenanceRecordId}")]
        //public async Task<IActionResult> DeleteMaintenanceRecord(int maintenanceRecordId)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Attempting to delete maintenance record ID {MaintenanceRecordId}", maintenanceRecordId);

        //        await _assetMaintenanceService.DeleteMaintenanceRecordAsync(maintenanceRecordId);

        //        _logger.LogInformation("Successfully deleted maintenance record ID {MaintenanceRecordId}", maintenanceRecordId);

        //        return Ok(new { message = "Maintenance record deleted successfully." });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        _logger.LogWarning(ex, "Failed to delete maintenance record: {Message}", ex.Message);
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while deleting the maintenance record ID {MaintenanceRecordId}", maintenanceRecordId);
        //        return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        //    }
        //}
        #endregion

    }
}
