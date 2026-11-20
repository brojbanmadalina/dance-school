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
            var validation = await ValidateSaveSession(currentUserId, request);
            if (validation.IsFailed)
                return validation.ToResult<SessionAttendanceResponse>();

            var now = _dateTime.UtcNow;
            var studentIds = request.Marks.Select(m => m.StudentId).ToList();

            var existingAttendances = await _db
                .Attendances.Where(a =>
                    a.GroupId == request.GroupId
                    && a.SessionDate == request.SessionDate
                    && studentIds.Contains(a.StudentId)
                )
                .ToDictionaryAsync(a => a.StudentId);

            var pointsByStudent = await _db
                .StudentPoints.Where(p => studentIds.Contains(p.StudentId))
                .ToDictionaryAsync(p => p.StudentId);

            foreach (var mark in request.Marks)
            {
                existingAttendances.TryGetValue(mark.StudentId, out var attendance);
                var pointsChange = CalculatePointsChange(attendance?.IsPresent, mark.IsPresent);

                if (attendance == null)
                {
                    _db.Attendances.Add(
                        new Attendance
                        {
                            Id = Guid.NewGuid(),
                            GroupId = request.GroupId,
                            StudentId = mark.StudentId,
                            SessionDate = request.SessionDate,
                            IsPresent = mark.IsPresent,
                            MarkedByUserId = currentUserId,
                            MarkedAt = now,
                        }
                    );
                }
                else if (attendance.IsPresent != mark.IsPresent)
                {
                    attendance.IsPresent = mark.IsPresent;
                    attendance.MarkedByUserId = currentUserId;
                    attendance.MarkedAt = now;
                }

                if (pointsChange != 0)
                    ApplyPointsChange(pointsByStudent, mark.StudentId, pointsChange, now);
            }

            await _db.SaveChangesAsync();

            var result = await GetSessions(currentUserId, request.SessionDate, request.GroupId);
            return Result.Ok(result.Value.First());
        }

        public async Task<Result<SessionAttendanceResponse>> MarkStudent(
            Guid instructorId,
            Guid groupId,
            Guid studentId,
            MarkStudentRequest request
        )
        {
            var request_ = new SaveAttendanceRequest
            {
                GroupId = groupId,
                SessionDate = request.SessionDate,
                Marks = new List<AttendanceMarkRequest>
                {
                    new() { StudentId = studentId, IsPresent = request.IsPresent }
                }
            };

            return await SaveSession(instructorId, request_);
        }

        public async Task<Result<SessionAttendanceResponse>> BulkMarkPresent(
            Guid instructorId,
            Guid groupId,
            BulkMarkPresentRequest request
        )
        {
            var request_ = new SaveAttendanceRequest
            {
                GroupId = groupId,
                SessionDate = request.SessionDate,
                Marks = request.StudentIds
                    .Select(id => new AttendanceMarkRequest { StudentId = id, IsPresent = true })
                    .ToList()
            };

            return await SaveSession(instructorId, request_);
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

        private async Task<Result> ValidateSaveSession(
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

            if (request.Marks.Select(m => m.StudentId).Distinct().Count() != request.Marks.Count)
                return Result.Fail("A student appears more than once in the request");

            var studentIds = request.Marks.Select(m => m.StudentId).ToList();

            var enrolledCount = await _db
                .Enrollments.Where(e =>
                    e.GroupId == request.GroupId && studentIds.Contains(e.StudentId)
                )
                .CountAsync();

            if (enrolledCount != studentIds.Count)
                return Result.Fail("A mark references a student not enrolled in this group");

            return Result.Ok();
        }

        private static int CalculatePointsChange(bool? previousIsPresent, bool isPresent)
        {
            if (previousIsPresent == isPresent)
                return 0;

            if (previousIsPresent is null)
                return isPresent ? 1 : 0;

            return isPresent ? 1 : -1;
        }

        private void ApplyPointsChange(
            Dictionary<Guid, StudentPoints> pointsByStudent,
            Guid studentId,
            int pointsChange,
            DateTimeOffset now
        )
        {
            if (!pointsByStudent.TryGetValue(studentId, out var points))
            {
                points = new StudentPoints
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    Points = 0,
                };
                _db.StudentPoints.Add(points);
                pointsByStudent[studentId] = points;
            }

            points.Points = Math.Max(0, points.Points + pointsChange);
            points.UpdatedAt = now;
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
