namespace DanceSchool.Business.Models.Attendance
{
    public class SessionAttendanceResponse
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public DateOnly SessionDate { get; set; }
        public int PresentCount { get; set; }
        public int TotalCount { get; set; }
        public List<AttendanceEntryResponse> Entries { get; set; } = new();
    }
}
