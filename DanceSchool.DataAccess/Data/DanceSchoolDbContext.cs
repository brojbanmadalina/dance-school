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

    public DanceSchoolDbContext(DbContextOptions<DanceSchoolDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>().Property(x => x.Email).HasMaxLength(100).IsRequired();

        builder.Entity<RefreshToken>().HasKey(x => x.Id);
        builder
            .Entity<RefreshToken>()
            .HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId);

        builder.Entity<Course>(entity =>
        {
            entity
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(c => c.ApprovedBy)
                .WithMany()
                .HasForeignKey(c => c.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Group>(entity =>
        {
            entity
                .HasOne(g => g.Course)
                .WithMany(c => c.Groups)
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(g => g.Instructor)
                .WithMany()
                .HasForeignKey(g => g.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(g => g.CreatedBy)
                .WithMany()
                .HasForeignKey(g => g.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(g => g.ApprovedBy)
                .WithMany()
                .HasForeignKey(g => g.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Enrollment>(entity =>
        {
            entity
                .HasOne(e => e.Group)
                .WithMany(g => g.Enrollments)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.GroupId, e.StudentId }).IsUnique();
        });

        builder.Entity<GroupSchedule>(entity =>
        {
            entity
                .HasOne(gs => gs.Group)
                .WithMany(g => g.Schedules)
                .HasForeignKey(gs => gs.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(gs => gs.Location)
                .WithMany(l => l.GroupSchedules)
                .HasForeignKey(gs => gs.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.AddQuartz(q => q.UsePostgreSql());
        builder.Entity<Conversation>(entity =>
        {
            entity
                .HasOne(c => c.Group)
                .WithMany()
                .HasForeignKey(c => c.GroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ConversationMember>(entity =>
        {
            entity.HasKey(m => new { m.ConversationId, m.UserId });
            entity
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Members)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Message>(entity =>
        {
            entity
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(m => new { m.ConversationId, m.SentAt });
        });
        builder.Entity<Attendance>(entity =>
        {
            entity.HasOne(a => a.Group)
                .WithMany()
                .HasForeignKey(a => a.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.GroupId, a.StudentId, a.SessionDate }).IsUnique();
        });

        builder.Entity<StudentPoints>(entity =>
        {
            entity.HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.StudentId).IsUnique();
        });
    }
}
