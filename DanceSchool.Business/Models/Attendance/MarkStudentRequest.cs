namespace DanceSchool.Business.Models.Attendance
{
    public class MarkStudentRequest
    {
        public DateOnly SessionDate { get; set; }
        public bool IsPresent { get; set; }
    }
}
