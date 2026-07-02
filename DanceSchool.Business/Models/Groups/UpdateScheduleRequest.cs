namespace DanceSchool.Business.Models.Groups
{
    public class UpdateScheduleRequest
    {
        public Guid LocationId { get; set; }
        public string Room { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
