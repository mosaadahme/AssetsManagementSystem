
using AssetsManagementSystem.DTOs.AssetTransferDTOs;
using AssetsManagementSystem.Services.AssetTransfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route ( "api/[controller]/[action]" )]
    [ApiController]
    [Authorize] 
    public class AssetTransferController : ControllerBase
    {
        private readonly AssetTransferService _transferService;
        private readonly ILogger<AssetTransferController> _logger;

        public AssetTransferController ( AssetTransferService transferService, ILogger<AssetTransferController> logger )
        {
            _transferService = transferService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize ( Roles = "Admin,Manager" )]
        public async Task<IActionResult> BulkRelocateAssets ( [FromBody] BulkRelocationRequestDTO dto ) 
        {
            try
            {
                var result = await _transferService.RelocateAssetsBulkAsync ( dto );
                return Ok ( result );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in BulkRelocateAssets" );
                return StatusCode ( 500, new { error = "Internal Server Error" } );
            }

        }



        // =================================================================================
        // 1. Direct Actions (Check-Out / Check-In / Relocate) - يتموا فوراً بواسطة الأدمن
        // =================================================================================

        #region 1. Assign (Check-Out from Stock)
        /// <summary>
        /// صرف أصل من المخزن لموظف (Admin/Manager Only)
        /// </summary>
        [HttpPost]
        [Authorize ( Roles = "Admin,Manager" )]
        public async Task<IActionResult> AssignToUser ( string barcode, Guid toUserId )
        {
            try
            {
                _logger.LogInformation ( "Assigning asset {Barcode} to user {UserId}", barcode, toUserId );
                var result = await _transferService.AssignAssetToUserAsync ( barcode, toUserId );
                return Ok ( result );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in AssignToUser" );
                return StatusCode ( 500, new { error = "Internal Server Error" } );
            }
        }
        #endregion

        #region 2. Return (Check-In to Stock)
        /// <summary>
        /// إرجاع أصل من موظف للمخزن (Admin/Manager Only)
        /// </summary>
        [HttpPost ( "{barcode}" )]
        [Authorize ( Roles = "Admin,Manager" )]
        public async Task<IActionResult> ReturnToStock ( string barcode )
        {
            try
            {
                _logger.LogInformation ( "Returning asset {Barcode} to stock", barcode );
                var result = await _transferService.ReturnAssetToStockAsync ( barcode );
                return Ok ( result );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in ReturnToStock" );
                return StatusCode ( 500, new { error = "Internal Server Error" } );
            }
        }
        #endregion

        #region 3. Relocate (Location to Location)
        /// <summary>
        /// نقل مكان الأصل (بدون تغيير الموظف)
        /// </summary>
        [HttpPost]
        [Authorize ( Roles = "Admin,Manager" )]
        public async Task<IActionResult> RelocateAsset ( [FromBody] LocationToLocationTransferDTO dto )
        {
            try
            {
                var result = await _transferService.RelocateAssetAsync ( dto );
                return Ok ( result );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in RelocateAsset" );
                return StatusCode ( 500, new { error = "Internal Server Error" } );
            }
        }
        #endregion

        // =================================================================================
        // 2. Handover Process (Transfer -> Approve/Reject) - دورة النقل بين الموظفين
        // =================================================================================

        #region 4. Request Transfer (Initiate)
        /// <summary>
        /// طلب نقل عهدة من موظف لآخر (الحالة بتكون Pending)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RequestTransfer ( [FromBody] UserToUserTransferDTO dto )
        {
            try
            {
                var result = await _transferService.TransferUserToUserAsync ( dto );
                return Ok ( new { message = "Transfer request sent successfully.", data = result } );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in RequestTransfer" );
                return StatusCode ( 500, new { error = ex.Message } );
            }
        }
        #endregion

        #region 5. Approve Transfer
        /// <summary>
        /// قبول استلام العهدة (يقوم به الموظف المستلم)
        /// </summary>
        [HttpPut ( "{transferId}" )]
        public async Task<IActionResult> ApproveTransfer ( int transferId )
        {
            try
            {
                await _transferService.ApproveTransferAsync ( transferId );
                return Ok ( new { message = "Transfer approved and asset assigned successfully." } );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex ) // لو يوزر غير المصرح له حاول يوافق
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in ApproveTransfer" );
                return StatusCode ( 500, new { error = "Internal Server Error" } );
            }
        }
        #endregion

        #region 6. Reject Transfer
        /// <summary>
        /// رفض استلام العهدة
        /// </summary>
        [HttpPut ( "{transferId}" )]
        public async Task<IActionResult> RejectTransfer ( int transferId, [FromQuery] string reason )
        {
            try
            {
                await _transferService.RejectTransferAsync ( transferId, reason );
                return Ok ( new { message = "Transfer request rejected." } );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error in RejectTransfer" );
                return StatusCode ( 500, new { error = "Internal Server Error" } );
            }
        }
        #endregion

        // =================================================================================
        // 3. Reporting & History
        // =================================================================================

        #region 7. Get My Pending Transfers
        /// <summary>
        /// عرض الطلبات المعلقة التي تحتاج موافقتي
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyPendingTransfers ( )
        {
            try
            {
                var result = await _transferService.GetMyPendingTransfersAsync ( );
                return Ok ( result );
            }
            catch ( Exception ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion

        #region 8. Get All Transfers (History)
        [HttpGet]
        [Authorize ( Roles = "Admin,Manager,Auditor" )]
        public async Task<IActionResult> GetAllTransfers ( )
        {
            try
            {
                var result = await _transferService.GetAllTransfersAsync ( );
                return Ok ( result );
            }
            catch ( Exception ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion

        #region 9. Get Transfer By ID
        [HttpGet ( "{id}" )]
        public async Task<IActionResult> GetTransferById ( int id )
        {
            try
            {
                var result = await _transferService.GetAssetTransferByIdAsync ( id );
                return Ok ( result );
            }
            catch ( KeyNotFoundException ex )
            {
                return NotFound ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                return BadRequest ( new { error = ex.Message } );
            }
        }
        #endregion
    }
}