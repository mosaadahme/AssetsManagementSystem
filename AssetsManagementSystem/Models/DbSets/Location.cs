using AssetsManagementSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetsManagementSystem.Models.DbSets
{
    [Index ( nameof ( Barcode ), IsUnique = true )]
    public class Location : BaseWithAuditEntity
    {
        [Required ( ErrorMessage = "Barcode is required." )]
        [MaxLength ( 100 )]
        public string Barcode { get; set; }

        [Required ( ErrorMessage = "Location name is required." )]
        [MaxLength ( 200 )]
        public string Name { get; set; }

        // هنسيب ده عشان الـ Compatibility
        // وهنملأه أوتوماتيك بـ (الدولة - المدينة - المبنى...) وقت الحفظ
        [Required ( ErrorMessage = "Address is required." )]
        [MaxLength ( 500 )]
        public string Address { get; set; }

        // ==========================================
        // الجزء الجديد الخاص بالـ Hierarchy
        // ==========================================

        [Required]
        public LocationLevel Level { get; set; }

        // الـ ID بتاع المكان الأب (مثلاً ID المبنى لو ده "دور")
        public int? ParentLocationId { get; set; }

        [ForeignKey ( nameof ( ParentLocationId ) )]
        public virtual Location ParentLocation { get; set; }

        // قائمة بالأماكن اللي تحت المكان ده (مثلاً كل الغرف اللي في الدور)
        public virtual ICollection<Location> ChildLocations { get; set; } = new HashSet<Location> ( );

        // ==========================================

        public virtual ICollection<Asset> Assets { get; set; } = new HashSet<Asset> ( );
    }
    public enum LocationLevel
    {
        Country = 1,
        City = 2,
        Region = 3,
        Building = 4,
        Floor = 5,
        Room = 6
    }
}