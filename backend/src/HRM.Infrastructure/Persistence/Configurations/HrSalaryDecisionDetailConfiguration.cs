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
        builder.Property(d => d.BaselineCoefficient).HasColumnType("decimal(5,2)");
        builder.Property(d => d.NewCoefficient).HasColumnType("decimal(5,2)");

        builder.HasIndex(d => new { d.SalaryDecisionId, d.EmployeeId }).IsUnique();

        builder.HasOne(d => d.SalaryDecision)
            .WithMany(dec => dec.Details)
            .HasForeignKey(d => d.SalaryDecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Employee)
            .WithMany(e => e.DecisionDetails)
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.BaselineSalaryGrade)
            .WithMany()
            .HasForeignKey(d => d.BaselineSalaryGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.NewSalaryGrade)
            .WithMany()
            .HasForeignKey(d => d.NewSalaryGradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
