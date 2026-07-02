using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Controllers
{
    [ApiController]
    [Route("api/groups/{groupId}/students")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpPost]
        public async Task<IActionResult> EnrollStudent(Guid groupId, EnrollStudentRequest request)
        {
            request.GroupId = groupId;
            var result = await _enrollmentService.EnrollStudent(request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupStudents(Guid groupId)
        {
            var result = await _enrollmentService.GetGroupStudents(groupId);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpDelete("{studentId}")]
        public async Task<IActionResult> RemoveStudent(Guid groupId, Guid studentId)
        {
            var result = await _enrollmentService.RemoveStudent(groupId, studentId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }
    }
}