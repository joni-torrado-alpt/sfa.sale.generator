using System.ComponentModel.DataAnnotations.Schema;
using sfa.sale.generator.core;

public class SfaLog : BaseEntity
{
    public SfaLogTypeEnum Type { get; set; }
    public int SfaContextId { get; set; }
    public string? Message { get; set; }
    public string? MessageDump { get; set; }

    [NotMapped]
    public new string? ModifiedBy { get; set; }
    [NotMapped]
    public new DateTime ModifiedOn { get; set; }
}

public enum SfaLogTypeEnum
{
    ERROR = 1, WARNING = 2, INFO = 3
}

public class SfaLogType : BaseEntityEnum<SfaLogTypeEnum> { }