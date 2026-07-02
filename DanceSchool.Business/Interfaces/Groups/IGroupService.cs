using DanceSchool.Business.Models.Courses;
using DanceSchool.Business.Models.Groups;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Courses
{
    public interface IGroupService
    {
        Task<Result<GroupResponse>> CreateGroup(CreateGroupRequest request);
        Task<Result<List<GroupResponse>>> GetGroups(Guid courseId);
        Task<Result<GroupResponse>> GetGroup(Guid groupId);
        Task<Result<GroupResponse>> UpdateGroup(Guid groupId, UpdateGroupRequest request);
        Task<Result> DeleteGroup(Guid groupId);
        Task<Result> ApproveGroup(Guid groupId);
        Task<Result> RejectGroup(Guid groupId);
    }
}