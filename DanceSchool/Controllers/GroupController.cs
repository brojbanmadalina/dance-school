using DanceSchool.Business.Constants;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Controllers
{
    [ApiController]
    [Route("api/courses/{courseId}/groups")]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(Guid courseId, CreateGroupRequest request)
        {
            request.CourseId = courseId;
            var result = await _groupService.CreateGroup(request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups(Guid courseId)
        {
            var result = await _groupService.GetGroups(courseId);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroup(Guid courseId, Guid groupId)
        {
            var result = await _groupService.GetGroup(groupId);

            return result.IsFailed
                ? NotFound(new { message = result.Errors.First().Message })
                : Ok(result.Value);
        }

        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroup(Guid courseId, Guid groupId, UpdateGroupRequest request)
        {
            var result = await _groupService.UpdateGroup(groupId, request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpDelete("{groupId}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteGroup(Guid courseId, Guid groupId)
        {
            var result = await _groupService.DeleteGroup(groupId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }

        [Authorize(Roles = UserRole.Admin)]
        [HttpPatch("{groupId}/approve")]
        public async Task<IActionResult> ApproveGroup(Guid courseId, Guid groupId)
        {
            var result = await _groupService.ApproveGroup(groupId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }

        [Authorize(Roles = UserRole.Admin)]
        [HttpPatch("{groupId}/reject")]
        public async Task<IActionResult> RejectGroup(Guid courseId, Guid groupId)
        {
            var result = await _groupService.RejectGroup(groupId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }
    }
}