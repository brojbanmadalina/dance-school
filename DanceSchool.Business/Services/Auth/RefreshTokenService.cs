using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Auth
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RefreshTokenService(DanceSchoolDbContext db, IDateTimeProvider dateTimeProvider)
        {
            _db = db;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<string> RotateAsync(Guid userId)
        {
            var existingTokens = await _db.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();

            _db.RefreshTokens.RemoveRange(existingTokens);

            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = _dateTimeProvider.UtcNow.AddDays(7),
            };

            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();

            return token.Token;
        }

        public async Task<RefreshToken?> ValidateAsync(string refreshToken)
        {
            var token = await _db
                .RefreshTokens.Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token == null || token.ExpiryDate < _dateTimeProvider.UtcNow)
            {
                return null;
            }

            return token;
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var token = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token != null)
            {
                _db.RefreshTokens.Remove(token);
                await _db.SaveChangesAsync();
            }
        }
    }
}
