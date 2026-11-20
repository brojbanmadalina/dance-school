namespace DanceSchool.Business.Models.Attendance
{
    public class SaveAttendanceRequest
    {
        public Guid GroupId { get; set; }
        public DateOnly SessionDate { get; set; }
        public List<Guid> PresentStudentIds { get; set; } = new();
    }
}
