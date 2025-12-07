using ContractApprovalSystem.Core.Interfaces;
using ContractApprovalSystem.Core.Models;
using ContractApprovalSystem.Infrastructure.Data;

namespace ContractApprovalSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        
        public IRepository<User> Users { get; private set; }
        public IRepository<Contract> Contracts { get; private set; }
        public IRepository<Approval> Approvals { get; private set; }
        
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new Repository<User>(_context);
            Contracts = new Repository<Contract>(_context);
            Approvals = new Repository<Approval>(_context);
        }
        
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
