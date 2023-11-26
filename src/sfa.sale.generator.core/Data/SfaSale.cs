namespace sfa.sale.generator.core;

public class SfaSale : BaseEntity
{
    public SfaSale(string guid, string url, TimeSpan? duration, bool isCompleted = false)
    {
        Guid = guid;
        Url = url;
        Duration = duration;
        IsCompleted = isCompleted;
    }
    public string Url { get; set; }
    public string Guid { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan? Duration { get; set; }
    public int SfaContextId { get; set; }
}