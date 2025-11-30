namespace AssetsManagementSystem.DTOs.Printing
{
    public class PrintRequestDTO
    {
        public List<string> Barcodes { get; set; }

        // المقاسات بالسنتيمتر (اختياري، وليها قيم افتراضية)
        public float WidthCm { get; set; } = 5f;  // العرض الافتراضي
        public float HeightCm { get; set; } = 5f; // الطول الافتراضي
    }
}
