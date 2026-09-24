using DanceSchool.Business.Events;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Auth;
using DanceSchool.DataAccess.Entities.Users;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Api.Consumers
{
    public class CourseCreatedConsumer : IConsumer<CourseCreatedEvent>
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<CourseCreatedConsumer> _logger;

        public CourseCreatedConsumer(DanceSchoolDbContext db, IEmailService emailService, ILogger<CourseCreatedConsumer> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CourseCreatedEvent> context)
        {
            var msg = context.Message;

            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == msg.CourseId);
            if (course is null)
            {
                _logger.LogWarning("Course {CourseId} not found, skip notification.", msg.CourseId);
                return;
            }

            var instructor = await _db.Users.FirstOrDefaultAsync(u => u.Id == course.CreatedByUserId);
            var students = await _db.Users.Where(u => u.Role == Role.Student).ToListAsync();

            var recipients = new List<User>();
            if (instructor is not null) recipients.Add(instructor);
            recipients.AddRange(students);

            foreach (var user in recipients)
            {
                try
                {
                    await _emailService.SendNewCourseEmail(user.Email, user.FirstName, course.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send new-course email to {Email}", user.Email);
                }
            }

            _logger.LogInformation("Notified {Count} users about course {CourseId}", recipients.Count, course.Id);
        }
    }
}