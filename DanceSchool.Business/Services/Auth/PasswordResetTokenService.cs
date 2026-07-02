using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Auth
{
    public class PasswordResetTokenService : IPasswordResetTokenService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IDateTimeProvider _dateTimeProvider;

        public PasswordResetTokenService(
            DanceSchoolDbContext db,
            IDateTimeProvider dateTimeProvider
        )
        {
            _db = db;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<string> CreateAsync(Guid userId)
        {
            var existingTokens = await _db
                .PasswordResetTokens.Where(x => x.UserId == userId && !x.IsUsed)
                .ToListAsync();

            _db.PasswordResetTokens.RemoveRange(existingTokens);

            var token = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = _dateTimeProvider.UtcNow.AddMinutes(30),
                IsUsed = false,
            };

            _db.PasswordResetTokens.Add(token);
            await _db.SaveChangesAsync();

            return token.Token;
        }

        public async Task<PasswordResetToken?> ValidateAsync(string token)
        {
            var resetToken = await _db
                .PasswordResetTokens.Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (
                resetToken == null
                || resetToken.IsUsed
                || resetToken.ExpiryDate < _dateTimeProvider.UtcNow
            )
            {
                return null;
            }

            return resetToken;
        }

        public async Task MarkUsedAsync(PasswordResetToken token)
        {
            token.IsUsed = true;
            await _db.SaveChangesAsync();
        }
    }
}
