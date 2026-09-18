using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrSalaryReviewPeriodConfiguration : IEntityTypeConfiguration<HrSalaryReviewPeriod>
{
    public void Configure(EntityTypeBuilder<HrSalaryReviewPeriod> builder)
    {
        builder.ToTable("HrSalaryReviewPeriod");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(255).IsRequired();
        builder.HasIndex(p => p.Name).IsUnique();
        builder.Property(p => p.ReviewType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
    }
}
