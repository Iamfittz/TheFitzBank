using Microsoft.EntityFrameworkCore;
using TheFitzBankAPI.Domain;


namespace TheFitzBankAPI.Infrastructure {
    public class BankingContext : DbContext {
        public BankingContext(DbContextOptions<BankingContext> options) : base(options) {
        }

        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankingContext).Assembly);
        }
    }
}