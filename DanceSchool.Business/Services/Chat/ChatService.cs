using DanceSchool.Business.Interfaces.Chat;
using DanceSchool.Business.Interfaces.Common;
using DanceSchool.Business.Models.Chat;
using DanceSchool.DataAccess.Data;
using DanceSchool.DataAccess.Entities.Chat;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DanceSchool.Business.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly DanceSchoolDbContext _db;
        private readonly IDateTimeProvider _dateTime;

        public ChatService(DanceSchoolDbContext db, IDateTimeProvider dateTime)
        {
            _db = db;
            _dateTime = dateTime;
        }

        public async Task<Result<ConversationResponse>> CreateDirectConversation(
            Guid currentUserId,
            CreateDirectConversationRequest request
        )
        {
            if (request.OtherUserId == currentUserId)
                return Result.Fail<ConversationResponse>(
                    "Cannot create a conversation with yourself"
                );

            var otherUserExists = await _db.Users.AnyAsync(u => u.Id == request.OtherUserId);
            if (!otherUserExists)
                return Result.Fail<ConversationResponse>("User not found");

            // ar trebui sa verif daca exista conv 1:1 intre users
            var existingConv = await _db
                .Conversations.Include(c => c.Members)
                .Where(c => !c.IsGroup)
                .Where(c =>
                    c.Members.Any(m => m.UserId == currentUserId)
                    && c.Members.Any(m => m.UserId == request.OtherUserId)
                )
                .FirstOrDefaultAsync();

            if (existingConv != null)
                return Result.Ok(await MapConversation(existingConv.Id));

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                IsGroup = false,
                Name = null,
                CreatedAt = _dateTime.UtcNow,
                Members = new List<ConversationMember>
                {
                    new() { UserId = currentUserId, JoinedAt = _dateTime.UtcNow },
                    new() { UserId = request.OtherUserId, JoinedAt = _dateTime.UtcNow },
                },
            };

            _db.Conversations.Add(conversation);
            await _db.SaveChangesAsync();

            return Result.Ok(await MapConversation(conversation.Id));
        }

        public async Task<Result<ConversationResponse>> CreateGroupConversation(
            Guid currentUserId,
            CreateGroupConversationRequest request
        )
        {
            var group = await _db
                .Groups.Include(g => g.Enrollments)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId);

            if (group == null)
                return Result.Fail<ConversationResponse>("Group not found");

            // luam toti studentii inscrisi in acea grupa
            var memberIds = group.Enrollments.Select(e => e.StudentId).ToList();
            memberIds.Add(group.InstructorId);
            memberIds = memberIds.Distinct().ToList();

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                IsGroup = true,
                Name = request.Name,
                GroupId = request.GroupId,
                CreatedAt = _dateTime.UtcNow,
                Members = memberIds
                    .Select(id => new ConversationMember
                    {
                        UserId = id,
                        JoinedAt = _dateTime.UtcNow,
                    })
                    .ToList(),
            };

            _db.Conversations.Add(conversation);
            await _db.SaveChangesAsync();

            return Result.Ok(await MapConversation(conversation.Id));
        }

        public async Task<Result<List<ConversationResponse>>> GetMyConversations(Guid currentUserId)
        {
            var conversationIds = await _db
                .ConversationMembers.Where(m => m.UserId == currentUserId)
                .Select(m => m.ConversationId)
                .ToListAsync();

            var conversations = new List<ConversationResponse>();
            foreach (var id in conversationIds)
            {
                conversations.Add(await MapConversation(id));
            }

            return Result.Ok(
                conversations.OrderByDescending(c => c.LastMessage?.SentAt ?? c.CreatedAt).ToList()
            );
        }

        public async Task<Result<List<MessageResponse>>> GetMessages(
            Guid currentUserId,
            Guid conversationId
        )
        {
            if (!await IsMember(conversationId, currentUserId))
                return Result.Fail<List<MessageResponse>>(
                    "You are not a member of this conversation"
                );

            var messages = await _db
                .Messages.Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var responses = messages.Select(MapMessage).ToList();

            return Result.Ok(responses);
        }

        public async Task<Result> AddMember(
            Guid currentUserId,
            Guid conversationId,
            Guid userIdToAdd
        )
        {
            var conversation = await _db
                .Conversations.Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                return Result.Fail("Conversation not found");

            if (!conversation.Members.Any(m => m.UserId == currentUserId))
                return Result.Fail("You are not a member of this conversation");

            if (conversation.Members.Any(m => m.UserId == userIdToAdd))
                return Result.Fail("User is already a member");

            var userExists = await _db.Users.AnyAsync(u => u.Id == userIdToAdd);
            if (!userExists)
                return Result.Fail("User not found");

            if (!conversation.IsGroup)
            {
                conversation.IsGroup = true;
                conversation.Name ??= "Group chat";
            }

            var convMembers = new ConversationMember
            {
                ConversationId = conversationId,
                UserId = userIdToAdd,
                JoinedAt = _dateTime.UtcNow,
            };
            _db.ConversationMembers.Add(convMembers);
            conversation.Members.Add(convMembers);

            await _db.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result<MessageResponse>> SaveMessage(
            Guid senderId,
            SendMessageRequest request
        )
        {
            if (!await IsMember(request.ConversationId, senderId))
                return Result.Fail<MessageResponse>("You are not a member of this conversation");

            if (string.IsNullOrWhiteSpace(request.Content))
                return Result.Fail<MessageResponse>("Message cannot be empty");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = request.ConversationId,
                SenderId = senderId,
                Content = request.Content,
                SentAt = _dateTime.UtcNow,
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            var sender = await _db.Users.FirstAsync(u => u.Id == senderId);
            message.Sender = sender;

            return Result.Ok(MapMessage(message));
        }

        public async Task<Result<List<Guid>>> GetMemberIds(Guid conversationId)
        {
            var ids = await _db
                .ConversationMembers.Where(m => m.ConversationId == conversationId)
                .Select(m => m.UserId)
                .ToListAsync();

            return Result.Ok(ids);
        }

        public async Task<bool> IsMember(Guid conversationId, Guid userId)
        {
            return await _db.ConversationMembers.AnyAsync(m =>
                m.ConversationId == conversationId && m.UserId == userId
            );
        }

        private async Task<ConversationResponse> MapConversation(Guid conversationId)
        {
            var conversation = await _db
                .Conversations.Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .FirstAsync(c => c.Id == conversationId);

            var lastMessage = await _db
                .Messages.Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            return new ConversationResponse
            {
                Id = conversation.Id,
                IsGroup = conversation.IsGroup,
                Name = conversation.Name,
                CreatedAt = conversation.CreatedAt,
                Members = conversation
                    .Members.Select(m => new ConversationMemberResponse
                    {
                        UserId = m.UserId,
                        FirstName = m.User.FirstName,
                        LastName = m.User.LastName,
                    })
                    .ToList(),
                LastMessage = lastMessage != null ? MapMessage(lastMessage) : null,
            };
        }

        private static MessageResponse MapMessage(Message message) =>
            new()
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
                Content = message.Content,
                SentAt = message.SentAt,
            };
    }
}
