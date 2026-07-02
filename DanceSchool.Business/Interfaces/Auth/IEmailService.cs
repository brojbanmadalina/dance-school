namespace DanceSchool.Business.Interfaces.Auth
{
    public interface IEmailService
    {
        Task SendResetPasswordEmail(string toEmail, string firstName, string link);
        Task SendCourseReminderEmail(string email, string studentName, string courseName, string level, TimeOnly startTime);

    }
}
