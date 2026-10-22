using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanceSchool.Business.Interfaces.Auth;
using DanceSchool.Business.Interfaces.Profile;
using DanceSchool.Business.Models.Profile;
using DanceSchool.DataAccess.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.UserProfile
{
    public class UserProfileService : IProfileService
    {
        private readonly DanceSchoolDbContext _danceSchoolDbContext;
        private readonly IUserProvider _userProvider;

        public UserProfileService(
            DanceSchoolDbContext danceSchoolDbContext,
            IUserProvider userProvider
        )
        {
            _danceSchoolDbContext = danceSchoolDbContext;
            _userProvider = userProvider;
        }

        public Task<Result<ProfileResponse>> GetUserProfile()
        {
            var userId = _userProvider.UserId;
            var user = _danceSchoolDbContext
                .Users.AsNoTracking() // este folosit pentru get-uri simplu, ajuta la performanta, nu e nevoie de tracking la modifiari
                .FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return Task.FromResult(Result.Fail<ProfileResponse>("User not found"));
            }
            else
            {
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
                return Task.FromResult(Result.Ok(profileResponse));
            }
        }

        public async Task<Result<List<UserEnrollmentResponse>>> GetUserEnrollments()
        {
            var userId = _userProvider.UserId;
            var enrollments = await _danceSchoolDbContext
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
