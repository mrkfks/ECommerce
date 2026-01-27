namespace Dashboard.Web.Models
{
    public class CustomerMessageSummaryVm
    {
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }
        public int PendingReplies { get; set; }
        public int RepliedMessages { get; set; }
        public List<CustomerMessageVm> RecentMessages { get; set; } = new();
    }
}
