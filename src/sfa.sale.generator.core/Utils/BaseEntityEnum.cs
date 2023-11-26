using System.ComponentModel.DataAnnotations;

namespace sfa.sale.generator.core;

public abstract class BaseEntityEnum<TEnum> where TEnum : Enum, IConvertible
{
    [Required]
    public TEnum Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual BaseEntityEnum<TEnum> InitForSeeding(TEnum value)
    {
        Id = value;
        Name = value.ToString();
        Description = value.GetEnumDescription();
        CreatedOn = DateTime.Now;
        return this;
    }
}
