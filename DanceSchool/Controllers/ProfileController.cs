using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Interfaces.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : Controller
    {
        public IProfileService _profileService;
        private readonly IEnrollmentService _enrollmentService;
        public ProfileController(IProfileService profileService, IEnrollmentService enrollmentService) 
        {
            this._profileService = profileService;
            this._enrollmentService = enrollmentService;
        }

        public async Task<IActionResult> GetProfile()
        {
            var result = await _profileService.GetUserProfile();
            return Ok(result);
        }
        
        [HttpGet("api/profile/enrollments")]
        public async Task<IActionResult> GetUserEnrollments()
        {
            var result = await _profileService.GetUserEnrollments();
            return Ok(result);
        }

        [HttpDelete("enrollments/{enrollmentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> LeaveGroup(Guid enrollmentId)
        {
            var result = await _enrollmentService.LeaveGroup(enrollmentId);
            return result.ToActionResult();
        }
    }
}
