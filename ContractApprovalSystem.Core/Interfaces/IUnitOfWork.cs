using System;
using System.Threading.Tasks;
using ContractApprovalSystem.Core.Models;

namespace ContractApprovalSystem.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Contract> Contracts { get; }
        IRepository<Approval> Approvals { get; }

        Task<int> CompleteAsync();
    }
}
