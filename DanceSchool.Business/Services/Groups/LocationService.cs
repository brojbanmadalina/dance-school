using DanceSchool.Business.Interfaces.Courses;
using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities;
using DanceSchool.DataAccess.Entities.Locations;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Courses
{
    public class LocationService : ILocationService
    {
        private readonly DanceSchoolDbContext _db;

        public LocationService(DanceSchoolDbContext db)
        {
            _db = db;
        }

        public async Task<Result<LocationResponse>> CreateLocation(CreateLocationRequest request)
        {
            var exists = await _db.Locations.AnyAsync(l => l.Name == request.Name);
            if (exists)
                return Result.Fail<LocationResponse>("A location with this name already exists");

            var location = new Location
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Address = request.Address,
            };

            _db.Locations.Add(location);
            await _db.SaveChangesAsync();

            return Result.Ok(MapToResponse(location));
        }

        public async Task<Result<List<LocationResponse>>> GetLocations()
        {
            var locations = await _db.Locations
                .OrderBy(l => l.Name)
                .ToListAsync();

            return Result.Ok(locations.Select(MapToResponse).ToList());
        }

        public async Task<Result<LocationResponse>> GetLocation(Guid locationId)
        {
            var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == locationId);
            if (location == null)
                return Result.Fail<LocationResponse>("Location not found");

            return Result.Ok(MapToResponse(location));
        }

        public async Task<Result<LocationResponse>> UpdateLocation(Guid locationId, UpdateLocationRequest request)
        {
            var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == locationId);
            if (location == null)
                return Result.Fail<LocationResponse>("Location not found");

            location.Name = request.Name;
            location.Address = request.Address;
            await _db.SaveChangesAsync();

            return Result.Ok(MapToResponse(location));
        }

        public async Task<Result> DeleteLocation(Guid locationId)
        {
            var location = await _db.Locations
                .Include(l => l.GroupSchedules)
                .FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null)
                return Result.Fail("Location not found");

            if (location.GroupSchedules.Any())
                return Result.Fail("Cannot delete a location that has schedules assigned to it");

            _db.Locations.Remove(location);
            await _db.SaveChangesAsync();

            return Result.Ok();
        }

        private static LocationResponse MapToResponse(Location location) => new()
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
        };
    }
}