using DanceSchool.DataAccess.Entities;
using DanceSchool.DataAccess.Entities.Courses;
using DanceSchool.DataAccess.Entities.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceSchool.DataAccess.Data.Configurations;

internal class GroupScheduleConfiguration : IEntityTypeConfiguration<GroupSchedule>
{
    public void Configure(EntityTypeBuilder<GroupSchedule> builder)
    {
        builder.ToTable("group_schedules");
        builder
            .HasOne(gs => gs.Group)
            .WithMany(g => g.Schedules)
            .HasForeignKey(gs => gs.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(gs => gs.Location)
            .WithMany(l => l.GroupSchedules)
            .HasForeignKey(gs => gs.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
