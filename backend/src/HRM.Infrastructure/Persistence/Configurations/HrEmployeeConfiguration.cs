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
        builder.Property(e => e.EmploymentStatus).HasConversion<string>().HasMaxLength(30).IsRequired();

        // BR-EMP-04/05: the referenced unit/title must be Active at the time
        // they are assigned — enforced at the application layer, not by a DB
        // constraint, so a later deactivation does not retroactively
        // invalidate this employee row.
        builder.HasOne(e => e.OrganizationalUnit)
            .WithMany(u => u.Employees)
            .HasForeignKey(e => e.OrganizationalUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.JobTitle)
            .WithMany(t => t.Employees)
            .HasForeignKey(e => e.JobTitleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
