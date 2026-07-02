using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.Business.Interfaces.Auth
{
    public interface IPasswordService
    {
        string Hash(User user, string password);
        bool Verify(User user, string hash, string password);
    }
}
