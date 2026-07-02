using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.DataAccess.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace DanceSchool.Business.Services.Auth
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string Hash(User user, string password)
        {
            return _hasher.HashPassword(user, password);
        }

        public bool Verify(User user, string hash, string password)
        {
            var result = _hasher.VerifyHashedPassword(user, hash, password);

            if (result == PasswordVerificationResult.Failed)
                return false;

            return true;
        }
    }
}
