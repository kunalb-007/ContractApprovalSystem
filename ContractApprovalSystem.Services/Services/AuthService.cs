using ContractApprovalSystem.Core.Interfaces;
using ContractApprovalSystem.Core.Models;
using ContractApprovalSystem.Services.DTOs;
using ContractApprovalSystem.Services.Interfaces;

namespace ContractApprovalSystem.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email exists (single query)
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email == registerDto.Email);
            
            if (existingUsers.Any())
            {
                throw new Exception("Email already registered");
            }
            
            // Hash password (this is slow but only happens once per registration)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            
            var user = new User
            {
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PasswordHash = passwordHash,
                Role = registerDto.Role,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();
            
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }
        
        public async Task<UserDto> LoginAsync(LoginDto loginDto)
        {
            // Single query to get user by email
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == loginDto.Email);
            var user = users.FirstOrDefault();
            
            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }
            
            // Verify password (BCrypt is slow but necessary)
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password");
            }
            
            if (!user.IsActive)
            {
                throw new Exception("Account is inactive");
            }
            
            // Return DTO directly without extra queries
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }
        
        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user != null ? MapToDto(user) : null;
        }
        
        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }
    }
}
