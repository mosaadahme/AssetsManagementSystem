//using AssetsManagementSystem.DTOs.AssetDTOs;
//using AssetsManagementSystem.DTOs.AssetSearchDTOs;
//using AssetsManagementSystem.Services.Report;
//using AssetsManagementSystem.Services.Report;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace AssetsManagementSystem.Controllers
//{
//    [ApiController]
//    [Route ( "api/[controller]" )]
//    //[Authorize]
//    public class ReportController : ControllerBase
//    {
//        private readonly ReportService _reportService;

//        public ReportController ( ReportService reportService )
//        {
//            _reportService = reportService;
//        }

//        #region Simple Search
//        /// <summary>
//        /// Search for assets using simple criteria
//        /// POST: api/Report/search
//        /// </summary>
//        [HttpPost ( "search" )]
//        [ProducesResponseType ( typeof ( IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> ), StatusCodes.Status200OK )]
//        [ProducesResponseType ( StatusCodes.Status400BadRequest )]
//        public async Task<IActionResult> SearchAssets ( [FromBody] AssetSearchCriteria criteria )
//        {
//            try
//            {
//                var assets = await _reportService.SearchAssetsAsync ( criteria );
//                return Ok ( new
//                {
//                    success = true,
//                    data = assets,
//                    count = assets.Count ( )
//                } );
//            }
//            catch ( ArgumentNullException ex )
//            {
//                return BadRequest ( new { success = false, message = ex.Message } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion

//        #region Advanced Search
//        /// <summary>
//        /// Advanced search with multiple filters and pagination
//        /// POST: api/Report/advanced-search?currentPage=1&pageSize=10
//        /// </summary>
//        [HttpPost ( "advanced-search" )]
//        [ProducesResponseType ( typeof ( object ), StatusCodes.Status200OK )]
//        [ProducesResponseType ( StatusCodes.Status400BadRequest )]
//        public async Task<IActionResult> AdvancedSearchAssets (
//            [FromBody] AdvancedAssetSearchCriteria criteria,
//            [FromQuery] int currentPage = 1,
//            [FromQuery] int pageSize = 10 )
//        {
//            try
//            {
//                var (assets, totalCount) = await _reportService.AdvancedSearchAssetsAsync ( criteria, currentPage, pageSize );

//                return Ok ( new
//                {
//                    success = true,
//                    data = assets,
//                    pagination = new
//                    {
//                        currentPage,
//                        pageSize,
//                        totalCount,
//                        totalPages = (int) Math.Ceiling ( totalCount / (double) pageSize )
//                    }
//                } );
//            }
//            catch ( ArgumentNullException ex )
//            {
//                return BadRequest ( new { success = false, message = ex.Message } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion

//        #region Status Report
//        /// <summary>
//        /// Get assets by status
//        /// GET: api/Report/by-status/{status}
//        /// </summary>
//        [HttpGet ( "by-status/{status}" )]
//        [ProducesResponseType ( typeof ( IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> ), StatusCodes.Status200OK )]
//        [ProducesResponseType ( StatusCodes.Status400BadRequest )]
//        public async Task<IActionResult> GetAssetsByStatus ( string status )
//        {
//            try
//            {
//                var assets = await _reportService.GetAssetsByStatusReportAsync ( status );
//                return Ok ( new
//                {
//                    success = true,
//                    data = assets,
//                    count = assets.Count ( )
//                } );
//            }
//            catch ( ArgumentException ex )
//            {
//                return BadRequest ( new { success = false, message = ex.Message } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion

//        #region Low Stock Report
//        /// <summary>
//        /// Get assets with low stock (quantity <= minimum limit)
//        /// GET: api/Report/low-stock
//        /// </summary>
//        [HttpGet ( "low-stock" )]
//        [ProducesResponseType ( typeof ( IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> ), StatusCodes.Status200OK )]
//        public async Task<IActionResult> GetLowStockAssets ( )
//        {
//            try
//            {
//                var assets = await _reportService.GetLowStockAssetsReportAsync ( );
//                return Ok ( new
//                {
//                    success = true,
//                    data = assets,
//                    count = assets.Count ( )
//                } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion

//        #region Expired Warranty Report
//        /// <summary>
//        /// Get assets with expired warranties
//        /// GET: api/Report/expired-warranty
//        /// </summary>
//        [HttpGet ( "expired-warranty" )]
//        [ProducesResponseType ( typeof ( IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> ), StatusCodes.Status200OK )]
//        public async Task<IActionResult> GetExpiredWarrantyAssets ( )
//        {
//            try
//            {
//                var assets = await _reportService.GetExpiredWarrantyAssetsReportAsync ( );
//                return Ok ( new
//                {
//                    success = true,
//                    data = assets,
//                    count = assets.Count ( )
//                } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion

//        #region Category Report
//        /// <summary>
//        /// Get all assets in a specific category
//        /// GET: api/Report/by-category/{categoryId}
//        /// </summary>
//        [HttpGet ( "by-category/{categoryId}" )]
//        [ProducesResponseType ( typeof ( IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> ), StatusCodes.Status200OK )]
//        [ProducesResponseType ( StatusCodes.Status400BadRequest )]
//        public async Task<IActionResult> GetAssetsByCategory ( int categoryId )
//        {
//            try
//            {
//                var assets = await _reportService.GetAssetsByCategoryReportAsync ( categoryId );
//                return Ok ( new
//                {
//                    success = true,
//                    data = assets,
//                    count = assets.Count ( )
//                } );
//            }
//            catch ( ArgumentException ex )
//            {
//                return BadRequest ( new { success = false, message = ex.Message } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion

//        #region Location Report
//        /// <summary>
//        /// Get all assets in a specific location
//        /// GET: api/Report/by-location/{locationId}
//        /// </summary>
//        [HttpGet ( "by-location/{locationId}" )]
//        [ProducesResponseType ( typeof ( IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> ), StatusCodes.Status200OK )]
//        [ProducesResponseType ( StatusCodes.Status400BadRequest )]
//        public async Task<IActionResult> GetAssetsByLocation ( int locationId )
//        {
//            try
//            {
//                var assets = await _reportService.GetAssetsByLocationReportAsync ( locationId );
//                return Ok ( new
//                {
//                    success = true,
//                    data = assets,
//                    count = assets.Count ( )
//                } );
//            }
//            catch ( ArgumentException ex )
//            {
//                return BadRequest ( new { success = false, message = ex.Message } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion

//        #region User Assets Report
//        /// <summary>
//        /// Get all assets assigned to a specific user
//        /// GET: api/Report/by-user/{userId}
//        /// </summary>
//        [HttpGet ( "by-user/{userId}" )]
//        [ProducesResponseType ( typeof ( IEnumerable<DTOs.AssetSearchDTOs.GetAssetResponseDTO> ), StatusCodes.Status200OK )]
//        [ProducesResponseType ( StatusCodes.Status400BadRequest )]
//        public async Task<IActionResult> GetAssetsByUser ( Guid userId )
//        {
//            try
//            {
//                var assets = await _reportService.GetAssetsByUserReportAsync ( userId );
//                return Ok ( new
//                {
//                    success = true,
//                    data = assets,
//                    count = assets.Count ( )
//                } );
//            }
//            catch ( ArgumentException ex )
//            {
//                return BadRequest ( new { success = false, message = ex.Message } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion

//        #region Summary Report
//        /// <summary>
//        /// Get summary statistics for all assets
//        /// GET: api/Report/summary
//        /// </summary>
//        [HttpGet ( "summary" )]
//        [ProducesResponseType ( typeof ( AssetSummaryReportDTO ), StatusCodes.Status200OK )]
//        public async Task<IActionResult> GetAssetsSummary ( )
//        {
//            try
//            {
//                var summary = await _reportService.GetAssetsSummaryReportAsync ( );
//                return Ok ( new
//                {
//                    success = true,
//                    data = summary
//                } );
//            }
//            catch ( Exception ex )
//            {
//                return StatusCode ( 500, new { success = false, message = ex.Message } );
//            }
//        }
//        #endregion
//    }
//}