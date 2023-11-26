namespace sfa.sale.generator.core;
public class SfaContextOffer : BaseEntity
{
    public long PromoId { get; set; }
    public long SimpleId { get; set; }
    public string Name { get; set; }
    public string Campaign { get; set; }
    public string CampaignPassword { get; set; }
    public int CategoryId { get; set; }
    public string? TreeNodeSelection { get; set; }
    public bool IsNewInstallation => string.IsNullOrEmpty(TreeNodeSelection) || TreeNodeSelection?.Split(",").Length > 4 ? true : false;
    public string Category { get; set; }
    public string Classification { get; set; }
    public List<SfaContextOfferFamily> Families { get; set; }
    public string MacroSegment { get; internal set; }
    public bool IsConsumo => MacroSegment == "CRP" ? true : false;
    public string SalesAgent { get; internal set; }
}