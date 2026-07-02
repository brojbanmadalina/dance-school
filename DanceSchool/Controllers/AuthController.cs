using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            var result = await _authService.Register(request);

            return result.ToActionResult();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserRequest request)
        {
            var result = await _authService.Login(request);

            return result.ToActionResult(onFail: r =>
            {
                if (r.Errors.Any(e => e.Message.Contains("locked", StringComparison.OrdinalIgnoreCase)))
                {
                    return StatusCode(429, new { message = r.Errors.First().Message });
                }

                return Unauthorized(new { message = r.Errors.First().Message });
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.Refresh(request);

            return result.ToActionResult(onFail: r =>
                Unauthorized(new { message = r.Errors.First().Message })
            );
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPassword(request);

            return result.ToActionResult();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _authService.ResetPassword(request);

            return result.ToActionResult();
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var result = await _authService.Logout(request);

            return result.ToActionResult();
        }
    }
}
