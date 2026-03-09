using HartfordInsurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HartfordInsurance.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<InsurancePlan> InsurancePlans { get; set; }
    public DbSet<PlanTier> PlanTiers { get; set; }
    public DbSet<CustomerPolicy> CustomerPolicies { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Nominee> Nominees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // PlanTier → InsurancePlan
        modelBuilder.Entity<PlanTier>()
            .HasOne<InsurancePlan>()
            .WithMany()
            .HasForeignKey(t => t.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // CustomerPolicy → InsurancePlan
        modelBuilder.Entity<CustomerPolicy>()
            .HasOne<InsurancePlan>()
            .WithMany()
            .HasForeignKey(p => p.PlanId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<CustomerPolicy>()
    .HasOne<PlanTier>()
    .WithMany()
    .HasForeignKey(cp => cp.TierId)
    .OnDelete(DeleteBehavior.Restrict);
    }
     
}