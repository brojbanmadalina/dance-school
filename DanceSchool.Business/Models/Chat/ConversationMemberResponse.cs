namespace DanceSchool.Business.Models.Chat
{
    public class ConversationMemberResponse
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
