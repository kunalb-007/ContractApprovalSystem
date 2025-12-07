using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractApprovalSystem.Core.Models
{
    public class Approval
    {
        [Key]
        public int Id { get; set; }
        
        public int ContractId { get; set; }
        
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; }
        
        public int ApproverId { get; set; }
        
        [ForeignKey("ApproverId")]
        public virtual User Approver { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } // "Approved", "Rejected"
        
        [StringLength(500)]
        public string Comments { get; set; }
        
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    }
}
