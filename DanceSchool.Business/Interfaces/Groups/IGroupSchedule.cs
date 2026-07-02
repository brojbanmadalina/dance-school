using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Courses
{
    public interface IGroupScheduleService
    {
        Task<Result<ScheduleResponse>> AddSchedule(CreateScheduleRequest request);
        Task<Result<List<ScheduleResponse>>> GetSchedules(Guid groupId);
        Task<Result<ScheduleResponse>> UpdateSchedule(Guid scheduleId, UpdateScheduleRequest request);
        Task<Result> RemoveSchedule(Guid scheduleId);
    }
}