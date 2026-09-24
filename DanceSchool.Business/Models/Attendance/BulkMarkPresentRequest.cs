namespace DanceSchool.Business.Models.Attendance
{
    public class BulkMarkPresentRequest
    {
        public DateOnly SessionDate { get; set; }
        public List<Guid> StudentIds { get; set; } = new();
    }
}
