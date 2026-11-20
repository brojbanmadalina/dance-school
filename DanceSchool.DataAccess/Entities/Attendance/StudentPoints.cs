using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.DataAccess.Entities.Attendances
{
    public class StudentPoints
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public User Student { get; set; }

        public int Points { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}