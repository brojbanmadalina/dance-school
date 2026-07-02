using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Courses;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Courses
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IDateTimeProvider _dateTimeProvider;

        public EnrollmentService(DanceSchoolDbContext db, IDateTimeProvider dateTimeProvider)
        {
            _db = db;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<EnrollmentResponse>> EnrollStudent(EnrollStudentRequest request)
        {
            var group = await _db.Groups
                .Include(g => g.Enrollments)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId);

            if (group == null)
                return Result.Fail<EnrollmentResponse>("Group not found");

            if (group.Status != ApprovalStatus.Approved)
                return Result.Fail<EnrollmentResponse>("Cannot enroll in a non-approved group");

            if (group.Enrollments.Count >= group.MaxCapacity)
                return Result.Fail<EnrollmentResponse>("Group is full");

            var alreadyEnrolled = group.Enrollments.Any(e => e.StudentId == request.StudentId);
            if (alreadyEnrolled)
                return Result.Fail<EnrollmentResponse>("Student is already enrolled in this group");

            var studentExists = await _db.Users.AnyAsync(u => u.Id == request.StudentId);
            if (!studentExists)
                return Result.Fail<EnrollmentResponse>("Student not found");

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                GroupId = request.GroupId,
                StudentId = request.StudentId,
                EnrolledAt = _dateTimeProvider.UtcNow,
            };

            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();

            var student = await _db.Users.FirstAsync(u => u.Id == request.StudentId);

            return Result.Ok(MapToResponse(enrollment, student.Email, student.FirstName, student.LastName));
        }

        public async Task<Result<List<EnrollmentResponse>>> GetGroupStudents(Guid groupId)
        {
            var group = await _db.Groups.AnyAsync(g => g.Id == groupId);
            if (!group)
                return Result.Fail<List<EnrollmentResponse>>("Group not found");

            var enrollments = await _db.Enrollments
                .Include(e => e.Student)
                .Where(e => e.GroupId == groupId)
                .OrderBy(e => e.EnrolledAt)
                .ToListAsync();

            var responses = enrollments.Select(e =>
                MapToResponse(e, e.Student.Email, e.Student.FirstName, e.Student.LastName)
            ).ToList();

            return Result.Ok(responses);
        }

        public async Task<Result> RemoveStudent(Guid groupId, Guid studentId)
        {
            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.GroupId == groupId && e.StudentId == studentId);

            if (enrollment == null)
                return Result.Fail("Enrollment not found");

            _db.Enrollments.Remove(enrollment);
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        private static EnrollmentResponse MapToResponse(
            Enrollment enrollment, string email, string firstName, string lastName) => new()
            {
                Id = enrollment.Id,
                GroupId = enrollment.GroupId,
                StudentId = enrollment.StudentId,
                StudentEmail = email,
                StudentFirstName = firstName,
                StudentLastName = lastName,
                EnrolledAt = enrollment.EnrolledAt,
            };
    }
}