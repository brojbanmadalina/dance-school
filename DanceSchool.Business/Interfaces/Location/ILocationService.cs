using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Courses
{
    public interface ILocationService
    {
        Task<Result<LocationResponse>> CreateLocation(CreateLocationRequest request);
        Task<Result<List<LocationResponse>>> GetLocations();
        Task<Result<LocationResponse>> GetLocation(Guid locationId);
        Task<Result<LocationResponse>> UpdateLocation(Guid locationId, UpdateLocationRequest request);
        Task<Result> DeleteLocation(Guid locationId);
    }
}