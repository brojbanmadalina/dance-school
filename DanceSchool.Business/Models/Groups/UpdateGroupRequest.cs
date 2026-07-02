namespace DanceSchool.Business.Models.Groups
{
    public class UpdateGroupRequest
    {
        public string Level { get; set; }
        public int MaxCapacity { get; set; }
        public Guid InstructorId { get; set; }
    }
}
