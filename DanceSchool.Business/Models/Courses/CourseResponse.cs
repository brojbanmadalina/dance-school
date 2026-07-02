namespace DanceSchool.Business.Models.Courses
{
    public class CourseResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
    }
}
