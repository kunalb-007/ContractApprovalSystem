using ContractApprovalSystem.Services.DTOs;
using ContractApprovalSystem.Services.Interfaces;
using ContractApprovalSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ContractApprovalSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ContractsApiController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractsApiController(IContractService contractService)
        {
            _contractService = contractService;
        }

        /// <summary>
        /// Get all contracts for the current user
        /// </summary>
        /// <returns>List of contracts</returns>
        [HttpGet("my-contracts")]
        [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyContracts()
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            var contracts = await _contractService.GetUserContractsAsync(userId.Value);
            return Ok(contracts);
        }

        /// <summary>
        /// Get contract by ID
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Contract details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null)
                return NotFound(new { message = "Contract not found" });

            return Ok(contract);
        }

        /// <summary>
        /// Create a new contract
        /// </summary>
        /// <param name="model">Contract details</param>
        /// <returns>Created contract</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = SessionHelper.GetUserId(HttpContext);
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                await _contractService.CreateContractAsync(model, userId.Value);
                return CreatedAtAction(nameof(GetMyContracts), new { message = "Contract created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing contract
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="model">Updated contract details</param>
        /// <returns>Success message</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] CreateContractDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _contractService.UpdateContractAsync(id, model);
                return Ok(new { message = "Contract updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Submit contract for approval
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SubmitContract(int id)
        {
            var userId = SessionHelper.GetUserId(HttpContext);
            if (!userId.HasValue)
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                await _contractService.SubmitForApprovalAsync(id, userId.Value);
                return Ok(new { message = "Contract submitted for approval" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a contract (draft only)
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteContract(int id)
        {
            try
            {
                await _contractService.DeleteContractAsync(id);
                return Ok(new { message = "Contract deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
