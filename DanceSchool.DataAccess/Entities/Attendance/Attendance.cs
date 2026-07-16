using System.ComponentModel.DataAnnotations.Schema;
using DanceSchool.DataAccess.Entities.Courses;
using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.DataAccess.Entities.Attendances
{
    [Table("attendances")]
    public class Attendance
    {
        public Guid Id { get; set; }

        public Guid GroupId { get; set; }
        public Group Group { get; set; }

        public Guid StudentId { get; set; }
        public User Student { get; set; }

        public DateOnly SessionDate { get; set; }

        public bool IsPresent { get; set; }

        public Guid MarkedByUserId { get; set; }
        public DateTimeOffset MarkedAt { get; set; }
    }
}