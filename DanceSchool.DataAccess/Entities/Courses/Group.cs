using DanceSchool.DataAccess.Entities.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanceSchool.DataAccess.Entities.Courses
{
    [Table("groups")]
    public class Group
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
        public Guid InstructorId { get; set; }
        public User Instructor { get; set; }
        public string Level { get; set; }
        public int MaxCapacity { get; set; }
        public ApprovalStatus Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedBy { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public User? ApprovedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<GroupSchedule> Schedules { get; set; }
    }
}