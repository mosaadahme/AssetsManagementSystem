﻿namespace AssetsManagementSystem.DTOs.LocationDTOs
{
    public class GetLocationRequestDTO
    {
        public int Id { get; set; }

        public string Barcode { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime AddedOnDate { set; get; }
        public DateTime? UpdatedDate { set; get; }
        
    }
}
