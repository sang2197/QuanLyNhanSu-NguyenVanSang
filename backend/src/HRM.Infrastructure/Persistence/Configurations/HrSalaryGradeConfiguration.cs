using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrSalaryGradeConfiguration : IEntityTypeConfiguration<HrSalaryGrade>
{
    public void Configure(EntityTypeBuilder<HrSalaryGrade> builder)
    {
        builder.ToTable("HrSalaryGrade");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Coefficient).HasColumnType("decimal(5,2)");
        builder.Property(g => g.Status).HasMaxLength(30);
        builder.Ignore(g => g.IsActive);
        builder.HasIndex(g => new { g.SalaryScaleId, g.GradeNumber }).IsUnique();

        builder.HasOne(g => g.SalaryScale)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.SalaryScaleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
