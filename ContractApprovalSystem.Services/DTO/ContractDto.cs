using System.ComponentModel.DataAnnotations;

namespace ContractApprovalSystem.Services.DTOs
{
    public class ContractDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public string Status { get; set; }
        public int CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }
    
    public class CreateContractDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
    
    public class ApprovalDto
    {
        public int ContractId { get; set; }
        
        [Required]
        public string Status { get; set; } // "Approved" or "Rejected"
        
        [StringLength(500)]
        public string Comments { get; set; }
    }
}
