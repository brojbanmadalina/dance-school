namespace DanceSchool.Business.Models.Courses
{
    public class CreateGroupRequest
    {
        public Guid CourseId { get; set; }
        public Guid InstructorId { get; set; }
        public string Level { get; set; }
        public string Room { get; set; }
        public string Location { get; set; }
        public int MaxCapacity { get; set; }
    }
}
