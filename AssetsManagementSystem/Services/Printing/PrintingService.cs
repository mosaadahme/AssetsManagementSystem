 
using BarcodeStandard;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp; // <-- المكتبة الجديدة للصور

namespace AssetsManagementSystem.Services.Printing
{
    public class PrintingService
    {
        public PrintingService ( )
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte [] GenerateBarcodeLabelsPdf ( List<string> barcodes )
        {
            var document = QuestPDF.Fluent.Document.Create ( container =>
            {
                container.Page ( page =>
                {
                    page.Size ( PageSizes.A4 );
                    page.Margin ( 1, Unit.Centimetre );
                    page.PageColor ( Colors.White );
                    page.DefaultTextStyle ( x => x.FontSize ( 10 ) );

                    page.Content ( )
                        .PaddingVertical ( 0.5f, Unit.Centimetre )
                        .Column ( column =>
                        {
                            column.Item ( ).Grid ( grid =>
                            {
                                grid.Columns ( 3 );
                                grid.Spacing ( 15 );

                                foreach ( var code in barcodes )
                                {
                                    grid.Item ( ).Element ( e => DrawSingleLabel ( e, code ) );
                                }
                            } );
                        } );
                } );
            } );

            return document.GeneratePdf ( );
        }

        private void DrawSingleLabel ( QuestPDF.Infrastructure.IContainer container, string barcodeText )
        {
            // 1. إعداد الباركود
            var barcode = new Barcode ( );
            barcode.IncludeLabel = false;
            barcode.Alignment = AlignmentPositions.Center;

            // 2. توليد الصورة باستخدام SkiaSharp (المكتبة الحديثة)
            // استخدمنا SKColors بدل Color عشان نمنع التعارض
            var img = barcode.Encode ( BarcodeStandard.Type.Code128, barcodeText, SKColors.Black, SKColors.White, 290, 120 );

            // 3. تحويل الصورة لـ Byte Array (الطريقة الجديدة)
            using var encoded = img.Encode ( SKEncodedImageFormat.Png, 100 );
            var imageBytes = encoded.ToArray ( );

            // 4. رسم الاستيكر
            container
                .Border ( 1 )
                .BorderColor ( Colors.Grey.Lighten1 )
                .Height ( 3.5f, Unit.Centimetre )
                .Column ( col =>
                {
                    col.Item ( ).AlignCenter ( ).Text ( "Asset Management Sys" ).FontSize ( 7 ).FontColor ( Colors.Grey.Darken2 );

                    col.Item ( )
                       .Height ( 1.8f, Unit.Centimetre )
                       .AlignCenter ( )
                       .AlignMiddle ( )
                       .Image ( imageBytes )
                       .FitArea ( );

                    col.Item ( ).AlignCenter ( ).Text ( barcodeText ).FontSize ( 10 ).Bold ( );
                } );
        }
    }
}