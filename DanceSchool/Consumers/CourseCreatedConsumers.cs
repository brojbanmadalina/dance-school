using DanceSchool.Business.Events;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Auth;
using DanceSchool.DataAccess.Entities.Courses;
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

        public CourseCreatedConsumer(
            DanceSchoolDbContext db,
            IEmailService emailService,
            ILogger<CourseCreatedConsumer> logger)
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

            // users that have already been notified about this course
            var alreadyNotified = (await _db.CourseNotifications
                    .Where(n => n.CourseId == course.Id)
                    .Select(n => n.UserId)
                    .ToListAsync())
                .ToHashSet();

            var sentCount = 0;
            var skippedCount = 0;

            foreach (var user in recipients)
            {
                if (alreadyNotified.Contains(user.Id))
                {
                    skippedCount++;
                    continue;
                }

                try
                {
                    await _emailService.SendNewCourseEmail(user.Email, user.FirstName, course.Name);

                    // we save it after sending the email to avoid sending multiple emails in case of failure
                    _db.CourseNotifications.Add(new CourseNotification
                    {
                        Id = Guid.NewGuid(),
                        CourseId = course.Id,
                        UserId = user.Id,
                        SentAt = DateTimeOffset.UtcNow,
                    });
                    await _db.SaveChangesAsync();

                    sentCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send new-course email to {Email}", user.Email);
                }
            }

            _logger.LogInformation(
                "Course {CourseId}: sent {Sent} emails, skipped {Skipped} already notified, {Total} recipients total.",
                course.Id, sentCount, skippedCount, recipients.Count);
        }
    }
}