using DanceSchool.DataAccess.Entities.Courses;

namespace DanceSchool.DataAccess.Entities.Locations
{
    public class Location
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<GroupSchedule> GroupSchedules { get; set; }
    }
}
