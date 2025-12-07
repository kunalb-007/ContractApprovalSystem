using ContractApprovalSystem.Core.Enums;
using ContractApprovalSystem.Core.Interfaces;
using ContractApprovalSystem.Core.Models;
using ContractApprovalSystem.Services.DTOs;
using ContractApprovalSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractApprovalSystem.Services.Services
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public ContractService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<IEnumerable<ContractDto>> GetAllContractsAsync()
        {
            var contracts = await _unitOfWork.Contracts.GetAllAsync();
            
            var contractsList = contracts.ToList();
            foreach (var contract in contractsList)
            {
                if (contract.Creator == null && contract.CreatedBy > 0)
                {
                    contract.Creator = await _unitOfWork.Users.GetByIdAsync(contract.CreatedBy);
                }
            }
            
            return contractsList.Select(MapToDto);
        }
        
        public async Task<IEnumerable<ContractDto>> GetUserContractsAsync(int userId)
        {
            var contracts = await _unitOfWork.Contracts
                .FindAsync(c => c.CreatedBy == userId);
            
            var contractsList = contracts.ToList();

            // Single query to load all users needed
            if (contractsList.Any())
            {
                var userIds = contractsList.Select(c => c.CreatedBy).Distinct().ToList();
                var users = await _unitOfWork.Users.GetAllAsync();
                var userDict = users.Where(u => userIds.Contains(u.Id))
                                    .ToDictionary(u => u.Id);
                
                foreach (var contract in contractsList)
                {
                    if (userDict.ContainsKey(contract.CreatedBy))
                    {
                        contract.Creator = userDict[contract.CreatedBy];
                    }
                }
            }
            
            return contractsList.Select(MapToDto);
        }
        
        public async Task<IEnumerable<ContractDto>> GetPendingApprovalsAsync()
        {
            var contracts = await _unitOfWork.Contracts
                .FindAsync(c => c.Status == ContractStatus.PendingApproval);
            
            var contractsList = contracts.ToList();

             // Bulk load creators
            if (contractsList.Any())
            {
                var userIds = contractsList.Select(c => c.CreatedBy).Distinct().ToList();
                var users = await _unitOfWork.Users.GetAllAsync();
                var userDict = users.Where(u => userIds.Contains(u.Id))
                                    .ToDictionary(u => u.Id);
                
                foreach (var contract in contractsList)
                {
                    if (userDict.ContainsKey(contract.CreatedBy))
                    {
                        contract.Creator = userDict[contract.CreatedBy];
                    }
                }
            }
            
            return contractsList.Select(MapToDto);
        }
        
        public async Task<IEnumerable<ContractDto>> GetManagerApprovalHistoryAsync(int managerId)
        {
            // Get all approvals made by this manager
            var approvals = await _unitOfWork.Approvals
                .FindAsync(a => a.ApproverId == managerId);
            
            var approvalsList = approvals.ToList();
            
            // Get unique contract IDs
            var contractIds = approvalsList.Select(a => a.ContractId).Distinct().ToList();
            
            if (!contractIds.Any())
            {
                return Enumerable.Empty<ContractDto>();
            }
            
            // Get all contracts that this manager reviewed
            var allContracts = await _unitOfWork.Contracts.GetAllAsync();
            var reviewedContracts = allContracts
                .Where(c => contractIds.Contains(c.Id))
                .ToList();
            
            // Bulk load creators in one go
            if (reviewedContracts.Any())
            {
                var userIds = reviewedContracts.Select(c => c.CreatedBy).Distinct().ToList();
                var users = await _unitOfWork.Users.GetAllAsync();
                var userDict = users.Where(u => userIds.Contains(u.Id))
                                    .ToDictionary(u => u.Id);
                
                foreach (var contract in reviewedContracts)
                {
                    if (userDict.ContainsKey(contract.CreatedBy))
                    {
                        contract.Creator = userDict[contract.CreatedBy];
                    }
                }
            }
            
            // Return only approved/rejected (not pending)
            return reviewedContracts
                .Where(c => c.Status == ContractStatus.Approved || c.Status == ContractStatus.Rejected)
                .OrderByDescending(c => c.SubmittedDate)
                .Select(MapToDto);
        }
        
        public async Task<ContractDto> GetContractByIdAsync(int id)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
            
            if (contract == null)
                return null;
            
            if (contract.Creator == null && contract.CreatedBy > 0)
            {
                contract.Creator = await _unitOfWork.Users.GetByIdAsync(contract.CreatedBy);
            }
            
            return MapToDto(contract);
        }
        
        public async Task<ContractDto> CreateContractAsync(CreateContractDto createDto, int userId)
        {
            var contract = new Contract
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Amount = createDto.Amount,
                Status = ContractStatus.Draft,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };
            
            await _unitOfWork.Contracts.AddAsync(contract);
            await _unitOfWork.CompleteAsync();
            
            contract.Creator = await _unitOfWork.Users.GetByIdAsync(userId);
            
            return MapToDto(contract);
        }
        
        public async Task<ContractDto> UpdateContractAsync(int id, CreateContractDto updateDto)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
            
            if (contract == null)
            {
                throw new Exception("Contract not found");
            }
            
            if (contract.Status != ContractStatus.Draft)
            {
                throw new Exception("Only draft contracts can be updated");
            }
            
            contract.Title = updateDto.Title;
            contract.Description = updateDto.Description;
            contract.Amount = updateDto.Amount;
            
            _unitOfWork.Contracts.Update(contract);
            await _unitOfWork.CompleteAsync();
            
            if (contract.Creator == null)
            {
                contract.Creator = await _unitOfWork.Users.GetByIdAsync(contract.CreatedBy);
            }
            
            return MapToDto(contract);
        }
        
        public async Task<bool> DeleteContractAsync(int id)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
            
            if (contract == null)
            {
                return false;
            }
            
            if (contract.Status != ContractStatus.Draft)
            {
                throw new Exception("Only draft contracts can be deleted");
            }
            
            _unitOfWork.Contracts.Delete(contract);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }
        
        public async Task<bool> SubmitForApprovalAsync(int contractId, int userId)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
            
            if (contract == null)
            {
                throw new Exception("Contract not found");
            }
            
            if (contract.CreatedBy != userId)
            {
                throw new Exception("Unauthorized");
            }
            
            if (contract.Status != ContractStatus.Draft)
            {
                throw new Exception("Contract is not in draft status");
            }
            
            contract.Status = ContractStatus.PendingApproval;
            contract.SubmittedDate = DateTime.UtcNow;
            
            _unitOfWork.Contracts.Update(contract);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }
        
        public async Task<bool> ApproveContractAsync(ApprovalDto approvalDto, int managerId)
        {
            try
            {
                var contract = await _unitOfWork.Contracts.GetByIdAsync(approvalDto.ContractId);
                
                if (contract == null)
                {
                    throw new Exception("Contract not found");
                }
                
                if (contract.Status != ContractStatus.PendingApproval)
                {
                    throw new Exception("Contract is not pending approval");
                }
                
                var manager = await _unitOfWork.Users.GetByIdAsync(managerId);
                if (manager == null)
                {
                    throw new Exception("Manager not found");
                }
                
                string finalStatus = approvalDto.Status == "Approved" 
                    ? ContractStatus.Approved 
                    : ContractStatus.Rejected;
                
                var approval = new Approval
                {
                    ContractId = approvalDto.ContractId,
                    ApproverId = managerId,
                    Status = finalStatus,
                    Comments = approvalDto.Comments ?? string.Empty,
                    ActionDate = DateTime.UtcNow
                };
                
                contract.Status = finalStatus;
                
                _unitOfWork.Contracts.Update(contract);
                await _unitOfWork.Approvals.AddAsync(approval);
                
                var result = await _unitOfWork.CompleteAsync();
                
                return result > 0;
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Failed to save approval: {errorMessage}", ex);
            }
        }
        
        private ContractDto MapToDto(Contract contract)
        {
            return new ContractDto
            {
                Id = contract.Id,
                Title = contract.Title,
                Description = contract.Description,
                Amount = contract.Amount,
                Status = contract.Status,
                CreatedBy = contract.CreatedBy,
                CreatorName = contract.Creator?.FullName ?? "Unknown User",
                CreatedDate = contract.CreatedDate,
                SubmittedDate = contract.SubmittedDate
            };
        }
    }
}
