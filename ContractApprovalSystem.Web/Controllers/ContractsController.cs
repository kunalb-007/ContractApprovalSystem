using ContractApprovalSystem.Services.DTOs;
using ContractApprovalSystem.Services.Interfaces;
using ContractApprovalSystem.Web.Filters;
using ContractApprovalSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ContractApprovalSystem.Web.Controllers
{
    [SessionAuthFilter]
    public class ContractsController : Controller
    {
        private readonly IContractService _contractService;

        public ContractsController(IContractService contractService)
        {
            _contractService = contractService;
        }

        private int GetCurrentUserId()
            => SessionHelper.GetUserId(HttpContext) ?? 0;

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var role = SessionHelper.GetUserRole(HttpContext);

            // ALWAYS return only contracts created by current user
            var contracts = await _contractService.GetUserContractsAsync(userId);

            ViewData["UserRole"] = role;
            return View(contracts);  // Goes to Views/Contracts/Index.cshtml
        }


        
        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            
            if (contract == null)
            {
                return NotFound();
            }
            
            return View(contract);
        }
        
        // GET: Contracts/Create
        public IActionResult Create()
        {
            return View();
        }
        
        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                var userId = GetCurrentUserId();
                await _contractService.CreateContractAsync(model, userId);
                TempData["SuccessMessage"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
        
        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            
            if (contract == null)
            {
                return NotFound();
            }
            
            var model = new CreateContractDto
            {
                Title = contract.Title,
                Description = contract.Description,
                Amount = contract.Amount
            };
            
            ViewData["ContractId"] = id;
            return View(model);
        }
        
        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateContractDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ContractId"] = id;
                return View(model);
            }
            
            try
            {
                await _contractService.UpdateContractAsync(id, model);
                TempData["SuccessMessage"] = "Contract updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewData["ContractId"] = id;
                return View(model);
            }
        }
        
        // POST: Contracts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _contractService.DeleteContractAsync(id);
                TempData["SuccessMessage"] = "Contract deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Contracts/Submit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _contractService.SubmitForApprovalAsync(id, userId);
                TempData["SuccessMessage"] = "Contract submitted for approval!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
