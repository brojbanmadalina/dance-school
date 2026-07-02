using DanceSchool.Business.Contracts.Student;
using DanceSchool.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceSchool.Api.Controllers
{

    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        //private readonly StudentService _studentService;

        //public StudentsController(StudentService studentService)
        //{
        //    _studentService = studentService;
        //}

        //[Authorize(Roles = "1,3")]
        //[HttpPost]
        //public async Task<IActionResult> CreateStudent(CreateStudentRequest request)
        //{
        //    await _studentService.CreateStudent(request);

        //    return Ok();
        //}
    }
}

