using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrBaseSalaryRateConfiguration : IEntityTypeConfiguration<HrBaseSalaryRate>
{
    public void Configure(EntityTypeBuilder<HrBaseSalaryRate> builder)
    {
        builder.ToTable("HrBaseSalaryRate");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Rate).HasColumnType("decimal(12,2)");

        // BR-SAL-01/02/03: one org-wide rate history, append-only — a new row
        // must have a later EffectiveDate than the latest existing one
        // (application-layer check; this index only prevents two rows
        // sharing the exact same date).
        builder.HasIndex(r => r.EffectiveDate).IsUnique();
    }
}
