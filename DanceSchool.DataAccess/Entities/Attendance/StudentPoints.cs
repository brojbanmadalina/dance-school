using System.ComponentModel.DataAnnotations.Schema;
using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.DataAccess.Entities.Attendances
{
    [Table("student_points")]
    public class StudentPoints
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public User Student { get; set; }

        public int Points { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}