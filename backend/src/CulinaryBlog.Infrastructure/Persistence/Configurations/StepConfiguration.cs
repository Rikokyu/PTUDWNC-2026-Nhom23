using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Persistence.Configurations;

public class RecipeStepConfiguration
    : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(
        EntityTypeBuilder<RecipeStep> builder)
    {
        builder.ToTable(
            "RecipeSteps",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_RecipeSteps_StepNumber_Positive",
                    "\"StepNumber\" > 0");
                table.HasCheckConstraint(
                    "CK_RecipeSteps_TimerMinutes_NonNegative",
                    "\"TimerMinutes\" >= 0");
            });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(x =>
            new { x.RecipeId, x.StepNumber })
            .IsUnique();
    }
}