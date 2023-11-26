namespace sfa.sale.generator.core;
public class SfaContextInput
{
    public SfaContextInput(string clientType, string clientId, string offerIdOrName, string environment, string? forceExecution = null)
    {
        if (!Enum.TryParse(typeof(SFAContextClientType), clientType, out var sfaEnumClientType)) throw new Exception($"'{nameof(clientType)}' param value '{clientType}' doesn't exist!");
        ClientIdType = (SFAContextClientType)sfaEnumClientType;
        ClientIdValue = clientId;
        bool.TryParse(forceExecution, out bool force);
        ForceExecution = force;
        if (long.TryParse(offerIdOrName, out long dummyLong))
        {
            OfferId = dummyLong;
        }
        else
        {
            OfferName = offerIdOrName;
        }

        if (!Enum.TryParse(typeof(SfaContextEnv), environment, out var sfaEnumEnv)) throw new Exception($"'env' param value '{environment}' doesn't exist!");
        Env = (SfaContextEnv)sfaEnumEnv;
        var envDevBDName = "PortalPRODUCAO";
        switch (sfaEnumEnv)
        {
            case SfaContextEnv.DEV:
                EnvDBServer = "PJDSFA01";
                EnvDBName = envDevBDName;
                EnvURL = "http://portalsfa-p2p.altice.pt";
                break;
            case SfaContextEnv.DEV_LOCAL:
                EnvDBServer = "PJDSFA01";
                EnvDBName = envDevBDName;
                EnvURL = "http://localhost:8080";
                break;
            case SfaContextEnv.QA:
                EnvDBServer = "PJDSFA01\\SFAQAII";
                EnvDBName = "Portal";
                EnvURL = "http://portalsfa-qa.altice.pt";
                break;
            case SfaContextEnv.QA_LOCAL:
                EnvDBServer = "PJDSFA01\\SFAQAII";
                EnvDBName = "Portal";
                EnvURL = "http://localhost:8081";
                break;
            case SfaContextEnv.QFIX:
                EnvDBServer = "SFA_QFIX";
                EnvDBName = "Portal";
                EnvURL = "https://portalsfa-qfix.telecom.pt";
                break;
            case SfaContextEnv.PROD:
                EnvDBServer = "PJDSFA01";
                EnvDBName = envDevBDName;
                EnvURL = "https://portalsfa.telecom.pt";
                break;
            case SfaContextEnv.PROD_LOCAL:
                EnvDBServer = "PJDSFA01";
                EnvDBName = envDevBDName;
                EnvURL = "http://localhost:8082";
                break;
            default:
                EnvDBServer = "PJDSFA01";
                EnvDBName = envDevBDName;
                EnvURL = "http://localhost:8080";
                break;
        }
    }

    public string LoginId { get; set; }
    public string LoginEmpId { get; set; }
    public string LoginPassword { get; set; }
    public SFAContextClientType ClientIdType { get; set; }
    public string ClientIdValue { get; set; }
    public bool ForceExecution { get; }
    public SfaContextEnv Env { get; set; }
    public string? EnvDBServer { get; }
    public string EnvDBName { get; }
    public string EnvURL { get; }
    public string? ClientAddress { get; set; }
    public long OfferId { get; }
    public string OfferName { get; } = string.Empty;
    public SfaContextInputAddress Address { get; set; }
}

public enum SFAContextClientType
{
    SRV,
    BI,
    NIF,
    NIC,
    NCC,
    PASSPORT,
    OPTION_16, //Número Telefone
}
public enum SfaContextEnv
{
    DEV,
    DEV_LOCAL,
    QA,
    QA_LOCAL,
    QFIX,
    PROD,
    PROD_LOCAL
}

public class SfaContextInputAddress
{
    public SfaContextInputAddress(string name, int cp4, int cp3, string numPoli, string floor, string? fraction = null)
    {
        Name = name;
        CP4 = cp4;
        CP3 = cp3;
        NumPoli = numPoli;
        Floor = floor;
        Fraction = fraction;
    }
    public string? Name { get; set; }
    public int? CP4 { get; set; }
    public string? NumPoli { get; }
    public int? CP3 { get; set; }
    public string? Floor { get; set; }
    public string? Fraction { get; set; }
}

