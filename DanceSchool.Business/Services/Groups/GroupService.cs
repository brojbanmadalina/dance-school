using DanceSchool.Business.Constants;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Courses;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Courses
{
    public class GroupService : IGroupService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IUserProvider _userProvider;
        private readonly IValidator<CreateGroupRequest> _createGroupValidator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public GroupService(
            DanceSchoolDbContext db,
            IUserProvider userProvider,
            IValidator<CreateGroupRequest> createGroupValidator,
            IDateTimeProvider dateTimeProvider)
        {
            _db = db;
            _userProvider = userProvider;
            _createGroupValidator = createGroupValidator;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<GroupResponse>> CreateGroup(CreateGroupRequest request)
        {
            var validation = await _createGroupValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return Result.Fail<GroupResponse>(validation.Errors.Select(e => new Error(e.ErrorMessage)).ToList());

            var userId = _userProvider.UserId ?? throw new UnauthorizedAccessException();
            var userRole = _userProvider.Role;

            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId);
            if (course == null)
                return Result.Fail<GroupResponse>("Course not found");
            if (course.Status != ApprovalStatus.Approved)
                return Result.Fail<GroupResponse>("Cannot create groups for a non-approved course");
            if (userRole == UserRole.Instructor && course.CreatedByUserId != userId)
                return Result.Fail<GroupResponse>("You do not have access to this course");

            var isAdmin = userRole == UserRole.Admin;

            var group = new Group
            {
                Id = Guid.NewGuid(),
                CourseId = request.CourseId,
                InstructorId = request.InstructorId,
                Level = request.Level,
                MaxCapacity = request.MaxCapacity,
                Status = isAdmin ? ApprovalStatus.Approved : ApprovalStatus.Pending,
                CreatedByUserId = userId,
                ApprovedByUserId = isAdmin ? userId : null,
                ApprovedAt = isAdmin ? _dateTimeProvider.UtcNow : null,
                CreatedAt = _dateTimeProvider.UtcNow,
            };

            _db.Groups.Add(group);
            await _db.SaveChangesAsync();

            return Result.Ok(MapToResponse(group));
        }

        public async Task<Result<List<GroupResponse>>> GetGroups(Guid courseId)
        {
            var userId = _userProvider.UserId ?? throw new UnauthorizedAccessException();
            var userRole = _userProvider.Role;

            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                return Result.Fail<List<GroupResponse>>("Course not found");

            var query = _db.Groups.Where(g => g.CourseId == courseId);

            if (userRole == UserRole.Instructor)
                query = query.Where(g => g.CreatedByUserId == userId);
            else if (userRole != UserRole.Admin)
                query = query.Where(g => g.Status == ApprovalStatus.Approved);

            var groups = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
            return Result.Ok(groups.Select(MapToResponse).ToList());
        }

        public async Task<Result<GroupResponse>> GetGroup(Guid groupId)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
                return Result.Fail<GroupResponse>("Group not found");

            return Result.Ok(MapToResponse(group));
        }

        public async Task<Result<GroupResponse>> UpdateGroup(Guid groupId, UpdateGroupRequest request)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
                return Result.Fail<GroupResponse>("Group not found");

            group.Level = request.Level;
            group.MaxCapacity = request.MaxCapacity;
            group.InstructorId = request.InstructorId;
            await _db.SaveChangesAsync();

            return Result.Ok(MapToResponse(group));
        }

        public async Task<Result> DeleteGroup(Guid groupId)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
                return Result.Fail("Group not found");

            _db.Groups.Remove(group);
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> ApproveGroup(Guid groupId)
        {
            var adminId = _userProvider.UserId ?? throw new UnauthorizedAccessException();
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return Result.Fail("Group not found");
            if (group.Status != ApprovalStatus.Pending)
                return Result.Fail("Only pending groups can be approved");

            group.Status = ApprovalStatus.Approved;
            group.ApprovedByUserId = adminId;
            group.ApprovedAt = _dateTimeProvider.UtcNow;
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> RejectGroup(Guid groupId)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return Result.Fail("Group not found");
            if (group.Status != ApprovalStatus.Pending)
                return Result.Fail("Only pending groups can be rejected");

            group.Status = ApprovalStatus.Rejected;
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        private static GroupResponse MapToResponse(Group group) => new()
        {
            Id = group.Id,
            CourseId = group.CourseId,
            InstructorId = group.InstructorId,
            Level = group.Level,
            MaxCapacity = group.MaxCapacity,
            Status = group.Status.ToString(),
            CreatedByUserId = group.CreatedByUserId,
            ApprovedByUserId = group.ApprovedByUserId,
            CreatedAt = group.CreatedAt,
            ApprovedAt = group.ApprovedAt,
        };
    }
}