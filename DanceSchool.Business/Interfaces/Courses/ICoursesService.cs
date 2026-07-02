using DanceSchool.Business.Models.Courses;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Courses
{
    public interface ICourseService
    {
        Task<Result<CourseResponse>> CreateCourse(CreateCourseRequest request);
        Task<Result<List<CourseResponse>>> GetCourses();
        Task<Result<CourseResponse>> GetCourse(Guid courseId);
        Task<Result<CourseResponse>> UpdateCourse(Guid courseId, UpdateCourseRequest request);
        Task<Result> DeleteCourse(Guid courseId);
        Task<Result> ApproveCourse(Guid courseId);
        Task<Result> RejectCourse(Guid courseId);
    }
}