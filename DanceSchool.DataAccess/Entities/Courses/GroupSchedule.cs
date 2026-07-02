using DanceSchool.DataAccess.Entities.Locations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanceSchool.DataAccess.Entities.Courses
{
    [Table("group_schedules")]
    public class GroupSchedule
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Group Group { get; set; }
        public Guid LocationId { get; set; }
        public Location Location { get; set; }
        public string Room { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
