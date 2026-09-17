using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrEmployeeConfiguration : IEntityTypeConfiguration<HrEmployee>
{
    public void Configure(EntityTypeBuilder<HrEmployee> builder)
    {
        builder.ToTable("HrEmployee");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.EmployeeCode).IsUnique();
        builder.Property(e => e.FullName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(30);
    }
}
