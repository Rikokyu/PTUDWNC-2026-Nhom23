using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.EmailConfirmed)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.GoogleSubject)
            .HasMaxLength(200);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.Bio)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.GoogleSubject)
            .IsUnique();

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}