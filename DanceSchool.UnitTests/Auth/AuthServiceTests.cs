using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Models.Auth;
using DanceSchool.Business.Services.Auth;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Auth;
using DanceSchool.DataAccess.Entities.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DanceSchool.Business.Tests.Services.Auth
{
    public class AuthServiceTests
    {
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILockoutService> _lockoutServiceMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IPasswordResetTokenService> _resetTokenServiceMock;
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

        public AuthServiceTests()
        {
            _jwtServiceMock = new Mock<IJwtService>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _emailServiceMock = new Mock<IEmailService>();
            _lockoutServiceMock = new Mock<ILockoutService>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            _resetTokenServiceMock = new Mock<IPasswordResetTokenService>();
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();
            _dateTimeProviderMock
                .Setup(x => x.UtcNow)
                .Returns(new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
        }

        [Fact]
        public async Task Register_Should_Create_User()
        {
            // Arrange

            var db = CreateDbContext();
            _passwordServiceMock
                .Setup(x => x.Hash(It.IsAny<User>(), "Password123!"))
                .Returns("hashed-password");
            var service = CreateService(db);

            var request = new RegisterUserRequest
            {
                Email = "test@test.com",
                Password = "Password123!",
                FirstName = "Bla",
                LastName = "Bla",
                DateOfBirth = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
            };

            // Act

            var result = await service.Register(request);

            // Assert

            result.IsSuccess.Should().BeTrue();

            var user = await db.Users.FirstOrDefaultAsync();

            user.Should().NotBeNull();
            user!.Email.Should().Be("test@test.com");

            user.CreatedAt.Should().Be(_dateTimeProviderMock.Object.UtcNow);

            user.Password.Should().NotBe(request.Password);
        }

        [Fact]
        public async Task Login_Should_Return_Fail_When_User_Blas_Not_Exist()
        {
            // Arrange

            var db = CreateDbContext();
            var service = CreateService(db);

            var request = new LoginUserRequest
            {
                Email = "smth@test.com",
                Password = "Password123!",
            };

            // Act

            var result = await service.Login(request);

            // Assert

            result.IsFailed.Should().BeTrue();

            result.Errors.Should().Contain(x => x.Message == "Invalid credentials");
        }

        [Fact]
        public async Task Login_Should_Return_AuthResponse_When_Credentials_Are_Valid()
        {
            // Arrange

            var db = CreateDbContext();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Password = "hashed",
                FirstName = "Bla",
                LastName = "Bla",
                DateOfBirth = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Role = Role.Student,
                CreatedAt = _dateTimeProviderMock.Object.UtcNow,
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            _passwordServiceMock
                .Setup(x => x.Verify(user, user.Password, "Password123!"))
                .Returns(true);

            _jwtServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns("access-token");

            _refreshTokenServiceMock
                .Setup(x => x.RotateAsync(user.Id))
                .ReturnsAsync("refresh-token");

            var service = CreateService(db);

            var request = new LoginUserRequest
            {
                Email = "test@test.com",
                Password = "Password123!",
            };

            // Act

            var result = await service.Login(request);

            // Assert

            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().Be("access-token");
            result.Value.RefreshToken.Should().Be("refresh-token");
        }

        [Fact]
        public async Task Login_Should_Return_Fail_When_Account_Is_Locked()
        {
            // Arrange

            var db = CreateDbContext();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "accountLocked@test.com",
                Password = "hashed",
                FirstName = "Bla",
                LastName = "Bla",
                DateOfBirth = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Role = Role.Student,
                CreatedAt = _dateTimeProviderMock.Object.UtcNow,
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            _lockoutServiceMock
                .Setup(x => x.IsLockedOut(It.IsAny<User>()))
                .Returns(true);

            _lockoutServiceMock
                .Setup(x => x.GetRemainingLockout(It.IsAny<User>()))
                .Returns(TimeSpan.FromMinutes(5));

            var service = CreateService(db);

            var request = new LoginUserRequest
            {
                Email = "accountLocked@test.com",
                Password = "Password123!",
            };

            // Act

            var result = await service.Login(request);

            // Assert

            result.IsFailed.Should().BeTrue();

            result.Errors.Should().Contain(x => x.Message.Contains("locked"));
        }

        [Fact]
        public async Task Refresh_Should_Return_Fail_When_Token_Is_Invalid()
        {
            // Arrange

            var db = CreateDbContext();

            _refreshTokenServiceMock
                .Setup(x => x.ValidateAsync("invalid-token"))
                .ReturnsAsync((DataAccess.Entities.Auth.RefreshToken?)null);

            var service = CreateService(db);

            var request = new RefreshTokenRequest { RefreshToken = "invalid-token" };

            // Act

            var result = await service.Refresh(request);

            // Assert

            result.IsFailed.Should().BeTrue();

            result.Errors.Should().Contain(x => x.Message == "Invalid or expired refresh token");
        }

        [Fact]
        public async Task Logout_Should_Revoke_RefreshToken()
        {
            // Arrange

            var db = CreateDbContext();
            var service = CreateService(db);

            var request = new LogoutRequest { RefreshToken = "refresh-token" };

            // Act

            var result = await service.Logout(request);

            // Assert

            result.IsSuccess.Should().BeTrue();

            _refreshTokenServiceMock.Verify(x => x.RevokeAsync("refresh-token"), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_Should_Send_Email_When_User_Exists()
        {
            // Arrange

            var db = CreateDbContext();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Password = "hashed",
                FirstName = "Bla",
                LastName = "Bla",
                DateOfBirth = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Role = Role.Student,
                CreatedAt = _dateTimeProviderMock.Object.UtcNow,
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            _resetTokenServiceMock.Setup(x => x.CreateAsync(user.Id)).ReturnsAsync("reset-token");

            var service = CreateService(db);

            var request = new ForgotPasswordRequest { Email = "test@test.com" };

            // Act

            var result = await service.ForgotPassword(request);

            // Assert

            result.IsSuccess.Should().BeTrue();

            _emailServiceMock.Verify(
                x =>
                    x.SendResetPasswordEmail(
                        "test@test.com",
                        "Bla",
                        It.Is<string>(s => s.Contains("reset-token"))
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_Ok_When_User_Blas_Not_Exist()
        {
            // Arrange

            var db = CreateDbContext();
            var service = CreateService(db);

            var request = new ForgotPasswordRequest { Email = "smth@test.com" };

            // Act

            var result = await service.ForgotPassword(request);

            // Assert

            result.IsSuccess.Should().BeTrue();

            _emailServiceMock.Verify(
                x =>
                    x.SendResetPasswordEmail(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    ),
                Times.Never
            );
        }

        private DanceSchoolDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<DanceSchoolDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new DanceSchoolDbContext(options);
        }

        private AuthService CreateService(DanceSchoolDbContext db)
        {
            return new AuthService(
                db,
                _jwtServiceMock.Object,
                _passwordServiceMock.Object,
                _emailServiceMock.Object,
                _lockoutServiceMock.Object,
                _refreshTokenServiceMock.Object,
                _resetTokenServiceMock.Object,
                _dateTimeProviderMock.Object,
                Microsoft.Extensions.Options.Options.Create(
                    new ForgotPasswordSettings
                    {
                        ResetPasswordUrl = "https://localhost/reset-password",
                    }
                )
            );
        }
    }
}
