using DanceSchool.Business.Constants;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Courses;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Courses
{
    public class CourseService : ICourseService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IUserProvider _userProvider;
        private readonly IValidator<CreateCourseRequest> _createCourseValidator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CourseService(
            DanceSchoolDbContext db,
            IUserProvider userProvider,
            IValidator<CreateCourseRequest> createCourseValidator,
            IDateTimeProvider dateTimeProvider)
        {
            _db = db;
            _userProvider = userProvider;
            _createCourseValidator = createCourseValidator;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<CourseResponse>> CreateCourse(CreateCourseRequest request)
        {
            var validation = await _createCourseValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return Result.Fail<CourseResponse>(validation.Errors.Select(e => new Error(e.ErrorMessage)).ToList());

            var userId = _userProvider.UserId ?? throw new UnauthorizedAccessException();
            var isAdmin = _userProvider.Role == UserRole.Admin;

            var course = new Course
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Status = isAdmin ? ApprovalStatus.Approved : ApprovalStatus.Pending,
                CreatedByUserId = userId,
                ApprovedByUserId = isAdmin ? userId : null,
                ApprovedAt = isAdmin ? _dateTimeProvider.UtcNow : null,
                CreatedAt = _dateTimeProvider.UtcNow,
            };

            _db.Courses.Add(course);
            await _db.SaveChangesAsync();

            return Result.Ok(MapToResponse(course));
        }

        public async Task<Result<List<CourseResponse>>> GetCourses()
        {
            var userId = _userProvider.UserId ?? throw new UnauthorizedAccessException();
            var userRole = _userProvider.Role;

            var query = _db.Courses.AsQueryable();

            if (userRole == UserRole.Instructor)
                query = query.Where(c => c.CreatedByUserId == userId);
            else if (userRole != UserRole.Admin)
                query = query.Where(c => c.Status == ApprovalStatus.Approved);

            var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return Result.Ok(courses.Select(MapToResponse).ToList());
        }

        public async Task<Result<CourseResponse>> GetCourse(Guid courseId)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                return Result.Fail<CourseResponse>("Course not found");

            return Result.Ok(MapToResponse(course));
        }

        public async Task<Result<CourseResponse>> UpdateCourse(Guid courseId, UpdateCourseRequest request)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                return Result.Fail<CourseResponse>("Course not found");

            course.Name = request.Name;
            course.Description = request.Description;
            await _db.SaveChangesAsync();

            return Result.Ok(MapToResponse(course));
        }

        public async Task<Result> DeleteCourse(Guid courseId)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                return Result.Fail("Course not found");

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> ApproveCourse(Guid courseId)
        {
            var adminId = _userProvider.UserId ?? throw new UnauthorizedAccessException();
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return Result.Fail("Course not found");
            if (course.Status != ApprovalStatus.Pending)
                return Result.Fail("Only pending courses can be approved");

            course.Status = ApprovalStatus.Approved;
            course.ApprovedByUserId = adminId;
            course.ApprovedAt = _dateTimeProvider.UtcNow;
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> RejectCourse(Guid courseId)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return Result.Fail("Course not found");
            if (course.Status != ApprovalStatus.Pending)
                return Result.Fail("Only pending courses can be rejected");

            course.Status = ApprovalStatus.Rejected;
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        private static CourseResponse MapToResponse(Course course) => new()
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Status = course.Status.ToString(),
            CreatedByUserId = course.CreatedByUserId,
            ApprovedByUserId = course.ApprovedByUserId,
            CreatedAt = course.CreatedAt,
            ApprovedAt = course.ApprovedAt,
        };
    }
}