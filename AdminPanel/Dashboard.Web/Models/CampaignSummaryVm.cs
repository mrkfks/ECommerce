namespace Dashboard.Web.Models;

public class CampaignSummaryVm
{
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int UpcomingCampaigns { get; set; }
    public int ExpiredCampaigns { get; set; }
    public List<CampaignVm> CurrentCampaigns { get; set; } = new();
}
