using DanceSchool.DataAccess.Entities.Courses;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanceSchool.DataAccess.Entities.Locations
{
    [Table("locations")]
    public class Location
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<GroupSchedule> GroupSchedules { get; set; }
    }
}
