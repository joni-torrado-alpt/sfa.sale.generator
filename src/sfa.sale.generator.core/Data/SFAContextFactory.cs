using System.Reflection;
using Microsoft.Data.SqlClient;
namespace sfa.sale.generator.core;

public static class SFAContextFactory
{
    private static Assembly _assembly => typeof(SFAContextFactory).Assembly;
    private static string _connectionString = "server={0};database={1};Integrated Security=SSPI;TrustServerCertificate=True;";
 
    public static SfaContext LoadSfaContext(SfaContextInput contextInput)
    {
        if (contextInput is null) throw new ArgumentNullException(nameof(contextInput));
        if (contextInput.LoginId is null || string.IsNullOrWhiteSpace(contextInput.LoginId)) throw new ArgumentNullException(nameof(contextInput.LoginId));
        if (contextInput?.LoginPassword is null || string.IsNullOrWhiteSpace(contextInput.LoginPassword)) throw new ArgumentNullException("sfaPass");

        var sfaContext = new SfaContext
        {
            LoginId = contextInput.LoginId,
            LoginPassword = contextInput.LoginPassword,
            ClientIdType = contextInput.ClientIdType.ToString(),
            ClientIdValue = contextInput.ClientIdValue,
            ClientAddress = new SfaContextClientAddress("R BOMBARDA", 1100, 100, "2E", ""),
            // ClientAddress = new SfaContextClientAddress("R TORRE", 2750, 762, "100", "AP", "100"), //Esta morada não necessita do EstouAqui
            Environment = contextInput.Env.ToString()
        };
        _connectionString = string.Format(_connectionString, contextInput?.EnvDBServer, contextInput?.EnvDBName);
        FillOffer(contextInput?.OfferId, contextInput?.OfferName, sfaContext);
        FillOfferFamily(sfaContext);
        FillMasterUser(sfaContext);

        sfaContext.LoginId = sfaContext.Offer.MacroSegment == "PME" ? contextInput!.LoginEmpId : sfaContext.LoginId;

        return sfaContext;
    }

    private static void FillOffer(long? offerId, string? offerName, SfaContext sfaContext)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        string? sqlQuery = ResourceHelper.ReadResourceAsString(_assembly, "SQL", "GetSfaContext.sql");
        using var command2 = new SqlCommand(sqlQuery, connection);
        command2.Parameters.AddWithValue("@paramOfferId", offerId);
        command2.Parameters.AddWithValue("@paramOfferName", offerName);
        using var reader2 = command2.ExecuteReader();
        string generatedQuery2 = command2.CommandText;

        while (reader2.Read())
        {
            sfaContext.Offer.PromoId = reader2.GetInt64(8);
            sfaContext.Offer.SimpleId = reader2.GetInt64(0);
            sfaContext.Offer.Name = reader2.GetString(1);
            sfaContext.Offer.CategoryId = reader2.GetInt32(2);
            sfaContext.Offer.Category = reader2.GetString(3);
            sfaContext.Offer.Classification = reader2.GetString(4);
            sfaContext.Offer.MacroSegment = reader2.GetString(5);
            sfaContext.Offer.Campaign = reader2.GetString(6);
            sfaContext.Offer.CampaignPassword = reader2.GetString(7);
            sfaContext.Offer.TreeNodeSelection = reader2.IsDBNull(9) ? string.Empty : reader2.GetString(9);
            sfaContext.Offer.SalesAgent = reader2.GetString(10);
        }
        if (sfaContext.Offer.PromoId <= 0) throw new Exception($"No Offer Id '{offerId}' or Name '{offerName}' found on SFA.");
    }

    private static void FillOfferFamily(SfaContext sfaContext)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        string? sqlQuery = ResourceHelper.ReadResourceAsString(_assembly, "SQL", "GetSfaContextOffer.sql");
        using var command = new SqlCommand(sqlQuery, connection);
        command.Parameters.AddWithValue("@paramOfferId", sfaContext.Offer.PromoId);
        using var reader = command.ExecuteReader();
        // Capture the generated SQL query
        string generatedQuery = command.CommandText;

        sfaContext.Offer.Families = new List<SfaContextOfferFamily>();
        while (reader.Read())
        {
            sfaContext.Offer.Families.Add(new SfaContextOfferFamily() { FamilyId = reader.GetInt64(0), Name = reader.GetString(2) });
        }
        if (!sfaContext.Offer.Families.Any()) throw new Exception($"No Offer family found on SFA for query: {Environment.NewLine}'{generatedQuery}'.");
    }

    private static void FillMasterUser(SfaContext sfaContext)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        string? sqlQuery = ResourceHelper.ReadResourceAsString(_assembly, "SQL", "GetSfaContextMasterUser.sql");
        using var command = new SqlCommand(sqlQuery, connection);
        using var reader = command.ExecuteReader();
        // Capture the generated SQL query
        string generatedQuery = command.CommandText;
        if (!reader.HasRows) throw new Exception($"No MasterUser found on SFA for query: {Environment.NewLine}'{generatedQuery}'.");

        sfaContext.MasterUser = new();
        while (reader.Read())
        {
            switch (reader.GetString(0))
            {
                case "OTT_AdmConta_BI":
                    sfaContext.MasterUser.BI = reader.GetString(1);
                    break;
                case "OTT_AdmConta_Email":
                    sfaContext.MasterUser.Email = reader.GetString(1);
                    break;
                case "OTT_AdmConta_NIF":
                    sfaContext.MasterUser.NIF = reader.GetString(1);
                    break;
                case "OTT_AdmConta_Nome":
                    sfaContext.MasterUser.Name = reader.GetString(1);
                    break;
                case "OTT_AdmConta_Telefone":
                    sfaContext.MasterUser.Telephone = reader.GetString(1);
                    break;
                case "OTT_AdmConta_Telemovel":
                    sfaContext.MasterUser.Mobile = reader.GetString(1);
                    break;
                default:
                    break;
            }
        }
    }

    public static string[] GetSfaLoginUsers(int numberOfInstances)
    {
        List<string> users = new();
        string connectionString = _connectionString;

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string sqlQuery = $"SELECT TOP {numberOfInstances} Login FROM dbo.Login (NOLOCK) WHERE ModificadoPor = 'RateLimit'";
            using var command = new SqlCommand(sqlQuery, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(reader.GetString(0));
            }
            if (!users.Any()) throw new Exception($"No login users found on SFA for query: {Environment.NewLine}'{sqlQuery}'.");

        }
        return users!.ToArray();
    }
}