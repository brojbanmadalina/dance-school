using DanceSchool.DataAccess.Entities.Attendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceSchool.DataAccess.Data.Configurations;

internal class StudentPointsConfiguration : IEntityTypeConfiguration<StudentPoints>
{
    public void Configure(EntityTypeBuilder<StudentPoints> builder)
    {
        builder.ToTable("student_points");
        builder.HasOne(p => p.Student)
            .WithMany()
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.StudentId).IsUnique();
    }
}
