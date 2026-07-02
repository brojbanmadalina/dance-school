namespace DanceSchool.Business.Models.Groups
{
    public class EnrollmentResponse
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentEmail { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public DateTimeOffset EnrolledAt { get; set; }
    }
}
