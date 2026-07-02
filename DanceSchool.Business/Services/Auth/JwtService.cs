using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Models.Auth;
using DanceSchool.DataAccess.Entities.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DanceSchool.Business.Services.Auth
{
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _options;
        private readonly IDateTimeProvider _dateTimeProvider;

        public JwtService(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
        {
            _options = options.Value;
            _dateTimeProvider = dateTimeProvider;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _options.Issuer,
                _options.Audience,
                claims,
                expires: _dateTimeProvider.UtcNow.AddMinutes(_options.ExpiryMinutes).UtcDateTime,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
