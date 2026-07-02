using DanceSchool.Business.Constants;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Jobs;
using DanceSchool.Business.Models.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Controllers
{
    [ApiController]
    [Route("api/courses")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateCourseRequest request)
        {
            var result = await _courseService.CreateCourse(request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var result = await _courseService.GetCourses();

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourse(Guid courseId)
        {
            var result = await _courseService.GetCourse(courseId);

            return result.IsFailed
                ? NotFound(new { message = result.Errors.First().Message })
                : Ok(result.Value);
        }

        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourse(Guid courseId, UpdateCourseRequest request)
        {
            var result = await _courseService.UpdateCourse(courseId, request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpDelete("{courseId}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteCourse(Guid courseId)
        {
            var result = await _courseService.DeleteCourse(courseId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }

        [Authorize(Roles = UserRole.Admin)]
        [HttpPatch("{courseId}/approve")]
        public async Task<IActionResult> ApproveCourse(Guid courseId)
        {
            var result = await _courseService.ApproveCourse(courseId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }

        [Authorize(Roles = UserRole.Admin)]
        [HttpPatch("{courseId}/reject")]
        public async Task<IActionResult> RejectCourse(Guid courseId)
        {
            var result = await _courseService.RejectCourse(courseId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }

        [HttpPost("/api/test/reminder")]
        [AllowAnonymous]
        public async Task<IActionResult> TestReminder([FromServices] CourseReminderJob job)
        {
            await job.Execute(null);
            return Ok("Job executed - check Mailtrap");
        }
    }
}