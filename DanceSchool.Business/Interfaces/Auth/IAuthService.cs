using DanceSchool.Business.Models.Auth;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<Result> Register(RegisterUserRequest request);
        Task<Result<AuthResponse>> Login(LoginUserRequest request);
        Task<Result<AuthResponse>> Refresh(RefreshTokenRequest request);
        Task<Result> ForgotPassword(ForgotPasswordRequest request);
        Task<Result> ResetPassword(ResetPasswordRequest request);
        Task<Result> Logout(LogoutRequest request);
    }
}
