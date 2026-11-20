using DanceSchool.Business.Interfaces;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Models.Attendance;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Attendances;
using DanceSchool.DataAccess.Entities.Courses;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IDateTimeProvider _dateTime;

        public AttendanceService(DanceSchoolDbContext db, IDateTimeProvider dateTime)
        {
            _db = db;
            _dateTime = dateTime;
        }

        public async Task<Result<List<SessionAttendanceResponse>>> GetSessions(
            Guid instructorId,
            DateOnly date,
            Guid? groupId
        )
        {
            var groupsQuery = _db
                .Groups.Include(g => g.Enrollments)
                    .ThenInclude(e => e.Student)
                .Where(g => g.InstructorId == instructorId);

            if (groupId.HasValue)
                groupsQuery = groupsQuery.Where(g => g.Id == groupId.Value);

            var groups = await groupsQuery.ToListAsync();

            if (groupId.HasValue && groups.Count == 0)
                return Result.Fail<List<SessionAttendanceResponse>>(
                    "Group not found or you are not its instructor"
                );

            var groupIds = groups.Select(g => g.Id).ToList();

            var attendances = await _db
                .Attendances.Where(a => groupIds.Contains(a.GroupId) && a.SessionDate == date)
                .ToListAsync();

            var sessions = groups
                .Select(group =>
                {
                    var marks = attendances
                        .Where(a => a.GroupId == group.Id)
                        .ToDictionary(a => a.StudentId, a => a.IsPresent);

                    var entries = group
                        .Enrollments.Select(e => new AttendanceEntryResponse
                        {
                            StudentId = e.StudentId,
                            FirstName = e.Student.FirstName,
                            LastName = e.Student.LastName,
                            IsPresent = marks.TryGetValue(e.StudentId, out var present)
                                ? present
                                : null,
                        })
                        .OrderBy(e => e.FirstName)
                        .ToList();

                    return BuildResponse(group, date, entries);
                })
                .OrderBy(s => s.GroupName)
                .ToList();

            return Result.Ok(sessions);
        }

        public async Task<Result<SessionAttendanceResponse>> SaveSession(
            Guid currentUserId,
            SaveAttendanceRequest request
        )
        {
            var studentIds = await ValidateSaveSession(currentUserId, request);
            if (studentIds.IsFailed)
                return studentIds.ToResult<SessionAttendanceResponse>();

            var enrolledStudentIds = studentIds.Value;
            var presentStudentIds = request.PresentStudentIds.Distinct().ToList();
            var now = _dateTime.UtcNow;

            _db.Attendances.AddRange(
                enrolledStudentIds.Select(studentId => new Attendance
                {
                    Id = Guid.NewGuid(),
                    GroupId = request.GroupId,
                    StudentId = studentId,
                    SessionDate = request.SessionDate,
                    IsPresent = presentStudentIds.Contains(studentId),
                    MarkedByUserId = currentUserId,
                    MarkedAt = now,
                })
            );

            await AddPointsForPresence(presentStudentIds, now);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Result.Fail<SessionAttendanceResponse>(
                    "Attendance for this session has already been saved"
                );
            }

            var result = await GetSessions(currentUserId, request.SessionDate, request.GroupId);
            return Result.Ok(result.Value.First());
        }

        public async Task<Result<StudentPointsResponse>> GetPoints(Guid studentId)
        {
            var student = await _db.Users.FirstOrDefaultAsync(u => u.Id == studentId);
            if (student == null)
                return Result.Fail<StudentPointsResponse>("Student not found");

            var points = await _db.StudentPoints.FirstOrDefaultAsync(p => p.StudentId == studentId);

            return Result.Ok(
                new StudentPointsResponse
                {
                    StudentId = studentId,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Points = points?.Points ?? 0,
                }
            );
        }

        private async Task<Result<List<Guid>>> ValidateSaveSession(
            Guid currentUserId,
            SaveAttendanceRequest request
        )
        {
            var instructorId = await _db
                .Groups.Where(g => g.Id == request.GroupId)
                .Select(g => (Guid?)g.InstructorId)
                .SingleOrDefaultAsync();

            if (instructorId == null)
                return Result.Fail("Group not found");

            if (instructorId != currentUserId)
                return Result.Fail("You are not the instructor of this group");

            var alreadySaved = await _db.Attendances.AnyAsync(a =>
                a.GroupId == request.GroupId && a.SessionDate == request.SessionDate
            );

            if (alreadySaved)
                return Result.Fail("Attendance for this session has already been saved");

            var enrolledStudentIds = await _db
                .Enrollments.Where(e => e.GroupId == request.GroupId)
                .Select(e => e.StudentId)
                .ToListAsync();

            if (request.PresentStudentIds.Any(id => !enrolledStudentIds.Contains(id)))
                return Result.Fail("A present student is not enrolled in this group");

            return Result.Ok(enrolledStudentIds);
        }

        private async Task AddPointsForPresence(List<Guid> presentStudentIds, DateTimeOffset now)
        {
            var existingPoints = await _db
                .StudentPoints.Where(p => presentStudentIds.Contains(p.StudentId))
                .ToListAsync();

            foreach (var points in existingPoints)
            {
                points.Points += 1;
                points.UpdatedAt = now;
            }

            var studentsWithoutPoints = presentStudentIds.Except(existingPoints.Select(p => p.StudentId));

            _db.StudentPoints.AddRange(
                studentsWithoutPoints.Select(studentId => new StudentPoints
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    Points = 1,
                    UpdatedAt = now,
                })
            );
        }

        private static SessionAttendanceResponse BuildResponse(
            Group group,
            DateOnly date,
            List<AttendanceEntryResponse> entries
        ) =>
            new()
            {
                GroupId = group.Id,
                GroupName = group.Course.Name,
                SessionDate = date,
                PresentCount = entries.Count(e => e.IsPresent == true),
                TotalCount = entries.Count,
                Entries = entries,
            };
    }
}
