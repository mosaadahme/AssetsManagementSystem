namespace AssetsManagementSystem.DTOs.AssetTransferDTOs
{
    //public class BulkMoveToLocationDTO
    //{
    //    public string TargetLocationBarcode { get; set; }

    //    // الأصول اللي هتتنقل
    //    public List<string> AssetBarcodes { get; set; }
    //}

    public class BulkMoveRequestDTO
    {
        // دي الليستة اللي في الصورة بتاعتك
        public List<LocationAssignmentDTO> Assignments { get; set; }
    }

    // 2. كلاس بيمثل كل مجموعة (مكان + أصوله)
    public class LocationAssignmentDTO
    {
        public string TargetLocationBarcode { get; set; }
        public List<string> AssetBarcodes { get; set; }
    }
}
