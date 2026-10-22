using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanceSchool.Business.Models.Profile
{
    public class UserEnrollmentResponse
    {
        public Guid EnrollmentId { get; set; }
        public DateTimeOffset EnrolledAt { get; set; }

        public Guid GroupId { get; set; }
        public string GroupLevel { get; set; }

        public Guid CourseId { get; set; }
        public string CourseName { get; set; }

        public string InstructorName { get; set; }

    }
}
