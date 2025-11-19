using AssetsManagementSystem.DTOs.LocationDTOs;
using AssetsManagementSystem.Services.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route ( "api/[controller]/[action]" )]
    [ApiController]
    // [Authorize] // يفضل تفعيلها على مستوى الكنترولر
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;
        private readonly ILogger<LocationController> _logger;

        public LocationController ( LocationService locationService, ILogger<LocationController> logger )
        {
            _locationService = locationService;
            _logger = logger;
        }

        #region Add New Location
        [HttpPost] // الروت هيكون: api/Location/AddLocation
        // [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddLocation ( [FromBody] AddLocationRequestDTO addLocationRequest )
        {
            // ملحوظة: [ApiController] بيعمل Check لـ ModelState تلقائي، بس لو حابب تعمل Log سيبه
            if ( !ModelState.IsValid )
            {
                _logger.LogWarning ( "Invalid model state for AddLocation request" );
                return BadRequest ( ModelState );
            }

            try
            {
                await _locationService.AddLocationAsync ( addLocationRequest );
                _logger.LogInformation ( $"Location '{addLocationRequest.Name}' added successfully." );

                // بنرجع 201 Created
                return StatusCode ( StatusCodes.Status201Created, new { Message = "Location added successfully" } );
            }
            catch ( InvalidOperationException ex ) // تكرار الاسم أو الباركود
            {
                _logger.LogWarning ( ex, "Duplicate location attempt." );
                return Conflict ( new { Error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error occurred while adding location." );
                return StatusCode ( 500, new { Error = "An internal error occurred.", Details = ex.Message } );
            }
        }
        #endregion

        #region Get Location By Barcode
        // الروت هيكون: api/Location/GetLocationByBarcode/LOC-001
        [HttpGet ( "{barcode}" )]
        // [Authorize(Roles = "Admin,Manager,Auditor")]
        public async Task<IActionResult> GetLocationByBarcode ( string barcode )
        {
            if ( string.IsNullOrEmpty ( barcode ) )
            {
                return BadRequest ( new { Error = "Location Barcode is required" } );
            }

            try
            {
                // تم تعديل اسم الدالة لتطابق السيرفيس الجديدة
                var location = await _locationService.GetLocationByBarcodeAsync ( barcode );
                return Ok ( location );
            }
            catch ( KeyNotFoundException ex )
            {
                _logger.LogWarning ( "Location not found: {Barcode}", barcode );
                return NotFound ( new { Error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error retrieving location: {Barcode}", barcode );
                return BadRequest ( new { Error = ex.Message } );
            }
        }
        #endregion

        #region Get All Locations
        [HttpGet] // الروت: api/Location/GetAllLocations
        public async Task<IActionResult> GetAllLocations ( )
        {
            try
            {
                var locations = await _locationService.GetAllLocationsAsync ( );
                return Ok ( locations );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error retrieving all locations." );
                return BadRequest ( new { Error = ex.Message } );
            }
        }
        #endregion

        #region Get Locations By Pagination
        [HttpGet] // الروت: api/Location/GetLocationsByPagination?currentPage=1&pageSize=10
        public async Task<IActionResult> GetLocationsByPagination ( [FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10 )
        {
            try
            {
                var locations = await _locationService.GetAllByPaginationLocationsAsync ( currentPage, pageSize );
                return Ok ( locations );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error retrieving paginated locations." );
                return BadRequest ( new { Error = ex.Message } );
            }
        }
        #endregion

        #region Update Location
        // الروت: api/Location/UpdateLocation/LOC-001
        [HttpPut ( "{barcode}" )]
        // [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateLocation ( string barcode, [FromBody] UpdateLocationRequestDTO updateLocationRequest )
        {
            if ( string.IsNullOrEmpty ( barcode ) ) return BadRequest ( new { Error = "Barcode is required" } );

            try
            {
                await _locationService.UpdateLocationAsync ( barcode, updateLocationRequest );

                _logger.LogInformation ( "Location updated: {Barcode}", barcode );
                return Ok ( new { Message = "Location updated successfully" } );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { Error = ex.Message } );
            }
            catch ( InvalidOperationException ex ) // تكرار الاسم
            {
                return Conflict ( new { Error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error updating location: {Barcode}", barcode );
                return BadRequest ( new { Error = ex.Message } );
            }
        }
        #endregion

        #region Delete Location
        // الروت: api/Location/DeleteLocation/LOC-001
        [HttpDelete ( "{barcode}" )]
        // [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteLocation ( string barcode )
        {
            if ( string.IsNullOrEmpty ( barcode ) ) return BadRequest ( new { Error = "Barcode is required" } );

            try
            {
                await _locationService.DeleteLocationAsync ( barcode );

                _logger.LogInformation ( "Location deleted: {Barcode}", barcode );
                return Ok ( new { Message = "Location deleted successfully" } );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { Error = ex.Message } );
            }
            catch ( InvalidOperationException ex ) // لو المكان فيه Assets
            {
                return BadRequest ( new { Error = ex.Message } ); // 400 Bad Request
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error deleting location: {Barcode}", barcode );
                return StatusCode ( 500, new { Error = "Internal error", Details = ex.Message } );
            }
        }
        #endregion
    }
}