using AssetsManagementSystem.DTOs.LocationDTOs;
using AssetsManagementSystem.Services.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        // الروت هيكون: api/Location/AddLocation
        [HttpPost]
        // [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddLocation ( [FromBody] AddLocationRequestDTO addLocationRequest )
        {
            if ( !ModelState.IsValid )
            {
                _logger.LogWarning ( "Invalid model state for AddLocation request" );
                return BadRequest ( ModelState );
            }

            try
            {
                await _locationService.AddLocationAsync ( addLocationRequest );
                _logger.LogInformation ( $"Location '{addLocationRequest.Name}' added successfully." );

                return StatusCode ( StatusCodes.Status201Created, new { Message = "Location added successfully" } );
            }
            catch ( InvalidOperationException ex ) // تكرار الاسم أو الباركود
            {
                _logger.LogWarning ( ex, "Duplicate location attempt." );
                return Conflict ( new { Error = ex.Message } );
            }
            catch ( KeyNotFoundException ex ) // لو الـ Parent اللي مبعوت مش موجود
            {
                return NotFound ( new { Error = ex.Message } );
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
        // الروت: api/Location/GetAllLocations
        [HttpGet]
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
        // الروت: api/Location/GetLocationsByPagination?currentPage=1&pageSize=10
        [HttpGet]
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
            catch ( InvalidOperationException ex ) // لو المكان فيه Assets أو جواه Child Locations
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

        // =========================================================================
        // الدـــوال الـجـــديـــدة (New Hierarchy & Search Endpoints)
        // =========================================================================

        #region Get Location Levels (الليفيلز)
        // الروت: api/Location/GetLevels
        [HttpGet]
        public IActionResult GetLevels ( )
        {
            try
            {
                var levels = _locationService.GetLocationLevels ( );
                return Ok ( levels );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error retrieving location levels." );
                return StatusCode ( 500, new { Error = "Internal error", Details = ex.Message } );
            }
        }
        #endregion

        #region Get Locations By Parent (التتابع / Cascading)
        // الروت: api/Location/GetLocationsByParent?parentId=1
        [HttpGet]
        public async Task<IActionResult> GetLocationsByParent ( [FromQuery] int? parentId )
        {
            try
            {
                var locations = await _locationService.GetLocationsByParentAsync ( parentId );
                return Ok ( locations );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, $"Error retrieving locations by parent id: {parentId}" );
                return BadRequest ( new { Error = ex.Message } );
            }
        }
        #endregion

        #region Get Location Breadcrumbs (المسار العكسي)
        // الروت: api/Location/GetLocationBreadcrumbs/50
        [HttpGet ( "{id}" )]
        public async Task<IActionResult> GetLocationBreadcrumbs ( int id )
        {
            if ( id <= 0 ) return BadRequest ( new { Error = "Valid Location ID is required" } );

            try
            {
                var breadcrumbs = await _locationService.GetLocationBreadcrumbsAsync ( id );
                return Ok ( breadcrumbs );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, $"Error retrieving breadcrumbs for location id: {id}" );
                return BadRequest ( new { Error = ex.Message } );
            }
        }
        #endregion

        #region Search Locations (البحث السريع)
        // الروت: api/Location/SearchLocations?query=IT
        [HttpGet]
        public async Task<IActionResult> SearchLocations ( [FromQuery] string query )
        {
            if ( string.IsNullOrWhiteSpace ( query ) )
            {
                return BadRequest ( new { Error = "Search query cannot be empty" } );
            }

            try
            {
                var locations = await _locationService.SearchLocationsAsync ( query );
                return Ok ( locations );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, $"Error searching locations with query: {query}" );
                return BadRequest ( new { Error = ex.Message } );
            }
        }
        #endregion
    }
}