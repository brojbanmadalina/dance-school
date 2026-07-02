using DanceSchool.DataAccess.Entities.Auth;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanceSchool.DataAccess.Entities.Users
{
    [Table("users")]
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public DateTimeOffset DateOfBirth { get; set; }
        public string Password { get; set; } 
        public Role Role { get; set; } = Role.Student;
        public int FailedLoginAttempts { get; set; } = 0;
        public  DateTimeOffset? LockoutEnd { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
