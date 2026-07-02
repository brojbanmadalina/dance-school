using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Controllers
{
    [ApiController]
    [Route("api/groups/{groupId}/schedules")]
    [Authorize]
    public class GroupScheduleController : ControllerBase
    {
        private readonly IGroupScheduleService _scheduleService;

        public GroupScheduleController(IGroupScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule(Guid groupId, CreateScheduleRequest request)
        {
            request.GroupId = groupId;
            var result = await _scheduleService.AddSchedule(request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedules(Guid groupId)
        {
            var result = await _scheduleService.GetSchedules(groupId);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpPut("{scheduleId}")]
        public async Task<IActionResult> UpdateSchedule(Guid groupId, Guid scheduleId, UpdateScheduleRequest request)
        {
            var result = await _scheduleService.UpdateSchedule(scheduleId, request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpDelete("{scheduleId}")]
        public async Task<IActionResult> RemoveSchedule(Guid groupId, Guid scheduleId)
        {
            var result = await _scheduleService.RemoveSchedule(scheduleId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }
    }
}