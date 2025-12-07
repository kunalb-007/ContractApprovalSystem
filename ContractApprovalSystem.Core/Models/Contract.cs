using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractApprovalSystem.Core.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } // "Draft", "PendingApproval", "Approved", "Rejected"
        
        public int CreatedBy { get; set; }
        
        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? SubmittedDate { get; set; }
        
        // Navigation properties
        public virtual ICollection<Approval> Approvals { get; set; }
    }
}
