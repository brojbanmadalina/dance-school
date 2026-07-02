using DanceSchool.Business.Constants;
using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Controllers
{
    [ApiController]
    [Route("api/locations")]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> CreateLocation(CreateLocationRequest request)
        {
            var result = await _locationService.CreateLocation(request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetLocations()
        {
            var result = await _locationService.GetLocations();

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpGet("{locationId}")]
        public async Task<IActionResult> GetLocation(Guid locationId)
        {
            var result = await _locationService.GetLocation(locationId);

            return result.IsFailed
                ? NotFound(new { message = result.Errors.First().Message })
                : Ok(result.Value);
        }

        [HttpPut("{locationId}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> UpdateLocation(Guid locationId, UpdateLocationRequest request)
        {
            var result = await _locationService.UpdateLocation(locationId, request);

            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpDelete("{locationId}")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> DeleteLocation(Guid locationId)
        {
            var result = await _locationService.DeleteLocation(locationId);

            return result.IsFailed
                ? BadRequest(new { message = result.Errors.First().Message })
                : Ok();
        }
    }
}