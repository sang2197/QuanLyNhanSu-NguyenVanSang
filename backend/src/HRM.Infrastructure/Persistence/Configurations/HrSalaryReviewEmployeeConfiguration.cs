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
        builder.Property(e => e.Outcome).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.IneligibleReason).HasMaxLength(500);
        builder.Property(e => e.RejectionReason).HasMaxLength(500);

        builder.HasIndex(e => new { e.ReviewPeriodId, e.EmployeeId }).IsUnique();

        builder.HasOne(e => e.ReviewPeriod)
            .WithMany(p => p.ReviewEmployees)
            .HasForeignKey(e => e.ReviewPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Employee)
            .WithMany(emp => emp.ReviewEntries)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CurrentSalaryGrade)
            .WithMany()
            .HasForeignKey(e => e.CurrentSalaryGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProposedSalaryGrade)
            .WithMany()
            .HasForeignKey(e => e.ProposedSalaryGradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
