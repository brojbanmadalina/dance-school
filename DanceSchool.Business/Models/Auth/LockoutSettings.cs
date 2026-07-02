namespace DanceSchool.Business.Models.Auth
{
    public class LockoutSettings
    {
        public int MaxAttempts { get; set; }
        public int LockoutMinutes { get; set; }
    }
}
