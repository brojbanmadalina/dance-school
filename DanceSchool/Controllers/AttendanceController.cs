using System.Security.Claims;
using DanceSchool.Business.Interfaces.Attendances;
using DanceSchool.Business.Models.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("session")]
        public async Task<IActionResult> GetSessions([FromQuery] DateOnly date, [FromQuery] Guid? groupId)
        {
            var result = await _attendanceService.GetSessions(CurrentUserId, date, groupId);
            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("session")]
        public async Task<IActionResult> SaveSession(SaveAttendanceRequest request)
        {
            var result = await _attendanceService.SaveSession(CurrentUserId, request);
            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("points/{studentId}")]
        public async Task<IActionResult> GetPoints(Guid studentId)
        {
            var result = await _attendanceService.GetPoints(studentId);
            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }
    }
}