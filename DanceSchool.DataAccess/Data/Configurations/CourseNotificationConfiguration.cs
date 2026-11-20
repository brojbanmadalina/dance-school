using DanceSchool.DataAccess.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceSchool.DataAccess.Data.Configurations;

internal class CourseNotificationConfiguration : IEntityTypeConfiguration<CourseNotification>
{
    public void Configure(EntityTypeBuilder<CourseNotification> builder)
    {
        builder.ToTable("course_notifications");
        builder.HasOne(n => n.Course)
            .WithMany()
            .HasForeignKey(n => n.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.CourseId, n.UserId }).IsUnique();
    }
}
