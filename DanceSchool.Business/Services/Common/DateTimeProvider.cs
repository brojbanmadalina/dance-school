using DanceSchool.Business.Interfaces.Common;

namespace DanceSchool.Business.Services.Common
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}