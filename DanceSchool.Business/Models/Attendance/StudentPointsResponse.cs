namespace DanceSchool.Business.Models.Attendance
{
    public class StudentPointsResponse
    {
        public Guid StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Points { get; set; }
    }
}
