using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.PostgreSQL;
using DanceSchool.DataAccess.Entities;
using DanceSchool.DataAccess.Entities.Attendances;
using DanceSchool.DataAccess.Entities.Auth;
using DanceSchool.DataAccess.Entities.Chat;
using DanceSchool.DataAccess.Entities.Courses;
using DanceSchool.DataAccess.Entities.Locations;
using DanceSchool.DataAccess.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.DataAccess.Data;

public class DanceSchoolDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<DanceStyle> DanceStyles { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<GroupSchedule> GroupSchedules { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationMember> ConversationMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<StudentPoints> StudentPoints { get; set; }
    public DbSet<CourseNotification> CourseNotifications { get; set; }

    public DanceSchoolDbContext(DbContextOptions<DanceSchoolDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(DanceSchoolDbContext).Assembly);
        builder.AddQuartz(q => q.UsePostgreSql());
    }
}
