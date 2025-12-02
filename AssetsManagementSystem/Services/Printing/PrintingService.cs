using BarcodeLib;
using BarcodeStandard;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace AssetsManagementSystem.Services.Printing
{
    public class PrintingService
    {
        public PrintingService ( )
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region 1. Dynamic Printing (Thermal Printers - Zebra/Xprinter)
        /// <summary>
        /// Print on roll (variable sizes like 5x5 or 6x2.5)
        /// </summary>
        public byte [] GenerateDynamicPdf ( List<string> barcodes, float widthCm, float heightCm )
        {
            var document = QuestPDF.Fluent.Document.Create ( container =>
            {
                container.Page ( page =>
                {
                    // Set precise page size
                    page.Size ( new PageSize ( widthCm, heightCm, Unit.Centimetre ) );

                    // Minimal margins for stickers
                    page.Margin ( 0.1f, Unit.Centimetre );
                    page.PageColor ( Colors.White );
                    page.DefaultTextStyle ( x => x.FontSize ( 8 ) );

                    page.Content ( ).Column ( col =>
                    {
                        foreach ( var code in barcodes )
                        {
                            // Draw the label
                            col.Item ( ).Element ( e => DrawLabel ( e, code, widthCm, heightCm ) );

                            // Page break between labels (important for thermal printers)
                            if ( code != barcodes.Last ( ) )
                                col.Item ( ).PageBreak ( );
                        }
                    } );
                } );
            } );

            return document.GeneratePdf ( );
        }
        #endregion

        #region 2. A4 Printing (Laser/Inkjet Printers)
        /// <summary>
        /// Print on A4 sheet divided into 3 columns
        /// </summary>
        public byte [] GenerateA4Pdf ( List<string> barcodes )
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
                                grid.Spacing ( 10 );

                                foreach ( var code in barcodes )
                                {
                                    grid.Item ( )
                                        .Border ( 1 )
                                        .BorderColor ( Colors.Grey.Lighten2 )
                                        .Padding ( 0.3f, Unit.Centimetre )
                                        .Height ( 5f, Unit.Centimetre )
                                        .Element ( e => DrawA4Label ( e, code ) );
                                }
                            } );
                        } );
                } );
            } );

            return document.GeneratePdf ( );
        }
        #endregion

        #region 3. Smart Drawing Logic - OPTIMIZED VERSION

        private void DrawLabel ( QuestPDF.Infrastructure.IContainer container, string barcodeText, float widthCm, float heightCm )
        {
            // Generate high-quality barcode image (no embedded text)
            var barcode = new Barcode ( );
            barcode.IncludeLabel = false;
            barcode.Alignment = AlignmentPositions.Center;

            using var img = barcode.Encode ( BarcodeStandard.Type.Code128, barcodeText,
                SKColors.Black, SKColors.White, 400, 150 );
            using var encoded = img.Encode ( SKEncodedImageFormat.Png, 100 );
            var imageBytes = encoded.ToArray ( );

            // Determine label size category
            bool isTiny = heightCm <= 2.5f;
            bool isSmall = heightCm > 2.5f && heightCm < 4.0f;

            // Adaptive sizing - OPTIMIZED FOR 6x2.5
            float borderSize = isTiny ? 0.5f : 1f;
            float padding = isTiny ? 0.05f : ( isSmall ? 0.12f : 0.2f );  // Reduced padding for tiny
            float titleSize = isTiny ? 6f : ( isSmall ? 7f : 9f );        // Increased title size
            float codeSize = isTiny ? 8f : ( isSmall ? 10f : 12f );       // Increased code size

            // Calculate available space for barcode image
            float totalPadding = ( padding * 2 ) + ( borderSize / 72 * 2.54f );
            float titleHeight = isTiny ? 0.25f : 0.35f;  // Reduced title height
            float codeHeight = isTiny ? 0.30f : 0.40f;   // Reduced code height
            float spacing = isTiny ? 0.05f : 0.10f;      // Reduced spacing
            float barcodeHeight = heightCm - totalPadding - titleHeight - codeHeight - ( spacing * 2 );

            // Make sure we have minimum space
            if ( barcodeHeight < 0.8f ) barcodeHeight = 0.8f;

            container
                .Border ( borderSize )
                .BorderColor ( Colors.Grey.Lighten2 )
                .Padding ( padding, Unit.Centimetre )
                .Column ( column =>
                {
                    column.Spacing ( isTiny ? 1 : 2 );

                    //// Title - ALWAYS show for consistency
                    //column.Item ( )
                    //    .AlignCenter ( )
                    //    .Text ( "Namaa InfoLogitic" )
                    //    .FontSize ( titleSize )
                    //    .FontColor ( Colors.Grey.Darken3 )
                    //    .SemiBold ( );

                    // Barcode Image with calculated dimensions
                    column.Item ( )
                        .AlignCenter ( )
                        .Height ( barcodeHeight, Unit.Centimetre )
                        .Width ( widthCm - ( padding * 2 ) - 0.2f, Unit.Centimetre )
                        .Image ( imageBytes, ImageScaling.FitArea );

                    // Barcode Text - larger and more prominent
                    column.Item ( )
                        .AlignCenter ( )
                        .Text ( barcodeText )
                        .FontSize ( codeSize )
                        .Bold ( );
                } );
        }

        private void DrawA4Label ( QuestPDF.Infrastructure.IContainer container, string barcodeText )
        {
            // Generate barcode for A4 labels
            var barcode = new Barcode ( );
            barcode.IncludeLabel = false;
            barcode.Alignment = AlignmentPositions.Center;

            using var img = barcode.Encode ( BarcodeStandard.Type.Code128, barcodeText,
                SKColors.Black, SKColors.White, 400, 150 );
            using var encoded = img.Encode ( SKEncodedImageFormat.Png, 100 );
            var imageBytes = encoded.ToArray ( );

            container
                .Column ( column =>
                {
                    column.Spacing ( 5 );

                    //// Title
                    //column.Item ( )
                    //    .AlignCenter ( )
                    //    .Text ( "Namaa InfoLogitic" )
                    //    .FontSize ( 10 )
                    //    .FontColor ( Colors.Grey.Darken3 )
                    //    .SemiBold ( );

                    // Barcode Image - fixed height for A4
                    column.Item ( )
                        .AlignCenter ( )
                        .Height ( 2.8f, Unit.Centimetre )
                        .Width ( 5.5f, Unit.Centimetre )
                        .Image ( imageBytes, ImageScaling.FitArea );

                    // Barcode Text
                    column.Item ( )
                        .AlignCenter ( )
                        .Text ( barcodeText )
                        .FontSize ( 12 )
                        .Bold ( );
                } );
        }

        #endregion

        #region 4. Alternative: Ultra-Compact Version for Very Small Labels
        /// <summary>
        /// Use this for labels smaller than 2cm height - removes title completely
        /// </summary>
        public byte [] GenerateMicroPdf ( List<string> barcodes, float widthCm, float heightCm )
        {
            var document = QuestPDF.Fluent.Document.Create ( container =>
            {
                container.Page ( page =>
                {
                    page.Size ( new PageSize ( widthCm, heightCm, Unit.Centimetre ) );
                    page.Margin ( 0.05f, Unit.Centimetre );
                    page.PageColor ( Colors.White );

                    page.Content ( ).Column ( col =>
                    {
                        foreach ( var code in barcodes )
                        {
                            // Generate barcode
                            var barcode = new Barcode ( );
                            barcode.IncludeLabel = false;
                            barcode.Alignment = AlignmentPositions.Center;

                            using var img = barcode.Encode ( BarcodeStandard.Type.Code128, code,
                                SKColors.Black, SKColors.White, 400, 150 );
                            using var encoded = img.Encode ( SKEncodedImageFormat.Png, 100 );
                            var imageBytes = encoded.ToArray ( );

                            col.Item ( )
                                .Border ( 0.5f )
                                .BorderColor ( Colors.Grey.Lighten3 )
                                .Padding ( 0.05f, Unit.Centimetre )
                                .Column ( inner =>
                                {
                                    inner.Spacing ( 1 );

                                    // Calculate space
                                    float availableHeight = heightCm - 0.25f;

                                    // Barcode
                                    inner.Item ( )
                                        .AlignCenter ( )
                                        .Height ( availableHeight * 0.70f, Unit.Centimetre )
                                        .Width ( widthCm - 0.25f, Unit.Centimetre )
                                        .Image ( imageBytes, ImageScaling.FitArea );

                                    // Text
                                    inner.Item ( )
                                        .AlignCenter ( )
                                        .Text ( code )
                                        .FontSize ( 7 )
                                        .Bold ( );
                                } );

                            if ( code != barcodes.Last ( ) )
                                col.Item ( ).PageBreak ( );
                        }
                    } );
                } );
            } );

            return document.GeneratePdf ( );
        }
        #endregion
    }
}