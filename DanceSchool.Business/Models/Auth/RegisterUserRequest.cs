namespace DanceSchool.Business.Models.Auth
{
    public class RegisterUserRequest
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string Password { get; set; }
    }
}
