namespace AssetsManagementSystem.DTOs.SupplierDTOs
{
    public class GetSupplierRequestDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string email { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Note { get; set; }
        public DateTime AddedOnDate { set; get; }
        public DateTime? UpdatedDate { set; get; }

    }
}
