using DanceSchool.DataAccess.Entities.Auth;

namespace DanceSchool.Business.Interfaces.Auth
{
    public interface IPasswordResetTokenService
    {
        Task<string> CreateAsync(Guid userId);
        Task<PasswordResetToken?> ValidateAsync(string token);
        Task MarkUsedAsync(PasswordResetToken token);
    }
}
