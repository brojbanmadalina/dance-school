using DanceSchool.Business.Models.Attendance;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Attendances
{
    public interface IAttendanceService
    {
        Task<Result<List<SessionAttendanceResponse>>> GetSessions(
            Guid instructorId, DateOnly date, Guid? groupId);
        Task<Result<SessionAttendanceResponse>> SaveSession(Guid currentUserId, SaveAttendanceRequest request);
        Task<Result<StudentPointsResponse>> GetPoints(Guid studentId);
    }
}