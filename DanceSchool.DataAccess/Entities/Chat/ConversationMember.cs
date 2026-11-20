using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.DataAccess.Entities.Chat
{
    public class ConversationMember
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTimeOffset JoinedAt { get; set; }
    }
}
