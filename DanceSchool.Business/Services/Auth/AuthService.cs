using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Models.Auth;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Auth;
using DanceSchool.DataAccess.Entities.Users;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DanceSchool.Business.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly ILockoutService _lockoutService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IPasswordResetTokenService _resetTokenService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ForgotPasswordSettings _forgotPasswordSettings;

        public AuthService(
            DanceSchoolDbContext db,
            IJwtService jwtService,
            IPasswordService passwordService,
            IEmailService emailService,
            ILockoutService lockoutService,
            IRefreshTokenService refreshTokenService,
            IPasswordResetTokenService resetTokenService,
            IDateTimeProvider dateTimeProvider,
            IOptions<ForgotPasswordSettings> options)
        {
            _db = db;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _emailService = emailService;
            _lockoutService = lockoutService;
            _refreshTokenService = refreshTokenService;
            _resetTokenService = resetTokenService;
            _dateTimeProvider = dateTimeProvider;
            _forgotPasswordSettings = options.Value;
        }

        public async Task<Result> Register(RegisterUserRequest request)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth.ToUniversalTime(),
                Role = Role.Student,
                CreatedAt = _dateTimeProvider.UtcNow,
            };

            user.Password = _passwordService.Hash(user, request.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result<AuthResponse>> Login(LoginUserRequest request)
        {
            var user = await GetUserByEmail(request.Email);

            if (user == null)
            {
                return Result.Fail<AuthResponse>("Invalid credentials");
            }

            var lockoutResult = CheckLockout(user);

            if (lockoutResult != null)
            {
                return lockoutResult;
            }

            if (!_passwordService.Verify(user, user.Password, request.Password))
            {
                return await HandleFailedLogin(user);
            }

            _lockoutService.RegisterSuccessfulLogin(user);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.RotateAsync(user.Id);

            return Result.Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            });
        }

        public async Task<Result<AuthResponse>> Refresh(RefreshTokenRequest request)
        {
            var token = await _refreshTokenService.ValidateAsync(request.RefreshToken);

            if (token == null)
            {
                return Result.Fail<AuthResponse>("Invalid or expired refresh token");
            }

            var accessToken = _jwtService.GenerateAccessToken(token.User);
            var newRefreshToken = await _refreshTokenService.RotateAsync(token.UserId);

            return Result.Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
            });
        }

        public async Task<Result> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await GetUserByEmail(request.Email);

            if (user == null)
            {
                return Result.Ok();
            }

            var token = await _resetTokenService.CreateAsync(user.Id);
            var link = $"{_forgotPasswordSettings.ResetPasswordUrl}?token={token}";

            await _emailService.SendResetPasswordEmail(user.Email, user.FirstName, link);

            return Result.Ok();
        }

        public async Task<Result> ResetPassword(ResetPasswordRequest request)
        {
            var token = await _resetTokenService.ValidateAsync(request.Token);

            if (token == null)
            {
                return Result.Fail("Invalid or expired token");
            }

            token.User.Password = _passwordService.Hash(token.User, request.NewPassword);
            await _resetTokenService.MarkUsedAsync(token);

            return Result.Ok();
        }

        public async Task<Result> Logout(LogoutRequest request)
        {
            await _refreshTokenService.RevokeAsync(request.RefreshToken);

            return Result.Ok();
        }

        private async Task<User?> GetUserByEmail(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        private Result<AuthResponse>? CheckLockout(User user)
        {
            if (!_lockoutService.IsLockedOut(user))
            {
                return null;
            }

            var remaining = _lockoutService.GetRemainingLockout(user)!.Value;
            var minutes = (int)Math.Ceiling(remaining.TotalMinutes);

            return Result
                .Fail<AuthResponse>($"Account locked. Try again in {minutes} minute(s).")
                .WithError(new Error("ACCOUNT_LOCKED"));
        }

        private async Task<Result<AuthResponse>> HandleFailedLogin(User user)
        {
            _lockoutService.RegisterFailedAttempt(user);
            await _db.SaveChangesAsync();

            return CheckLockout(user)
                ?? Result.Fail<AuthResponse>("Invalid credentials");
        }
    }
}