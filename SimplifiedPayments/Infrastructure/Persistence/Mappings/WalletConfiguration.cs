using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Infrastructure.Persistence.Mappings;

public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("tb_wallets");

        builder.HasKey(wallet => wallet.Id);
        builder.Property(wallet => wallet.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(wallet => wallet.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(wallet => wallet.Balance)
            .HasColumnName("balance").HasPrecision(18, 2).IsRequired();
    }
}
