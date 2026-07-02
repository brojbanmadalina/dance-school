using DanceSchool.Business.Services.DanceStyle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Api.Controllers
{
    [ApiController]
    [Route("api/dance-styles")]
    public class DanceStyleController : ControllerBase
    {
        private readonly DanceStyleService _service;

        public DanceStyleController(DanceStyleService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }
    }
}
