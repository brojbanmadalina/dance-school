using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.Business.Interfaces.Auth
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
    }
}
