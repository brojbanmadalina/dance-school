namespace DanceSchool.Business.Models.Groups
{
    public class EnrollStudentRequest
    {
        public Guid GroupId { get; set; }
        public Guid StudentId { get; set; }
    }
}
