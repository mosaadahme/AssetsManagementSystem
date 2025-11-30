using AssetsManagementSystem.DTOs.Printing;
using AssetsManagementSystem.Services.Printing;
using Microsoft.AspNetCore.Mvc;

namespace AssetsManagementSystem.Controllers
{
    [Route ( "api/[controller]/[action]" )]
    [ApiController]
    public class PrintingController : ControllerBase
    {
        private readonly PrintingService _printingService;

        public PrintingController ( PrintingService printingService )
        {
            _printingService = printingService;
        }

        //[HttpPost]
        //public IActionResult PrintLabels ( [FromBody] List<string> barcodes )
        //{
        //    if ( barcodes == null || !barcodes.Any ( ) )
        //        return BadRequest ( "No barcodes provided." );

        //    try
        //    {
        //        // 1. Generate PDF Bytes
        //        var pdfBytes = _printingService.GenerateBarcodeLabelsPdf ( barcodes );

        //        // 2. Return File to Browser
        //        string fileName = $"Barcodes_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

        //        // بنرجع الملف عشان المتصفح يفتحه أو يحمله
        //        return File ( pdfBytes, "application/pdf", fileName );
        //    }
        //    catch ( Exception ex )
        //    {
        //        // لو حصل أي خطأ في الرسم نرجعه
        //        return StatusCode ( 500, new { error = $"Error generating PDF: {ex.Message}" } );
        //    }
        //}

        [HttpPost]
        public IActionResult PrintLabels ( [FromBody] PrintRequestDTO request )
        {
            if ( request.Barcodes == null || !request.Barcodes.Any ( ) )
                return BadRequest ( "No barcodes provided." );

            try
            {
                // نمرر المقاسات للسيرفيس
                var pdfBytes = _printingService.GenerateBarcodeLabelsPdf (
                    request.Barcodes,
                    request.WidthCm,
                    request.HeightCm
                );

                string fileName = $"Labels_{request.WidthCm}x{request.HeightCm}_{DateTime.Now:HHmm}.pdf";
                return File ( pdfBytes, "application/pdf", fileName );
            }
            catch ( Exception ex )
            {
                return StatusCode ( 500, new { error = ex.Message } );
            }
        }
    }
}