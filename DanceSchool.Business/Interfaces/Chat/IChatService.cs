using DanceSchool.Business.Models.Chat;
using FluentResults;

namespace DanceSchool.Business.Interfaces.Chat
{
    public interface IChatService
    {
        Task<Result<ConversationResponse>> CreateDirectConversation(Guid currentUserId, CreateDirectConversationRequest request);
        Task<Result<ConversationResponse>> CreateGroupConversation(Guid currentUserId, CreateGroupConversationRequest request);

        Task<Result> AddMember(Guid currentUserId, Guid conversationId, Guid userIdToAdd);
        Task<Result<List<ConversationResponse>>> GetMyConversations(Guid currentUserId);
        Task<Result<List<MessageResponse>>> GetMessages(Guid currentUserId, Guid conversationId);
        Task<Result<MessageResponse>> SaveMessage(Guid senderId, SendMessageRequest request);
        Task<Result<List<Guid>>> GetMemberIds(Guid conversationId);
        Task<bool> IsMember(Guid conversationId, Guid userId);
    }
}
