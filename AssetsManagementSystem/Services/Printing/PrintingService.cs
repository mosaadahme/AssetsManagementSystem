
//using BarcodeStandard;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;
//using SkiaSharp; // <-- المكتبة الجديدة للصور

//namespace AssetsManagementSystem.Services.Printing
//{
//    public class PrintingService
//    {
//        public PrintingService ( )
//        {
//            QuestPDF.Settings.License = LicenseType.Community;
//        }

//        public byte [] GenerateBarcodeLabelsPdf ( List<string> barcodes )
//        {
//            var document = QuestPDF.Fluent.Document.Create ( container =>
//            {
//                container.Page ( page =>
//                {
//                    page.Size ( PageSizes.A4 );
//                    page.Margin ( 1, Unit.Centimetre );
//                    page.PageColor ( Colors.White );
//                    page.DefaultTextStyle ( x => x.FontSize ( 10 ) );

//                    page.Content ( )
//                        .PaddingVertical ( 0.5f, Unit.Centimetre )
//                        .Column ( column =>
//                        {
//                            column.Item ( ).Grid ( grid =>
//                            {
//                                grid.Columns ( 3 );
//                                grid.Spacing ( 15 );

//                                foreach ( var code in barcodes )
//                                {
//                                    grid.Item ( ).Element ( e => DrawSingleLabel ( e, code ) );
//                                }
//                            } );
//                        } );
//                } );
//            } );

//            return document.GeneratePdf ( );
//        }

//        private void DrawSingleLabel ( QuestPDF.Infrastructure.IContainer container, string barcodeText )
//        {
//            // 1. إعداد الباركود
//            var barcode = new Barcode ( );
//            barcode.IncludeLabel = false;
//            barcode.Alignment = AlignmentPositions.Center;

//            // 2. توليد الصورة باستخدام SkiaSharp (المكتبة الحديثة)
//            // استخدمنا SKColors بدل Color عشان نمنع التعارض
//            var img = barcode.Encode ( BarcodeStandard.Type.Code128, barcodeText, SKColors.Black, SKColors.White, 290, 120 );

//            // 3. تحويل الصورة لـ Byte Array (الطريقة الجديدة)
//            using var encoded = img.Encode ( SKEncodedImageFormat.Png, 100 );
//            var imageBytes = encoded.ToArray ( );

//            // 4. رسم الاستيكر
//            container
//                .Border ( 1 )
//                .BorderColor ( Colors.Grey.Lighten1 )
//                .Height ( 3.5f, Unit.Centimetre )
//                .Column ( col =>
//                {
//                    col.Item ( ).AlignCenter ( ).Text ( "Asset Management Sys" ).FontSize ( 7 ).FontColor ( Colors.Grey.Darken2 );

//                    col.Item ( )
//                       .Height ( 1.8f, Unit.Centimetre )
//                       .AlignCenter ( )
//                       .AlignMiddle ( )
//                       .Image ( imageBytes )
//                       .FitArea ( );

//                    col.Item ( ).AlignCenter ( ).Text ( barcodeText ).FontSize ( 10 ).Bold ( );
//                } );
//        }
//    }
//}

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

        // التعديل: الدالة بتاخد DTO كامل بدل ليستة بس
        public byte [] GenerateBarcodeLabelsPdf ( List<string> barcodes, float widthCm, float heightCm )
        {
            var document = QuestPDF.Fluent.Document.Create ( container =>
            {
                container.Page ( page =>
                {
                    // 1. ضبط مقاس الصفحة بناءً على طلب اليوزر (ديناميكي)
                    page.Size ( new PageSize ( widthCm, heightCm, Unit.Centimetre ) );

                    // هوامش صغيرة جداً للطابعات الحرارية
                    page.Margin ( 0.2f, Unit.Centimetre );

                    page.PageColor ( Colors.White );
                    page.DefaultTextStyle ( x => x.FontSize ( 8 ) ); // تصغير الخط ليتناسب مع المقاسات الصغيرة

                    page.Content ( ).Column ( col =>
                    {
                        foreach ( var code in barcodes )
                        {
                            // كل باركود في صفحة لوحده (نظام الطابعات الحرارية)
                            col.Item ( ).PageBreak ( );

                            // رسم الاستيكر ليملأ الصفحة بالكامل
                            col.Item ( ).Element ( e => DrawDynamicLabel ( e, code, widthCm, heightCm ) );
                        }
                    } );
                } );
            } );

            return document.GeneratePdf ( );
        }

        private void DrawDynamicLabel ( QuestPDF.Infrastructure.IContainer container, string barcodeText, float width, float height )
        {
            // إعداد الباركود
            var barcode = new Barcode ( );
            barcode.IncludeLabel = false;
            barcode.Alignment = AlignmentPositions.Center;

            // توليد الصورة (بنكبر الدقة شوية)
            using var img = barcode.Encode ( BarcodeStandard.Type.Code128, barcodeText, SKColors.Black, SKColors.White, 400, 150 );
            using var encoded = img.Encode ( SKEncodedImageFormat.Png, 100 );
            var imageBytes = encoded.ToArray ( );

            // رسم المحتوى داخل الحدود المتاحة
            container
                .Border ( 1 ) // برواز اختياري (ممكن تشيله لو الورق ليه حدود)
                .BorderColor ( Colors.Grey.Lighten2 )
                .Height ( height, Unit.Centimetre ) // الارتفاع من الـ DTO
                .Layers ( layers =>
                {
                    // 1. العنوان (فوق)
                    layers.PrimaryLayer ( ).AlignTop ( ).PaddingTop ( 0.1f, Unit.Centimetre ).AlignCenter ( )
                          .Text ( "Asset Management" ).FontSize ( 6 ).FontColor ( Colors.Grey.Darken3 );

                    // 2. الباركود (في المنتصف - ماخد راحته)
                    layers.Layer ( ).AlignMiddle ( )
                          .PaddingVertical ( 0.8f, Unit.Centimetre ) // سيب مساحة فوق وتحت
                          .PaddingHorizontal ( 0.1f, Unit.Centimetre )
                          .AlignCenter ( )
                          .ScaleToFit ( ) // مهم جداً عشان الصورة تتكيف مع 5x5 أو 6x2.5
                          .Image ( imageBytes );

                    // 3. الكود (تحت)
                    layers.Layer ( ).AlignBottom ( ).PaddingBottom ( 0.1f, Unit.Centimetre ).AlignCenter ( )
                          .Text ( barcodeText ).FontSize ( 8 ).Bold ( );
                } );
        }
    }
}