using DanceSchool.Business.Interfaces.Attendances;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Models.Attendance;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Attendances;
using DanceSchool.DataAccess.Entities.Courses;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Attendances
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
            var group = await _db
                .Groups.Include(g => g.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId);

            if (group == null)
                return Result.Fail<SessionAttendanceResponse>("Group not found");

            if (group.InstructorId != currentUserId)
                return Result.Fail<SessionAttendanceResponse>(
                    "You are not the instructor of this group"
                );

            var enrolledIds = group.Enrollments.Select(e => e.StudentId).ToHashSet();

            if (request.Marks.Any(m => !enrolledIds.Contains(m.StudentId)))
                return Result.Fail<SessionAttendanceResponse>(
                    "A mark references for a student not enrolled in this group"
                );

            var existingPresence = await _db
                .Attendances.Where(a =>
                    a.GroupId == request.GroupId && a.SessionDate == request.SessionDate
                )
                .ToListAsync();

            foreach (var mark in request.Marks)
            {
                var row = existingPresence.FirstOrDefault(a => a.StudentId == mark.StudentId);

                if (row == null)
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
                            MarkedAt = _dateTime.UtcNow,
                        }
                    );

                    if (mark.IsPresent)
                        await AdjustPoints(mark.StudentId, +1);
                }
                else if (row.IsPresent != mark.IsPresent)
                {
                    await AdjustPoints(mark.StudentId, mark.IsPresent ? +1 : -1);

                    row.IsPresent = mark.IsPresent;
                    row.MarkedByUserId = currentUserId;
                    row.MarkedAt = _dateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();

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

        private async Task AdjustPoints(Guid studentId, int delta)
        {
            var row = await _db.StudentPoints.FirstOrDefaultAsync(p => p.StudentId == studentId);

            if (row == null)
            {
                row = new StudentPoints
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    Points = 0,
                    UpdatedAt = _dateTime.UtcNow,
                };
                _db.StudentPoints.Add(row);
            }

            row.Points = Math.Max(0, row.Points + delta);
            row.UpdatedAt = _dateTime.UtcNow;
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
