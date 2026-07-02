namespace DanceSchool.Business.Models.Courses
{
    public class GroupResponse
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public Guid InstructorId { get; set; }
        public string Level { get; set; }
        public string Room { get; set; }
        public string Location { get; set; }
        public int MaxCapacity { get; set; }
        public string Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
    }
}
