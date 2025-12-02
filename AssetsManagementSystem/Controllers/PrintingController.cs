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

        // 1. طباعة A4 (ثابتة)
        [HttpPost]
        public IActionResult PrintA4 ( [FromBody] List<string> barcodes )
        {
            if ( barcodes == null || !barcodes.Any ( ) )
                return BadRequest ( "No barcodes provided." );

            try
            {
                var pdfBytes = _printingService.GenerateA4Pdf ( barcodes );
                return File ( pdfBytes, "application/pdf", $"A4_Labels_{DateTime.Now:HHmm}.pdf" );
            }
            catch ( Exception ex )
            {
                return StatusCode ( 500, new { error = ex.Message } );
            }
        }

        // 2. طباعة حرارية (ديناميكية - 5*5 أو 6*2.5)
        [HttpPost]
        public IActionResult PrintThermal ( [FromBody] PrintRequestDTO request )
        {
            if ( request.Barcodes == null || !request.Barcodes.Any ( ) )
                return BadRequest ( "No barcodes provided." );

            // Validation بسيط للمقاسات
            if ( request.WidthCm <= 0 || request.HeightCm <= 0 )
                return BadRequest ( "Width and Height must be greater than 0." );

            try
            {
                // هنا السحر: بنبعت المقاسات للسيرفيس
                var pdfBytes = _printingService.GenerateDynamicPdf (
                    request.Barcodes,
                    request.WidthCm,
                    request.HeightCm
                );

                string fileName = $"Thermal_{request.WidthCm}x{request.HeightCm}_{DateTime.Now:HHmm}.pdf";
                return File ( pdfBytes, "application/pdf", fileName );
            }
            catch ( Exception ex )
            {
                return StatusCode ( 500, new { error = ex.Message } );
            }
        }
    }
}