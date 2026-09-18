using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrOrganizationalUnitConfiguration : IEntityTypeConfiguration<HrOrganizationalUnit>
{
    public void Configure(EntityTypeBuilder<HrOrganizationalUnit> builder)
    {
        builder.ToTable("HrOrganizationalUnit");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(255).IsRequired();
        builder.Property(u => u.UnitType).HasMaxLength(30).IsRequired();
        builder.Property(u => u.ContactEmail).HasMaxLength(255);
        builder.Property(u => u.ContactPhone).HasMaxLength(30);
        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        // BR-ORG-03: name unique among units sharing the same parent. A plain
        // composite index does NOT by itself enforce uniqueness among
        // top-level (ParentId is null) units, since SQL Server treats
        // multiple NULLs as distinct — that case additionally needs the
        // application-layer check already in OrganizationalUnitService.
        builder.HasIndex(u => new { u.ParentId, u.Name }).IsUnique();

        builder.HasOne(u => u.Parent)
            .WithMany(u => u.Children)
            .HasForeignKey(u => u.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
