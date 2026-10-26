using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Infrastructure.Persistence.Mappings;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("tb_users");
      
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(user => user.FullName)
            .HasColumnName("full_name").HasMaxLength(150).IsRequired();

        builder.Property(user => user.Document)
            .HasColumnName("document").HasMaxLength(14).IsUnicode(false).IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email").HasMaxLength(100).IsUnicode(false).IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password").HasMaxLength(255).IsUnicode(false).IsRequired();

        builder.Property(user => user.Type)
            .HasColumnName("user_type").HasMaxLength(20).IsUnicode(false).IsRequired()
            .HasConversion<UserTypeConverter>();
        
        builder.Ignore(user => user.CanSendMoney);

        builder.HasOne(user => user.Wallet)
            .WithOne()
            .HasForeignKey<Wallet>(wallet => wallet.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.Wallet).IsRequired();
    }
}