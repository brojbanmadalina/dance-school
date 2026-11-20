using DanceSchool.DataAccess.Entities.Users;

namespace DanceSchool.DataAccess.Entities.Chat
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public Guid SenderId { get; set; }
        public User Sender { get; set; }
        public string Content { get; set; }
        public DateTimeOffset SentAt { get; set; }
    }
}
