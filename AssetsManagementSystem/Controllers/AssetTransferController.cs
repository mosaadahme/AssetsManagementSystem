
//using AssetsManagementSystem.DTOs.AssetTransferDTOs;
//using AssetsManagementSystem.Services.AssetTransfer;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace AssetsManagementSystem.Controllers
//{
//    [Route("api/[controller]/[action]")]
//    [ApiController]
//    public class AssetTransferController : ControllerBase
//    {
//        private readonly AssetTransferService _assetTransferService;
//        private readonly ILogger<AssetTransferController> _logger;

//        public AssetTransferController(AssetTransferService assetTransferService, ILogger<AssetTransferController> logger)
//        {
//            _assetTransferService = assetTransferService;
//            _logger = logger;
//        }

//        #region Transfer Location to Location
//        [HttpPost]
//        //[Authorize(Roles = "Admin,Manager")]

//        public async Task<IActionResult> TransferLocationToLocation([FromBody] LocationToLocationTransferDTO dto)
//        {
//            if (!ModelState.IsValid)
//            {
//                _logger.LogWarning("Invalid model state for TransferLocationToLocation.");
//                return BadRequest(ModelState);
//            }

//            try
//            {
//                var result = await _assetTransferService.TransferLocationToLocationAsync(dto);
//                _logger.LogInformation("Asset transfer (location to location) added successfully.");
//                return CreatedAtAction(nameof(GetAssetTransferById), new { id = result.Id }, result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while transferring asset (location to location).");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Transfer User to User
//        [HttpPost]
//        [Authorize(Roles = "Admin,Manager")]

//        public async Task<IActionResult> TransferUserToUser([FromBody] UserToUserTransferDTO dto)
//        {
//            if (!ModelState.IsValid)
//            {
//                _logger.LogWarning("Invalid model state for TransferUserToUser.");
//                return BadRequest(ModelState);
//            }

//            try
//            {
//                var result = await _assetTransferService.TransferUserToUserAsync(dto);
//                _logger.LogInformation("Asset transfer (user to user) added successfully.");
//                return CreatedAtAction(nameof(GetAssetTransferById), new { id = result.Id }, result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while transferring asset (user to user).");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Transfer User and Location
//        [HttpPost]
//        [Authorize(Roles = "Admin,Manager")]

//        public async Task<IActionResult> TransferUserAndLocation([FromBody] UserAndLocationTransferDTO dto)
//        {
//            if (!ModelState.IsValid)
//            {
//                _logger.LogWarning("Invalid model state for TransferUserAndLocation.");
//                return BadRequest(ModelState);
//            }

//            try
//            {
//                var result = await _assetTransferService.TransferUserAndLocationAsync(dto);
//                _logger.LogInformation("Asset transfer (user and location) added successfully.");
//                return CreatedAtAction(nameof(GetAssetTransferById), new { id = result.Id }, result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while transferring asset (user and location).");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Approve Transfer
//        [HttpPost("{id}/approve")]
//        [Authorize(Roles = "User,Manager")]

//        public async Task<IActionResult> ApproveTransfer(int id)
//        {
//            try
//            {
//                await _assetTransferService.ApproveTransferAsync(id);
//                _logger.LogInformation($"Asset transfer {id} approved successfully.");
//                return Ok("Transfer approved successfully.");
//            }
//            catch (KeyNotFoundException ex)
//            {
//                _logger.LogWarning(ex, $"Asset transfer {id} not found.");
//                return NotFound(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error occurred while approving asset transfer {id}.");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Reject Transfer
//        [HttpPost("{id}/reject")]
//        //[Authorize(Roles = "Admin,Manager")]

//        public async Task<IActionResult> RejectTransfer(int id)//, [FromBody] string rejectionReason)
//        {
//            if (string.IsNullOrWhiteSpace("rejectionReason"))
//            {
//                _logger.LogWarning("Rejection reason is required.");
//                return BadRequest("Rejection reason is required.");
//            }

//            try
//            {
//                await _assetTransferService.RejectTransferAsync(id, "rejectionReason");
//                _logger.LogInformation($"Asset transfer {id} rejected successfully.");
//                return Ok("Transfer rejected successfully.");
//            }
//            catch (KeyNotFoundException ex)
//            {
//                _logger.LogWarning(ex, $"Asset transfer {id} not found.");
//                return NotFound(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error occurred while rejecting asset transfer {id}.");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Update Location to Location Transfer
//        [HttpGet("GetAssetForCurrentUser")]
//        [Authorize(Roles = "User,Manager")]

//        public async Task<IActionResult> GetAssetForCurrentUser()
//        {
//            try
//            {
//                var result = await _assetTransferService.GetAssetTransferForCurrentUserByIdAsync();
//                _logger.LogInformation("All asset transfers retrieved successfully.");
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while retrieving all asset transfers.");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Update User to User Transfer
//        [HttpPut("{id}/usertouser")]
//        [Authorize(Roles = "Admin,Manager")]

//        public async Task<IActionResult> UpdateUserToUserTransfer(int id, [FromBody] UpdateUserToUserTransferDTO dto)
//        {
//            if (!ModelState.IsValid)
//            {
//                _logger.LogWarning("Invalid model state for UpdateUserToUserTransfer.");
//                return BadRequest(ModelState);
//            }

//            try
//            {
//                var result = await _assetTransferService.UpdateUserToUserTransferAsync(id, dto);
//                _logger.LogInformation("Asset transfer (user to user) updated successfully.");
//                return Ok(result);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                _logger.LogWarning(ex, "Transfer record not found.");
//                return NotFound(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while updating asset transfer (user to user).");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Update User and Location Transfer
//        [HttpPut("{id}/userandlocation")]
//        [Authorize(Roles = "Admin,Manager")]

//        public async Task<IActionResult> UpdateUserAndLocationTransfer(int id, [FromBody] UpdateUserAndLocationTransferDTO dto)
//        {
//            if (!ModelState.IsValid)
//            {
//                _logger.LogWarning("Invalid model state for UpdateUserAndLocationTransfer.");
//                return BadRequest(ModelState);
//            }

//            try
//            {
//                var result = await _assetTransferService.UpdateUserAndLocationTransferAsync(id, dto);
//                _logger.LogInformation("Asset transfer (user and location) updated successfully.");
//                return Ok(result);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                _logger.LogWarning(ex, "Transfer record not found.");
//                return NotFound(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while updating asset transfer (user and location).");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Delete Asset Transfer
//        [HttpDelete("{id}")]
//        [Authorize(Roles = "Admin,Manager")]

//        public async Task<IActionResult> DeleteAssetTransfer(int id)
//        {
//            try
//            {
//                await _assetTransferService.DeleteAssetTransferAsync(id);
//                _logger.LogInformation($"Asset transfer {id} deleted successfully.");
//                return NoContent();
//            }
//            catch (KeyNotFoundException ex)
//            {
//                _logger.LogWarning(ex, $"Asset transfer {id} not found.");
//                return NotFound(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error occurred while deleting asset transfer {id}.");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Get Asset Transfer by ID
//        [HttpGet("{id}")]
//        [Authorize(Roles = "Admin,Manager,Auditor")]

//        public async Task<IActionResult> GetAssetTransferById(int id)
//        {
//            try
//            {
//                var result = await _assetTransferService.GetAssetTransferByIdAsync(id);
//                _logger.LogInformation($"Asset transfer {id} retrieved successfully.");
//                return Ok(result);
//            }
//            catch (KeyNotFoundException ex)
//            {
//                _logger.LogWarning(ex, $"Asset transfer {id} not found.");
//                return NotFound(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error occurred while retrieving asset transfer {id}.");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Get All Asset Transfers
//        [HttpGet]
//        [Authorize(Roles = "Admin,Manager,Auditor")]

//        public async Task<IActionResult> GetAllAssetTransfers()
//        {
//            try
//            {
//                var result = await _assetTransferService.GetAllAssetTransfersAsync();
//                _logger.LogInformation("All asset transfers retrieved successfully.");
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while retrieving all asset transfers.");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion

//        #region Get By Pagination Asset Transfers
//        [HttpGet]
//        [Authorize(Roles = "Admin,Manager,Auditor")]

//        public async Task<IActionResult> GetAssetTransfersByPagination(int currentPage, int pageSize)
//        {
//            try
//            {
//                var result = await _assetTransferService.GetAllByPaginationAssetTransfersAsync(currentPage, pageSize);
//                _logger.LogInformation("All asset transfers retrieved successfully.");
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while retrieving all asset transfers.");
//                return StatusCode(500, "Internal server error.");
//            }
//        }
//        #endregion
//    }
//}

using AssetsManagementSystem.DTOs.AssetTransferDTOs;
using AssetsManagementSystem.Services.AssetTransfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route ( "api/[controller]/[action]" )]
    [ApiController]
    [Authorize] // كل العمليات محتاجة لوجين
    public class AssetTransferController : ControllerBase
    {
        private readonly AssetTransferService _transferService;
        private readonly ILogger<AssetTransferController> _logger;

        public AssetTransferController ( AssetTransferService transferService, ILogger<AssetTransferController> logger )
        {
            _transferService = transferService;
            _logger = logger;
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