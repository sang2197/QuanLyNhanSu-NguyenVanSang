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
        builder.Property(s => s.Reason).HasMaxLength(255);

        builder.HasOne(s => s.Employee)
            .WithMany(e => e.Salaries)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SalaryScale)
            .WithMany(sc => sc.EmployeeSalaries)
            .HasForeignKey(s => s.SalaryScaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SalaryGrade)
            .WithMany()
            .HasForeignKey(s => s.SalaryGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Decision)
            .WithMany()
            .HasForeignKey(s => s.DecisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
