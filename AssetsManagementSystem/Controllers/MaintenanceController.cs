using AssetsManagementSystem.DTOs.AssetMaintenanceDTOs.AssetMaintanceTempDTOs;
using AssetsManagementSystem.Models.Enums;
using AssetsManagementSystem.Services.Maintenance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route ( "api/[controller]" )]
    [ApiController]
    // [Authorize] // يفضل تفعيله بناءً على نظام الصلاحيات عندك
    public class MaintenanceController : ControllerBase
    {
        private readonly MaintenanceService _maintenanceService;

        public MaintenanceController ( MaintenanceService maintenanceService )
        {
            _maintenanceService = maintenanceService;
        }

        #region 1. Preventive Maintenance (Time-Based)

        /// <summary>
        /// إنشاء خطة صيانة دورية (Setup Phase)
        /// </summary>
        [HttpPost ( "plans" )]
        public async Task<IActionResult> CreateMaintenancePlan ( [FromBody] CreateMaintenancePlanRequestDTO dto )
        {
            if ( !ModelState.IsValid ) return BadRequest ( ModelState );

            var planId = await _maintenanceService.CreateMaintenancePlanAsync ( dto );
            return Ok ( new { Message = "Maintenance plan created successfully", PlanId = planId } );
        }

        /// <summary>
        /// تسجيل تنفيذ صيانة مجدولة (Execution Phase)
        /// </summary>
        [HttpPost ( "schedules/execute" )]
        public async Task<IActionResult> ExecuteMaintenance ( [FromBody] ExecuteMaintenanceDTO dto )
        {
            if ( !ModelState.IsValid ) return BadRequest ( ModelState );

            await _maintenanceService.ExecutePreventiveAsync ( dto );
            return Ok ( new { Message = $"Maintenance status updated to {dto.Status}" } );
        }

        #endregion

        #region 2. Corrective Maintenance (Breakdown)

        /// <summary>
        /// تقديم بلاغ عطل جديد من قبل موظف (Request Phase)
        /// </summary>
        [HttpPost ( "requests/submit" )]
        public async Task<IActionResult> SubmitRepairRequest ( [FromBody] SubmitRepairRequestDTO dto )
        {
            if ( !ModelState.IsValid ) return BadRequest ( ModelState );

            var requestId = await _maintenanceService.SubmitRepairRequestAsync ( dto );
            return Ok ( new { Message = "Repair request submitted successfully", RequestId = requestId } );
        }

        /// <summary>
        /// مراجعة الطلب من قبل مسؤول الصيانة (Review/Triage Phase)
        /// </summary>
        [HttpPut ( "requests/review" )]
        public async Task<IActionResult> ReviewRequest ( [FromBody] ReviewRequestDTO dto )
        {
            if ( !ModelState.IsValid ) return BadRequest ( ModelState );

            await _maintenanceService.ReviewRequestAsync ( dto );
            return Ok ( new { Message = $"Request {dto.RequestId} has been {dto.Decision}" } );
        }

        /// <summary>
        /// إغلاق طلب الإصلاح بعد التنفيذ (Closure Phase)
        /// </summary>
        [HttpPut ( "requests/close" )]
        public async Task<IActionResult> CloseRepairRequest ( [FromBody] CloseRepairDTO dto )
        {
            if ( !ModelState.IsValid ) return BadRequest ( ModelState );

            await _maintenanceService.CloseRepairRequestAsync ( dto );
            return Ok ( new { Message = "Repair request closed and asset status updated" } );
        }

        #endregion

        #region Getters (Optional but Helpful)

        // يمكنك إضافة Endpoints هنا لجلب جداول الصيانة المتأخرة 
        // أو الطلبات المعلقة بناءً على احتياج الـ Frontend

        #endregion
    }
}