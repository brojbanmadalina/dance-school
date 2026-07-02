using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.DataAccess.Entities.Auth
{

    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
    }

}
