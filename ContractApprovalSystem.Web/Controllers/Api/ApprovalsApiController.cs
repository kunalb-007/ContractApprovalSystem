using ContractApprovalSystem.Services.DTOs;
using ContractApprovalSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractApprovalSystem.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class ApprovalsApiController : ControllerBase
    {
        private readonly IContractService _contractService;
        
        public ApprovalsApiController(IContractService contractService)
        {
            _contractService = contractService;
        }
        
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
        
        // GET: api/approvals/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var contracts = await _contractService.GetPendingApprovalsAsync();
                return Ok(new { success = true, data = contracts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        // POST: api/approvals/approve
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveContract([FromBody] ApprovalDto approvalDto)
        {
            try
            {
                var managerId = GetCurrentUserId();
                await _contractService.ApproveContractAsync(approvalDto, managerId);
                
                return Ok(new { success = true, message = "Contract approval action completed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
