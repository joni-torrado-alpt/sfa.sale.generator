namespace sfa.sale.generator.core;
public class SfaContextClientAddress : BaseEntity
{
    private SfaContextClientAddress()
    {
        
    }

    public SfaContextClientAddress(string name, int cp4, int cp3, string policeNumber, string floor, string? fraction = null)
    {
        Name = name;
        CP4 = cp4;
        CP3 = cp3;
        PoliceNumber = policeNumber;
        Floor = floor;
        Fraction = fraction;
    }
    public string? Name { get; set; }
    public int? CP4 { get; set; }
    public int? CP3 { get; set; }
    public string? PoliceNumber { get; set;}
    public string? Floor { get; set; }
    public string? Fraction { get; set; }
}
