namespace DanceSchool.Business.Models.Groups
{
    public class ScheduleResponse
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }
        public string Room { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayName { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
