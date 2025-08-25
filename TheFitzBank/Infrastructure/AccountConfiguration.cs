using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheFitzBankAPI.Domain;

namespace TheFitzBankAPI.Infrastructure {
    public class AccountConfiguration : IEntityTypeConfiguration<Account> {
        public void Configure(EntityTypeBuilder<Account> builder) {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(a => a.AccountNumber)
                   .IsRequired();

            builder.HasIndex(a => a.AccountNumber)
                   .IsUnique();

            builder.Property(a => a.OwnerName)
                   .IsRequired();

            builder.Property(a => a.Currency)
                   .HasMaxLength(3)
                   .IsRequired();

            builder.Property(a => a.Balance)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(a => a.CreatedAt)
                   .HasColumnType("datetime2(0)");

            builder.Property(a => a.UpdatedAt)
                   .HasColumnType("datetime2(0)");

            builder.Property(a => a.IsClosed)
                   .HasDefaultValue(false);
        }
    }
}
