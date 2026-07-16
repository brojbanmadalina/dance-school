namespace DanceSchool.Business.Models.Attendance
{
    public class AttendanceEntryResponse
    {
        public Guid StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? IsPresent { get; set; }
    }
}
