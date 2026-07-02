using DanceSchool.DataAccess.Entities.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanceSchool.DataAccess.Entities.Courses
{
    [Table("courses")]
    public class Course
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ApprovalStatus Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedBy { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public User? ApprovedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public ICollection<Group> Groups { get; set; }
    }
}