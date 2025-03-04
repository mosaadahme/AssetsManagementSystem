namespace AssetsManagementSystem.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(LocationService locationService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        #region AddNewLocation

        [HttpPost("add")]
        //[Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddLocation([FromBody] AddLocationRequestDTO addLocationRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for AddLocation request");
                return BadRequest(ModelState);
            }

            try
            {
                await _locationService.AddLocationAsync(addLocationRequest);
                _logger.LogInformation($"Location '{addLocationRequest.Name}' added successfully.");
                return Ok(new { Message = "Location added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Attempt to add a duplicate location: {addLocationRequest.Name}");
                return Conflict(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding location.");
                return StatusCode(500, new { Error = "An error occurred while adding the location", Details = ex.Message });
            }
        }

        #endregion

        #region GetLocationById

        [HttpGet()]
        [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetLocationById(string locationBarcode)
        {
            if (string.IsNullOrEmpty(locationBarcode))
            {
                _logger.LogWarning("Invalid location ID in GetLocationById request");
                return BadRequest(new { Error = "Invalid location ID" });
            }

            try
            {
                var location = await _locationService.GetLocationByIdAsync(locationBarcode);
                _logger.LogInformation($"Location with ID {locationBarcode} retrieved successfully.");
                return Ok(location);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Location with ID {locationBarcode} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving location.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the location", Details = ex.Message });
            }
        }

        #endregion

        #region GetAllLocations

        [HttpGet("all")]
        //[Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetAllLocations()
        {
            try
            {
                var locations = await _locationService.GetAllLocationsAsync();
                _logger.LogInformation("All locations retrieved successfully.");
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all locations.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the locations", Details = ex.Message });
            }
        }

        #endregion

        #region GetByPaginationLocations

        [HttpGet("ByPagination")]
        [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetLocationsByPagination(int currentPage, int pageSize)
        {
            try
            {
                var locations = await _locationService.GetAllByPaginationLocationsAsync(currentPage, pageSize);
                _logger.LogInformation("All locations retrieved successfully.");
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all locations.");
                return StatusCode(500, new { Error = "An error occurred while retrieving the locations", Details = ex.Message });
            }
        }

        #endregion

        #region UpdateLocation

        [HttpPut("update/{locationBarcode}")]
        //[Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> UpdateLocation(string locationBarcode, [FromBody] UpdateLocationRequestDTO updateLocationRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateLocation request");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(locationBarcode))
            {
                _logger.LogWarning("Invalid location ID in UpdateLocation request");
                return BadRequest(new { Error = "Invalid location ID" });
            }

            try
            {
                await _locationService.UpdateLocationAsync(locationBarcode, updateLocationRequest);
                _logger.LogInformation($"Location with ID {locationBarcode} updated successfully.");
                return Ok(new { Message = "Location updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Attempt to update location with duplicate name: {updateLocationRequest.Name}");
                return Conflict(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Location with ID {locationBarcode} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating location.");
                return StatusCode(500, new { Error = "An error occurred while updating the location", Details = ex.Message });
            }
        }

        #endregion

        #region DeleteLocation

        [HttpDelete("delete/{locationBarcode}")]
        //[Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> DeleteLocation(string locationBarcode)
        {
            if (string.IsNullOrEmpty(locationBarcode))
            {
                _logger.LogWarning("Invalid location ID in DeleteLocation request");
                return BadRequest(new { Error = "Invalid location ID" });
            }

            try
            {
                await _locationService.DeleteLocationAsync(locationBarcode);
                _logger.LogInformation($"Location with ID {locationBarcode} deleted successfully.");
                return Ok(new { Message = "Location deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Location with ID {locationBarcode} not found.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting location.");
                return StatusCode(500, new { Error = "An error occurred while deleting the location", Details = ex.Message });
            }
        }

        #endregion
    }
}
