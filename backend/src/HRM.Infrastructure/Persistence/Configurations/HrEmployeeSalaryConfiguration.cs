using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrEmployeeSalaryConfiguration : IEntityTypeConfiguration<HrEmployeeSalary>
{
    public void Configure(EntityTypeBuilder<HrEmployeeSalary> builder)
    {
        builder.ToTable("HrEmployeeSalary");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Coefficient).HasColumnType("decimal(5,2)");
        builder.Property(s => s.Reason).HasMaxLength(255).IsRequired();

        // Append-only per-employee salary/grade history — no EffectiveTo
        // column; a later row with a newer EffectiveDate supersedes it.
        builder.HasIndex(s => new { s.EmployeeId, s.EffectiveDate }).IsUnique();

        builder.HasOne(s => s.Employee)
            .WithMany(e => e.Salaries)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SalaryGrade)
            .WithMany()
            .HasForeignKey(s => s.SalaryGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SalaryDecision)
            .WithMany()
            .HasForeignKey(s => s.SalaryDecisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
