using AssetsManagementSystem.Models.Commons.Common;

namespace AssetsManagementSystem.Models.DbSets
{
    public class Supplier:BaseWithAuditEntity
    {
        [Required(ErrorMessage = "Supplier name is required.")]
        [MaxLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters.")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Contact information is required.")]
        [MaxLength(200, ErrorMessage = "Contact information cannot exceed 200 characters.")]
        [MinLength(10,ErrorMessage = "Contact information cannot less than 10 characters.")]
        public string email { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Note { get; set; }
        public virtual ICollection<AssetsSuppliers> AssetsSuppliers { get; set; }
        public virtual ICollection<SupplierContactPerson> ContactPersons { get; set; } = new HashSet<SupplierContactPerson> ( );
    }
}
