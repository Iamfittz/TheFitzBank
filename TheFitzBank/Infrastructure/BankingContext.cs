using Microsoft.EntityFrameworkCore;
using TheFitzBankAPI.Domain;


namespace TheFitzBankAPI.Infrastructure {
    public class BankingContext : DbContext {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public BankingContext(DbContextOptions<BankingContext> options) : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Account>()
                .HasKey(a => a.AccountNumber);
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Id);
            modelBuilder.Entity<Transaction>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.AccountNumber)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Transaction>()
                .Property(a => a.Amount)
                .HasColumnType("decimal(18,2)");

            base.OnModelCreating(modelBuilder);
        }
    }
}