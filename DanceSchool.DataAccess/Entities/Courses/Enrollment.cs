using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.DataAccess.Entities.Courses
{
    public class Enrollment
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Group Group { get; set; }
        public Guid StudentId { get; set; }
        public User Student { get; set; }
        public DateTimeOffset EnrolledAt { get; set; }
    }
}
