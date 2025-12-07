using ContractApprovalSystem.Services.DTOs;

namespace ContractApprovalSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto registerDto);
        Task<UserDto> LoginAsync(LoginDto loginDto);
        Task<UserDto> GetUserByIdAsync(int userId);
    }
}
