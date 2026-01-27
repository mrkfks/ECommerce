namespace Dashboard.Web.Models
{
    public class MessageReplyVm
    {
        public int MessageId { get; set; }
        public string Reply { get; set; } = string.Empty;
        public int RepliedByUserId { get; set; }
    }
}
