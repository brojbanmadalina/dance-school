namespace DanceSchool.Business.Models.Chat
{
    public class CreateGroupConversationRequest
    {
        public string Name { get; set; }
        public Guid GroupId { get; set; } //o groupa de dans, de ex
    }
}
