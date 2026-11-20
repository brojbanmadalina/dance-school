using DanceSchool.Business.Interfaces.Common;

namespace DanceSchool.Business.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}