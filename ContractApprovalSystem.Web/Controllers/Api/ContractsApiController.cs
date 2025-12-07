using ContractApprovalSystem.Services.DTOs;
using ContractApprovalSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContractApprovalSystem.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContractsApiController : ControllerBase
    {
        private readonly IContractService _contractService;
        
        public ContractsApiController(IContractService contractService)
        {
            _contractService = contractService;
        }
        
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
        
        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }
        
        // GET: api/contracts
        [HttpGet]
        public async Task<IActionResult> GetContracts()
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();
                
                IEnumerable<ContractDto> contracts;
                
                if (role == "Manager")
                {
                    contracts = await _contractService.GetAllContractsAsync();
                }
                else
                {
                    contracts = await _contractService.GetUserContractsAsync(userId);
                }
                
                return Ok(new { success = true, data = contracts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        // GET: api/contracts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContract(int id)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(id);
                
                if (contract == null)
                {
                    return NotFound(new { success = false, message = "Contract not found" });
                }
                
                return Ok(new { success = true, data = contract });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        // POST: api/contracts
        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var contract = await _contractService.CreateContractAsync(createDto, userId);
                
                return CreatedAtAction(
                    nameof(GetContract), 
                    new { id = contract.Id }, 
                    new { success = true, data = contract, message = "Contract created successfully" }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        // PUT: api/contracts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] CreateContractDto updateDto)
        {
            try
            {
                var contract = await _contractService.UpdateContractAsync(id, updateDto);
                return Ok(new { success = true, data = contract, message = "Contract updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        // DELETE: api/contracts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            try
            {
                var result = await _contractService.DeleteContractAsync(id);
                
                if (!result)
                {
                    return NotFound(new { success = false, message = "Contract not found" });
                }
                
                return Ok(new { success = true, message = "Contract deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        
        // POST: api/contracts/5/submit
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _contractService.SubmitForApprovalAsync(id, userId);
                
                return Ok(new { success = true, message = "Contract submitted for approval" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
