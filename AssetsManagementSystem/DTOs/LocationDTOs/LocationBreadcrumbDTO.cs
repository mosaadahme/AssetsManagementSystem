namespace AssetsManagementSystem.DTOs.LocationDTOs
{
    public class LocationBreadcrumbDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; } // رقم الليفيل
        public string LevelName { get; set; }
    }
}
