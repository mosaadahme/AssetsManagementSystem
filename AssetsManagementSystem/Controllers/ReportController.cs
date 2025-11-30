using AssetsManagementSystem.DTOs.AssetSearchDTOs;
using AssetsManagementSystem.Services.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [ApiController]
    [Route ( "api/[controller]" )]
    // [Authorize(Roles = "Admin,Manager,Auditor")] // فعلها وقت البرودكشن
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController ( ReportService reportService, ILogger<ReportController> logger )
        {
            _reportService = reportService;
            _logger = logger;
        }

        #region Simple Search
        /// <summary>
        /// Search for assets using simple criteria (Search Term)
        /// </summary>
        [HttpPost ( "search" )]
        public async Task<IActionResult> SearchAssets ( [FromBody] AssetSearchCriteria criteria )
        {
            try
            {
                var assets = await _reportService.SearchAssetsAsync ( criteria );
                return Ok ( new
                {
                    success = true,
                    count = assets.Count ( ),
                    data = assets
                } );
            }
            catch ( ArgumentNullException ex )
            {
                return BadRequest ( new { success = false, message = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in simple search" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion

        #region Advanced Search
        /// <summary>
        /// Advanced search with multiple filters and pagination
        /// </summary>
        [HttpPost ( "advanced-search" )]
        public async Task<IActionResult> AdvancedSearchAssets (
            [FromBody] AdvancedAssetSearchCriteria criteria,
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 10 )
        {
            try
            {
                var (assets, totalCount) = await _reportService.AdvancedSearchAssetsAsync ( criteria, currentPage, pageSize );

                return Ok ( new
                {
                    success = true,
                    pagination = new
                    {
                        currentPage,
                        pageSize,
                        totalCount,
                        totalPages = (int) Math.Ceiling ( totalCount / (double) pageSize )
                    },
                    data = assets
                } );
            }
            catch ( ArgumentNullException ex )
            {
                return BadRequest ( new { success = false, message = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in advanced search" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion

        #region Status Report
        [HttpGet ( "by-status/{status}" )]
        public async Task<IActionResult> GetAssetsByStatus ( string status )
        {
            try
            {
                var assets = await _reportService.GetAssetsByStatusReportAsync ( status );
                return Ok ( new { success = true, count = assets.Count ( ), data = assets } );
            }
            catch ( ArgumentException ex )
            {
                return BadRequest ( new { success = false, message = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in status report" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion

        #region Low Stock Report
        [HttpGet ( "low-stock" )]
        public async Task<IActionResult> GetLowStockAssets ( )
        {
            try
            {
                var assets = await _reportService.GetLowStockAssetsReportAsync ( );
                return Ok ( new { success = true, count = assets.Count ( ), data = assets } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in low stock report" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion

        #region Expired Warranty Report
        [HttpGet ( "expired-warranty" )]
        public async Task<IActionResult> GetExpiredWarrantyAssets ( )
        {
            try
            {
                var assets = await _reportService.GetExpiredWarrantyAssetsReportAsync ( );
                return Ok ( new { success = true, count = assets.Count ( ), data = assets } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in expired warranty report" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion

        #region Category Report
        [HttpGet ( "by-category/{categoryId}" )]
        public async Task<IActionResult> GetAssetsByCategory ( int categoryId )
        {
            try
            {
                var assets = await _reportService.GetAssetsByCategoryReportAsync ( categoryId );
                return Ok ( new { success = true, count = assets.Count ( ), data = assets } );
            }
            catch ( ArgumentException ex )
            {
                return BadRequest ( new { success = false, message = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in category report" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion

        #region Location Report
        [HttpGet ( "by-location/{locationId}" )]
        public async Task<IActionResult> GetAssetsByLocation ( int locationId )
        {
            try
            {
                var assets = await _reportService.GetAssetsByLocationReportAsync ( locationId );
                return Ok ( new { success = true, count = assets.Count ( ), data = assets } );
            }
            catch ( ArgumentException ex )
            {
                return BadRequest ( new { success = false, message = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in location report" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion

        #region User Assets Report
        [HttpGet ( "by-user/{userId}" )]
        public async Task<IActionResult> GetAssetsByUser ( Guid userId )
        {
            try
            {
                var assets = await _reportService.GetAssetsByUserReportAsync ( userId );
                return Ok ( new { success = true, count = assets.Count ( ), data = assets } );
            }
            catch ( ArgumentException ex )
            {
                return BadRequest ( new { success = false, message = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in user assets report" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion

        #region Summary Report (Dashboard)
        [HttpGet ( "summary" )]
        public async Task<IActionResult> GetAssetsSummary ( )
        {
            try
            {
                var summary = await _reportService.GetAssetsSummaryReportAsync ( );
                return Ok ( new { success = true, data = summary } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in summary report" );
                return StatusCode ( 500, new { success = false, message = "Internal Server Error" } );
            }
        }
        #endregion
    }
}