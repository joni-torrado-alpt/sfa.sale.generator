using System.ComponentModel.DataAnnotations.Schema;

namespace sfa.sale.generator.core;
public class SfaContext : BaseEntity
{
    public required string LoginId { get; set; }
    [NotMapped]
    public string? LoginPassword { get; set; }
    public required string ClientIdType { get; set; }
    public required string ClientIdValue { get; set; }
    public required string Environment { get; set; }
    public SfaContextClientAddress? ClientAddress { get; set; }
    public SfaContextOffer Offer { get; set; } = new();
    public SfaContextMasterUser? MasterUser { get; set; }
    public ICollection<SfaSale>? SfaSales { get; set; }
    public bool IsCompleted { get; set; }
}