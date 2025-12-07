using ContractApprovalSystem.Services.DTOs;

namespace ContractApprovalSystem.Services.Interfaces
{
    public interface IContractService
    {
        Task<IEnumerable<ContractDto>> GetAllContractsAsync();
        Task<IEnumerable<ContractDto>> GetUserContractsAsync(int userId);
        Task<IEnumerable<ContractDto>> GetPendingApprovalsAsync();
        Task<ContractDto> GetContractByIdAsync(int id);
        Task<ContractDto> CreateContractAsync(CreateContractDto createDto, int userId);
        Task<ContractDto> UpdateContractAsync(int id, CreateContractDto updateDto);
        Task<bool> DeleteContractAsync(int id);
        Task<bool> SubmitForApprovalAsync(int contractId, int userId);
        Task<bool> ApproveContractAsync(ApprovalDto approvalDto, int managerId);

        Task<IEnumerable<ContractDto>> GetManagerApprovalHistoryAsync(int managerId);

    }
}
