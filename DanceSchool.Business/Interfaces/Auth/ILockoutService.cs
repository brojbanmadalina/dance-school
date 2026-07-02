using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.Business.Interfaces.Auth
{
    public interface ILockoutService
    {
        bool IsLockedOut(User user);
        TimeSpan? GetRemainingLockout(User user);
        void RegisterFailedAttempt(User user);
        void RegisterSuccessfulLogin(User user);
    }
}
