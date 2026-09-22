using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrLaborContractConfiguration : IEntityTypeConfiguration<HrLaborContract>
{
    public void Configure(EntityTypeBuilder<HrLaborContract> builder)
    {
        builder.ToTable("HrLaborContract");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ContractNumber).HasMaxLength(100).IsRequired();
        builder.Property(c => c.ContractType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(c => c.ContractSalaryAmount).HasColumnType("decimal(14,2)");
        builder.Property(c => c.SalaryNote).HasMaxLength(500);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(c => c.TerminationReason).HasMaxLength(500);

        builder.HasIndex(c => c.ContractNumber).IsUnique(); // BR-CON-01

        // At most one Active contract per employee (BR-CON-10). DBML has no
        // filtered-index syntax, so this rule is enforced only here (see the
        // Note on HrLaborContract in Docs/Database/HRM_System.dbml) — the
        // same pattern as HrSalaryDecision's non-cancelled-period index.
        builder.HasIndex(c => c.EmployeeId)
            .IsUnique()
            .HasFilter("[Status] = 'ACTIVE'")
            .HasDatabaseName("HrLaborContract_index_one_active_per_employee");

        builder.HasOne(c => c.Employee)
            .WithMany(e => e.Contracts)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
