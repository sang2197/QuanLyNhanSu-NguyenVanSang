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
        builder.Property(d => d.DecisionType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.FileUrl).HasMaxLength(500);

        // At most one non-cancelled decision per review period (US-06) —
        // matches the filtered unique index in Docs/Database/Gen_Table.sql.
        builder.HasIndex(d => d.ReviewPeriodId)
            .IsUnique()
            .HasFilter("[Status] <> 'CANCELLED'")
            .HasDatabaseName("HrSalaryDecision_index_noncancelled_period");

        builder.HasOne(d => d.ReviewPeriod)
            .WithMany(p => p.Decisions)
            .HasForeignKey(d => d.ReviewPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.SignerEmployee)
            .WithMany()
            .HasForeignKey(d => d.SignerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
