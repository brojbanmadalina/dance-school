using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Courses
{
    public interface IEnrollmentService
    {
        Task<Result<EnrollmentResponse>> EnrollStudent(EnrollStudentRequest request);
        Task<Result<List<EnrollmentResponse>>> GetGroupStudents(Guid groupId);
        Task<Result> RemoveStudent(Guid groupId, Guid studentId);
    }
}