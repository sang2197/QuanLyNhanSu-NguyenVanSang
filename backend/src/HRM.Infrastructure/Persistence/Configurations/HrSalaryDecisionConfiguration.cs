using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrSalaryDecisionConfiguration : IEntityTypeConfiguration<HrSalaryDecision>
{
    public void Configure(EntityTypeBuilder<HrSalaryDecision> builder)
    {
        builder.ToTable("HrSalaryDecision");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DecisionNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(d => d.DecisionNumber).IsUnique();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        // At most one non-cancelled decision per review period (US-SGP-06).
        // DBML has no filtered-index syntax, so this rule is enforced only
        // here (see the Note on HrSalaryDecision in Docs/Database/HRM_System.dbml).
        builder.HasIndex(d => d.ReviewPeriodId)
            .IsUnique()
            .HasFilter("[Status] <> 'CANCELLED'")
            .HasDatabaseName("HrSalaryDecision_index_noncancelled_period");

        builder.HasOne(d => d.ReviewPeriod)
            .WithMany(p => p.Decisions)
            .HasForeignKey(d => d.ReviewPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
