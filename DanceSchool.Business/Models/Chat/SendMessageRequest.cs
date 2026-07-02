namespace DanceSchool.Business.Models.Chat
{
    public class SendMessageRequest
    {
        public Guid ConversationId { get; set; }
        public string Content { get; set; }
    }
}
