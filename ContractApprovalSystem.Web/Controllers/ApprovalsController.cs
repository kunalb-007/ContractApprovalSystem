using ContractApprovalSystem.Services.DTOs;
using ContractApprovalSystem.Services.Interfaces;
using ContractApprovalSystem.Web.Filters;
using ContractApprovalSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ContractApprovalSystem.Web.Controllers
{
    [ManagerAuthFilter]
    public class ApprovalsController : Controller
    {
        private readonly IContractService _contractService;
        
        public ApprovalsController(IContractService contractService)
        {
            _contractService = contractService;
        }
        
        private int GetCurrentUserId()
        {
            return SessionHelper.GetUserId(HttpContext) ?? 0;
        }
        
        // GET: Approvals/History - Shows contracts manager has already reviewed
        public async Task<IActionResult> History()
        {
            var managerId = GetCurrentUserId();
            var contracts = await _contractService.GetManagerApprovalHistoryAsync(managerId);
            return View(contracts);
        }
        
        // GET: Approvals/Index - Shows pending contracts awaiting approval
        public async Task<IActionResult> Index()
        {
            var contracts = await _contractService.GetPendingApprovalsAsync();
            return View(contracts);
        }
        
        // POST: Approvals/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int contractId, string comments)
        {
            try
            {
                var approvalDto = new ApprovalDto
                {
                    ContractId = contractId,
                    Status = "Approved",
                    Comments = comments
                };
                
                var managerId = GetCurrentUserId();
                await _contractService.ApproveContractAsync(approvalDto, managerId);
                
                TempData["SuccessMessage"] = "Contract approved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Approvals/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int contractId, string comments)
        {
            try
            {
                var approvalDto = new ApprovalDto
                {
                    ContractId = contractId,
                    Status = "Rejected",
                    Comments = comments
                };
                
                var managerId = GetCurrentUserId();
                await _contractService.ApproveContractAsync(approvalDto, managerId);
                
                TempData["SuccessMessage"] = "Contract rejected successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
