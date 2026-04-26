using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetsManagementSystem.Models.Commons.Common;

namespace AssetsManagementSystem.Models.DbSets
{
    public class SupplierContactPerson : BaseWithAuditEntity
    {
        [Required ( ErrorMessage = "Contact person name is required." )]
        [MaxLength ( 100, ErrorMessage = "Name cannot exceed 100 characters." )]
        public string Name { get; set; }

        [MaxLength ( 100, ErrorMessage = "Job title cannot exceed 100 characters." )]
        public string JobTitle { get; set; } // مهم جداً عشان تعرف صفته (مدير مبيعات، دعم فني، إلخ)

        [EmailAddress ( ErrorMessage = "Invalid email format." )]
        [MaxLength ( 200, ErrorMessage = "Email cannot exceed 200 characters." )]
        public string Email { get; set; }

        [MaxLength ( 20, ErrorMessage = "Phone number cannot exceed 20 characters." )]
        public string PhoneNumber { get; set; }

        public string? Note { get; set; }

        // حالة الشخص (هل ما زال يعمل لدى المورد ومسؤول عن التواصل؟)
        public bool IsActive { get; set; } = true;

      
        [Required]
        public int SupplierId { get; set; }

        [ForeignKey ( nameof ( SupplierId ) )]
        public virtual Supplier Supplier { get; set; }
    }
}