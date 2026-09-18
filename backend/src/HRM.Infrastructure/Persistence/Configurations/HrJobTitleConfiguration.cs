using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRM.Infrastructure.Persistence.Configurations;

public class HrJobTitleConfiguration : IEntityTypeConfiguration<HrJobTitle>
{
    public void Configure(EntityTypeBuilder<HrJobTitle> builder)
    {
        builder.ToTable("HrJobTitle");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(255).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
    }
}
