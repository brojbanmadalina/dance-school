using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces;
using DanceSchool.Business.Models.Profile;
using DanceSchool.DataAccess.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services
{
    public class UserProfileService : IProfileService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IUserProvider _userProvider;

        public UserProfileService(
            DanceSchoolDbContext danceSchoolDbContext,
            IUserProvider userProvider
        )
        {
            _db = danceSchoolDbContext;
            _userProvider = userProvider;
        }

        public async Task<Result<ProfileResponse>> GetUserProfile()
        {
            var userId = _userProvider.UserId;
            var user = await _db
                .Users.AsNoTracking() // este folosit pentru get-uri simplu, ajuta la performanta, nu e nevoie de tracking la modifiari
                .SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Result.Fail<ProfileResponse>("User not found");
            }

            var profileResponse = new ProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
            };
            return Result.Ok(profileResponse);
        }

        public async Task<Result<List<UserEnrollmentResponse>>> GetUserEnrollments()
        {
            var userId = _userProvider.UserId;
            var enrollments = await _db
                .Enrollments.AsNoTracking()
                .Where(e => e.StudentId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .Select(e => new UserEnrollmentResponse
                {
                    EnrollmentId = e.Id,
                    EnrolledAt = e.EnrolledAt,
                    GroupId = e.GroupId,
                    GroupLevel = e.Group.Level,
                    CourseId = e.Group.CourseId,
                    CourseName = e.Group.Course.Name,
                    InstructorName =
                        e.Group.Instructor.FirstName + " " + e.Group.Instructor.LastName,
                })
                .ToListAsync();

            return Result.Ok(enrollments);
        }
    }
}
