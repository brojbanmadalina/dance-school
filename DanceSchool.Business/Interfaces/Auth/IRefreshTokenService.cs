using DanceSchool.DataAccess.Entities.Auth;

namespace DanceSchool.Business.Interfaces.Auth
{
    public interface IRefreshTokenService
    {
        Task<string> RotateAsync(Guid userId);
        Task RevokeAsync(string refreshToken);
        Task<RefreshToken?> ValidateAsync(string refreshToken);
    }
}
