using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrSalaryScaleConfiguration : IEntityTypeConfiguration<HrSalaryScale>
{
    public void Configure(EntityTypeBuilder<HrSalaryScale> builder)
    {
        builder.ToTable("HrSalaryScale");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();
        builder.Property(s => s.Name).HasMaxLength(255).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.Status).HasMaxLength(30);
    }
}
