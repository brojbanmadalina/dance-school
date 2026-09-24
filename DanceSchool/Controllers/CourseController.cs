using Asp.Versioning;
using DanceSchool.Business.Constants;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Jobs;
using DanceSchool.Business.Models.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DanceSchool.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/courses")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IMemoryCache _cache;
        private const string CoursesCacheKey = "courses_cache_v2";

        public CourseController(ICourseService courseService, IMemoryCache cache)
        {
            _courseService = courseService;
            _cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateCourseRequest request)
        {
            var result = await _courseService.CreateCourse(request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [MapToApiVersion("1.0")]
        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var result = await _courseService.GetCourses();

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [MapToApiVersion("2.0")]
        [HttpGet]
        public IActionResult GetCoursesV2()
        {
            if (_cache.TryGetValue(CoursesCacheKey, out var cachedCourses))
            {
                return Ok(cachedCourses);
            }

            var courses = new List<object>
            {
                new { Id = Guid.NewGuid(), Name = "Salsa", Description = "" },
                new { Id = Guid.NewGuid(), Name = "Bachata Sensual", Description = "" },
                new { Id = Guid.NewGuid(), Name = "Tango Argentinian", Description = "" },
            };

            _cache.Set(CoursesCacheKey, courses, TimeSpan.FromMinutes(5));

            return Ok(courses);
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
