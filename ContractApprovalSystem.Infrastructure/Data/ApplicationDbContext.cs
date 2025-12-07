using ContractApprovalSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractApprovalSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired();
            });
            
            // Contract entity configuration
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasOne(c => c.Creator)
                    .WithMany(u => u.Contracts)
                    .HasForeignKey(c => c.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.Property(c => c.Status)
                    .HasDefaultValue("Draft");
            });
            
            // Approval entity configuration
            modelBuilder.Entity<Approval>(entity =>
            {
                entity.HasOne(a => a.Contract)
                    .WithMany(c => c.Approvals)
                    .HasForeignKey(a => a.ContractId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(a => a.Approver)
                    .WithMany(u => u.Approvals)
                    .HasForeignKey(a => a.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
