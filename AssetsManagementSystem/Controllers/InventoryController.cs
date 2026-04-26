using AssetsManagementSystem.DTOs.InventoryDTOs;
using AssetsManagementSystem.Services.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetsManagementSystem.Controllers
{
    [Route ( "api/[controller]/[action]" )]
    [ApiController]
   
    //[Authorize ( Roles = "Admin,Manager,Auditor" )]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController ( InventoryService inventoryService, ILogger<InventoryController> logger )
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        #region 1. Start Audit Session
        /// <summary>
        /// البدء في جلسة جرد جديدة لمكان معين
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> StartAudit ( [FromBody] StartAuditRequestDTO dto )
        {
            try
            {
                // 1. الحصول على ID الموظف الحالي من التوكن
                var userIdString = User.FindFirstValue ( ClaimTypes.NameIdentifier );
                if ( !Guid.TryParse ( userIdString, out Guid auditorId ) )
                {
                    return Unauthorized ( new { error = "Invalid User ID in Token." } );
                }

                _logger.LogInformation ( "User {UserId} starting audit for Location {LocationId}", auditorId, dto.LocationId );

                // 2. استدعاء السيرفيس
                var auditId = await _inventoryService.StartAuditAsync ( dto, auditorId );

                // 3. الرد برقم الجلسة
                return StatusCode ( StatusCodes.Status201Created, new
                {
                    message = "Audit session started successfully.",
                    auditId = auditId
                } );
            }
            catch ( KeyNotFoundException ex ) // لو المكان مش موجود
            {
                _logger.LogWarning ( ex, "StartAudit failed: {Message}", ex.Message );
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex ) // لو فيه جلسة مفتوحة بالفعل
            {
                _logger.LogWarning ( ex, "StartAudit failed: {Message}", ex.Message );
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex ) // أي خطأ تاني
            {
                _logger.LogError ( ex, "Unexpected error in StartAudit." );
                return StatusCode ( 500, new { error = "An internal error occurred." } );
            }
        }
        #endregion

        #region 2. Submit & Analyze Audit
        /// <summary>
        /// إرسال الباركودات والحصول على تقرير العجز والزيادة
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitAudit ( [FromBody] SubmitAuditRequestDTO dto )
        {
            try
            {
                _logger.LogInformation ( "Submitting audit {AuditId} with {Count} scanned items.", dto.AuditId, dto.ScannedBarcodes.Count );

                // 1. استدعاء السيرفيس للتحليل
                var report = await _inventoryService.SubmitAuditAsync ( dto );

                // 2. إرجاع التقرير
                return Ok ( report );
            }
            catch ( KeyNotFoundException ex ) // لو رقم الجلسة غلط
            {
                _logger.LogWarning ( ex, "SubmitAudit failed: {Message}", ex.Message );
                return NotFound ( new { error = ex.Message } );
            }
            catch ( InvalidOperationException ex ) // لو الجلسة مقفولة أصلاً
            {
                _logger.LogWarning ( ex, "SubmitAudit failed: {Message}", ex.Message );
                return BadRequest ( new { error = ex.Message } );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Unexpected error in SubmitAudit for AuditId {AuditId}", dto.AuditId );
                return StatusCode ( 500, new { error = "An internal error occurred while processing the audit report." } );
            }
        }
        #endregion

        #region 3. Get Audit History (Search)
        
        [HttpGet ( "history" )] // URL: api/Inventory/GetAuditHistory/history?...
        public async Task<IActionResult> GetAuditHistory ( [FromQuery] AuditSearchFilterDTO filter )
        {
            try
            {
                var history = await _inventoryService.GetAuditHistoryAsync ( filter );
                return Ok ( history );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error retrieving audit history." );
                return StatusCode ( 500, new { error = "An internal error occurred." } );
            }
        }
        #endregion

        #region 3. Get Audit History (Search)

        [HttpGet ( "Detailedhistory" )] 
        public async Task<IActionResult> GetAuditDetails ( [FromQuery] int auditId )
        {
            try
            {
                var history = await _inventoryService.GetAuditDetailsAsync ( auditId );
                return Ok ( history );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "Error retrieving audit history." );
                return StatusCode ( 500, new { error = "An internal error occurred." } );
            }
        }
        #endregion
    }
}