using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Infrastructure.Persistence.Mappings;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("tb_transfers");

        builder.HasKey(transfer => transfer.Id);
        builder.Property(transfer => transfer.Id).HasColumnName("id")
            .ValueGeneratedOnAdd();
        builder.Property(transfer => transfer.PayerId).HasColumnName("payer_id")
            .IsRequired();
        builder.Property(transfer => transfer.PayeeId).HasColumnName("payee_id")
            .IsRequired();
        builder.Property(transfer => transfer.Amount).HasColumnName("amount")
            .HasPrecision(18, 2).IsRequired();
        builder.Property(transfer => transfer.CreatedAt).HasColumnName("created_at")
            .HasColumnType("datetime2(3)").IsRequired();

        builder.HasOne<User>().WithMany().HasForeignKey(transfer => transfer.PayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(transfer => transfer.PayeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
