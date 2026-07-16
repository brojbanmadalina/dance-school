namespace DanceSchool.Business.Models.Attendance
{
    public class AttendanceMarkRequest
    {
        public Guid StudentId { get; set; }
        public bool IsPresent { get; set; }
    }
}
