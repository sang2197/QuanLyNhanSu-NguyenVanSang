using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrSalaryDecisionDetailConfiguration : IEntityTypeConfiguration<HrSalaryDecisionDetail>
{
    public void Configure(EntityTypeBuilder<HrSalaryDecisionDetail> builder)
    {
        builder.ToTable("HrSalaryDecisionDetail");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.OldCoefficient).HasColumnType("decimal(5,2)");
        builder.Property(d => d.NewCoefficient).HasColumnType("decimal(5,2)");
        builder.Property(d => d.Reason).HasMaxLength(500);

        builder.HasOne(d => d.Decision)
            .WithMany(dec => dec.Details)
            .HasForeignKey(d => d.DecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Employee)
            .WithMany(e => e.DecisionDetails)
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.OldSalary)
            .WithMany()
            .HasForeignKey(d => d.OldSalaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.OldGrade)
            .WithMany()
            .HasForeignKey(d => d.OldGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.NewSalaryGrade)
            .WithMany()
            .HasForeignKey(d => d.NewSalaryGradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
