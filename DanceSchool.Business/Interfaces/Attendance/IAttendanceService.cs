using DanceSchool.Business.Models.Attendance;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Attendances
{
    public interface IAttendanceService
    {
        Task<Result<List<SessionAttendanceResponse>>> GetSessions(
            Guid instructorId, DateOnly date, Guid? groupId);
        Task<Result<SessionAttendanceResponse>> SaveSession(Guid currentUserId, SaveAttendanceRequest request);
        Task<Result<SessionAttendanceResponse>> MarkStudent(Guid instructorId, Guid groupId, Guid studentId, MarkStudentRequest request);
        Task<Result<SessionAttendanceResponse>> BulkMarkPresent(Guid instructorId, Guid groupId, BulkMarkPresentRequest request);
        Task<Result<StudentPointsResponse>> GetPoints(Guid studentId);
    }
}