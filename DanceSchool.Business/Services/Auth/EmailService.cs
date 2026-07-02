using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Models.Auth;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace DanceSchool.Business.Services.Auth
{
    public class EmailService: IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly EmailTemplateService _templateService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public EmailService(IOptions<EmailSettings> options, EmailTemplateService templateService, IDateTimeProvider dateTimeProvider)
        {
            _settings = options.Value;
            _templateService = templateService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task SendResetPasswordEmail(string toEmail, string firstName, string link)
        {
            var model = new
            {
                first_name = firstName,
                reset_link = link,
                year = _dateTimeProvider.UtcNow.Year
            };

            var body = await _templateService.RenderAsync("reset-password.liquid", model);

            var message = new MailMessage();
            message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
            message.To.Add(toEmail);
            message.Subject = "Reset your password";
            message.Body = body;
            message.IsBodyHtml = true;

            var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }

        public async Task SendCourseReminderEmail(string toEmail, string firstName, string courseName, string level, TimeOnly startTime)
        {
            var model = new
            {
                first_name = firstName,
                course_name = courseName,
                level = level,
                start_time = startTime.ToString("HH:mm"),
                year = _dateTimeProvider.UtcNow.Year
            };

            var body = await _templateService.RenderAsync("course-reminder.liquid", model);

            var message = new MailMessage();
            message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
            message.To.Add(toEmail);
            message.Subject = $"Reminder: {courseName} - {level} mâine la {startTime:HH:mm}";
            message.Body = body;
            message.IsBodyHtml = true;

            var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}