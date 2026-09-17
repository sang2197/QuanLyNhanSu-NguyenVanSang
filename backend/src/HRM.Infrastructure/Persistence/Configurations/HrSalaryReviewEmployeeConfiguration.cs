using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrSalaryReviewEmployeeConfiguration : IEntityTypeConfiguration<HrSalaryReviewEmployee>
{
    public void Configure(EntityTypeBuilder<HrSalaryReviewEmployee> builder)
    {
        builder.ToTable("HrSalaryReviewEmployee");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CurrentCoefficient).HasColumnType("decimal(5,2)");
        builder.Property(e => e.ProposedCoefficient).HasColumnType("decimal(5,2)");
        builder.Property(e => e.EligibilityStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.EligibilityReason).HasMaxLength(500);
        builder.Property(e => e.ReviewStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(500);

        builder.HasIndex(e => new { e.ReviewPeriodId, e.EmployeeId }).IsUnique();

        builder.HasOne(e => e.ReviewPeriod)
            .WithMany(p => p.ReviewEmployees)
            .HasForeignKey(e => e.ReviewPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Employee)
            .WithMany(emp => emp.ReviewEntries)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CurrentSalary)
            .WithMany()
            .HasForeignKey(e => e.CurrentSalaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CurrentGrade)
            .WithMany()
            .HasForeignKey(e => e.CurrentGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProposedGrade)
            .WithMany()
            .HasForeignKey(e => e.ProposedGradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
