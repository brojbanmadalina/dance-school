using DanceSchool.DataAccess.Entities.Courses;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanceSchool.DataAccess.Entities.Chat
{
    [Table("conversations")]
    public class Conversation
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public string? Name { get; set; }
        public Guid? GroupId { get; set; }
        public Group? Group { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public ICollection<ConversationMember> Members { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
