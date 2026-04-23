using TaskFlow.Platform.Domain.Tasks.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Platform.Persistence.Tasks.EntityConfigurations;

public sealed class TaskEmailEntityConfiguration : IEntityTypeConfiguration<TaskEmail>
{
    public void Configure(EntityTypeBuilder<TaskEmail> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
