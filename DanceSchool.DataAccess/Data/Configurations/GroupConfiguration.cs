using DanceSchool.DataAccess.Entities;
using DanceSchool.DataAccess.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceSchool.DataAccess.Data.Configurations;

internal class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");
        builder
            .HasOne(g => g.Course)
            .WithMany(c => c.Groups)
            .HasForeignKey(g => g.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(g => g.Instructor)
            .WithMany()
            .HasForeignKey(g => g.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(g => g.CreatedBy)
            .WithMany()
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(g => g.ApprovedBy)
            .WithMany()
            .HasForeignKey(g => g.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
