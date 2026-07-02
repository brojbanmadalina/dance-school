namespace DanceSchool.Business.Models.Chat
{
    public class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTimeOffset SentAt { get; set; }
    }
}
