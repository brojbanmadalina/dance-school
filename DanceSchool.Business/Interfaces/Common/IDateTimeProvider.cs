namespace DanceSchool.Business.Interfaces.Common
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}