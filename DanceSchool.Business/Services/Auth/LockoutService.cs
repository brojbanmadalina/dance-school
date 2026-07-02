using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Models.Auth;
using DanceSchool.DataAccess.Entities.Users;
using Microsoft.Extensions.Options;

namespace DanceSchool.Business.Services.Auth
{
    public class LockoutService: ILockoutService
    {
        private readonly LockoutSettings _settings;
        private readonly IDateTimeProvider _dateTimeProvider;

        public LockoutService(IOptions<LockoutSettings> settings, IDateTimeProvider dateTimeProvider)
        {
            _settings = settings.Value;
            _dateTimeProvider = dateTimeProvider;
        }

        public bool IsLockedOut(User user)
        {
            if (user.LockoutEnd == null)
            {
                return false;
            }

            return user.LockoutEnd > _dateTimeProvider.UtcNow;
        }

        public TimeSpan? GetRemainingLockout(User user)
        {
            if (!IsLockedOut(user))
            {
                return null;
            }

            return user.LockoutEnd!.Value - _dateTimeProvider.UtcNow;
        }

        public void RegisterFailedAttempt(User user)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= _settings.MaxAttempts)
            {
                user.LockoutEnd = _dateTimeProvider.UtcNow.AddMinutes(_settings.LockoutMinutes);
            }
        }

        public void RegisterSuccessfulLogin(User user)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
        }
    }
}
