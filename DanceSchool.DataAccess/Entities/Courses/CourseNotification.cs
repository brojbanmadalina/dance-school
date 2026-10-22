using DanceSchool.DataAccess.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanceSchool.DataAccess.Entities.Courses
{
    [Table("course_notifications")]
    public class CourseNotification
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTimeOffset SentAt { get; set; }
    }
}
