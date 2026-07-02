namespace DanceSchool.Business.Models.Chat
{
    public class ConversationResponse
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<ConversationMemberResponse> Members { get; set; }
        public MessageResponse LastMessage { get; set; }
    }
}
