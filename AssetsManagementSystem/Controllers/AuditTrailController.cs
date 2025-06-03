using AssetsManagementSystem.Services.Categories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditTrailController :  ControllerBase
    {
        private readonly AuditTrailService AuditTrailService;
        public AuditTrailController(AuditTrailService auditTrailService)
        {
            AuditTrailService = auditTrailService;
        }
        #region GetAllAuditTrail
        [HttpGet("GetAuditTrails")]
        // [Authorize(Roles = "Admin,Manager,Auditor")]

        public async Task<IActionResult> GetAuditTrails()
        {
            try
            {
                var categories = await AuditTrailService.GetAuditTrails();
                
                return Ok(categories);
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, new { Error = "An error occurred while retrieving the Audit", Details = ex.Message });
            }
        }
        #endregion
    }
}
