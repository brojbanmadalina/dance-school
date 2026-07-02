using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Courses;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Courses
{
    public class GroupScheduleService : IGroupScheduleService
    {
        private readonly DanceSchoolDbContext _db;

        public GroupScheduleService(DanceSchoolDbContext db)
        {
            _db = db;
        }

        public async Task<Result<ScheduleResponse>> AddSchedule(CreateScheduleRequest request)
        {
            var group = await _db.Groups.AnyAsync(g => g.Id == request.GroupId);
            if (!group)
                return Result.Fail<ScheduleResponse>("Group not found");

            var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == request.LocationId);
            if (location == null)
                return Result.Fail<ScheduleResponse>("Location not found");

            var conflict = await _db.GroupSchedules
                .AnyAsync(gs => gs.GroupId == request.GroupId && gs.DayOfWeek == request.DayOfWeek);
            if (conflict)
                return Result.Fail<ScheduleResponse>("This group already has a schedule for this day");

            var schedule = new GroupSchedule
            {
                Id = Guid.NewGuid(),
                GroupId = request.GroupId,
                LocationId = request.LocationId,
                Room = request.Room,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            };

            _db.GroupSchedules.Add(schedule);
            await _db.SaveChangesAsync();

            return Result.Ok(MapToResponse(schedule, location.Name));
        }

        public async Task<Result<List<ScheduleResponse>>> GetSchedules(Guid groupId)
        {
            var group = await _db.Groups.AnyAsync(g => g.Id == groupId);
            if (!group)
                return Result.Fail<List<ScheduleResponse>>("Group not found");

            var schedules = await _db.GroupSchedules
                .Include(gs => gs.Location)
                .Where(gs => gs.GroupId == groupId)
                .OrderBy(gs => gs.DayOfWeek)
                .ThenBy(gs => gs.StartTime)
                .ToListAsync();

            var responses = schedules.Select(s => MapToResponse(s, s.Location.Name)).ToList();
            return Result.Ok(responses);
        }

        public async Task<Result<ScheduleResponse>> UpdateSchedule(Guid scheduleId, UpdateScheduleRequest request)
        {
            var schedule = await _db.GroupSchedules
                .Include(gs => gs.Location)
                .FirstOrDefaultAsync(gs => gs.Id == scheduleId);

            if (schedule == null)
                return Result.Fail<ScheduleResponse>("Schedule not found");

            var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == request.LocationId);
            if (location == null)
                return Result.Fail<ScheduleResponse>("Location not found");

            schedule.LocationId = request.LocationId;
            schedule.Room = request.Room;
            schedule.DayOfWeek = request.DayOfWeek;
            schedule.StartTime = request.StartTime;
            schedule.EndTime = request.EndTime;
            await _db.SaveChangesAsync();

            return Result.Ok(MapToResponse(schedule, location.Name));
        }

        public async Task<Result> RemoveSchedule(Guid scheduleId)
        {
            var schedule = await _db.GroupSchedules.FirstOrDefaultAsync(gs => gs.Id == scheduleId);
            if (schedule == null)
                return Result.Fail("Schedule not found");

            _db.GroupSchedules.Remove(schedule);
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        private static ScheduleResponse MapToResponse(GroupSchedule schedule, string locationName) => new()
        {
            Id = schedule.Id,
            GroupId = schedule.GroupId,
            LocationId = schedule.LocationId,
            LocationName = locationName,
            Room = schedule.Room,
            DayOfWeek = schedule.DayOfWeek,
            DayName = schedule.DayOfWeek.ToString(),
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
        };
    }
}