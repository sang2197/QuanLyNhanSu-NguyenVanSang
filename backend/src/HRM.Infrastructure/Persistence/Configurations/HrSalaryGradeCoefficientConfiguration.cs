using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrSalaryGradeCoefficientConfiguration : IEntityTypeConfiguration<HrSalaryGradeCoefficient>
{
    public void Configure(EntityTypeBuilder<HrSalaryGradeCoefficient> builder)
    {
        builder.ToTable("HrSalaryGradeCoefficient");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Coefficient).HasColumnType("decimal(5,2)");

        // BR-SAL-12/13: append-only effective-dated history per grade — a new
        // row must have a later EffectiveDate than the latest existing one
        // for that grade (application-layer check).
        builder.HasIndex(c => new { c.SalaryGradeId, c.EffectiveDate }).IsUnique();

        builder.HasOne(c => c.SalaryGrade)
            .WithMany(g => g.Coefficients)
            .HasForeignKey(c => c.SalaryGradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
