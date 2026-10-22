using DanceSchool.Business.Models.Profile;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanceSchool.Business.Interfaces.Profile
{
    public interface IProfileService
    {
        Task<Result<ProfileResponse>> GetUserProfile();
        Task<Result<List<UserEnrollmentResponse>>> GetUserEnrollments();
    }
}
