using InsuranceSystem2._0.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace InsuranceSystem2._0.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<InsuranceProduct> InsuranceProducts { get; set; }
        public DbSet<PolicyPlan> PolicyPlans { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<PremiumPayment> PremiumPayments { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }
        public DbSet<ClaimStatusHistory> ClaimStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // USER -> CUSTOMER (One to One)
            // =========================
            modelBuilder.Entity<User>()
                .HasOne(u => u.Customer)
                .WithOne(c => c.User)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // USER -> CLAIM STATUS HISTORY (One to Many)
            // =========================
            modelBuilder.Entity<User>()
                .HasMany(u => u.ClaimHistories)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // CUSTOMER -> POLICY (One to Many)
            // =========================
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Policies)
                .WithOne(p => p.Customer)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // CUSTOMER -> PREMIUM PAYMENT (One to Many)
            // =========================
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.PremiumPayments)
                .WithOne(pp => pp.Customer)
                .HasForeignKey(pp => pp.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // INSURANCE PRODUCT -> POLICY PLAN (One to Many)
            // =========================
            modelBuilder.Entity<InsuranceProduct>()
                .HasMany(ip => ip.PolicyPlans)
                .WithOne(pp => pp.InsuranceProduct)
                .HasForeignKey(pp => pp.InsuranceProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // INSURANCE PRODUCT -> POLICY (One to Many)
            // =========================
            modelBuilder.Entity<InsuranceProduct>()
                .HasMany(ip => ip.Policies)
                .WithOne(p => p.InsuranceProduct)
                .HasForeignKey(p => p.InsuranceProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // POLICY PLAN -> POLICY (One to Many)
            // =========================
            modelBuilder.Entity<PolicyPlan>()
                .HasMany(pp => pp.Policies)
                .WithOne(p => p.Plan)
                .HasForeignKey(p => p.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // POLICY -> PREMIUM PAYMENT (One to Many)
            // =========================
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Payments)
                .WithOne(pp => pp.Policy)
                .HasForeignKey(pp => pp.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // POLICY -> CLAIM (One to Many)
            // =========================
            modelBuilder.Entity<Policy>()
                .HasMany(p => p.Claims)
                .WithOne(c => c.Policy)
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // CLAIM -> CLAIM DOCUMENT (One to Many)
            // =========================
            modelBuilder.Entity<Claim>()
                .HasMany(c => c.ClaimDocuments)
                .WithOne(cd => cd.Claim)
                .HasForeignKey(cd => cd.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // CLAIM -> CLAIM STATUS HISTORY (One to Many)
            // =========================
            modelBuilder.Entity<Claim>()
                .HasMany(c => c.ClaimStatusHistories)
                .WithOne(h => h.Claim)
                .HasForeignKey(h => h.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // UNIQUE CONSTRAINTS
            // =========================
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<InsuranceProduct>()
                .HasIndex(p => p.ProductName)
                .IsUnique();

            modelBuilder.Entity<Policy>()
                .HasIndex(p => p.PolicyNumber)
                .IsUnique();

            modelBuilder.Entity<PremiumPayment>()
                .HasIndex(p => p.TransactionReference)
                .IsUnique();

            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.ClaimNumber)
                .IsUnique();

            // Optional: if you want plan names unique within a product, uncomment this
            modelBuilder.Entity<PolicyPlan>()
                .HasIndex(pp => new { pp.InsuranceProductId, pp.PlanName })
               .IsUnique();
        }
    }
}
