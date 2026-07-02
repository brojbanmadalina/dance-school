using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DanceSchool.Business.Jobs;

public class CourseReminderJob : IJob
{
    private readonly DanceSchoolDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;

    public CourseReminderJob(
        DanceSchoolDbContext db,
        IEmailService emailService,
        IDateTimeProvider dateTime
    )
    {
        _db = db;
        _emailService = emailService;
        _dateTime = dateTime;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var tomorrow = _dateTime.UtcNow.Date.AddDays(1);
        var tomorrowDay = tomorrow.DayOfWeek;

        var groupsTomorrow = await _db
            .Groups.Include(g => g.Course)
            .Include(g => g.Schedules)
            .Include(g => g.Enrollments)
                .ThenInclude(e => e.Student)
            .Where(g =>
                g.Status == ApprovalStatus.Approved
                && g.Schedules.Any(s => s.DayOfWeek == tomorrowDay)
            )
            .ToListAsync();

        foreach (var group in groupsTomorrow)
        {
            var schedule = group.Schedules.First(s => s.DayOfWeek == tomorrowDay);

            foreach (var enrollment in group.Enrollments)
            {
                await _emailService.SendCourseReminderEmail(
                    enrollment.Student.Email,
                    enrollment.Student.FirstName,
                    group.Course.Name,
                    group.Level,
                    schedule.StartTime
                );
                await Task.Delay(2000);
            }
        }
    }
}
