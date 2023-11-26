using sfa.sale.generator.core;

public class PlaywrightApp
{
    public async Task SFA_Should_Create_Sale()
    {
        await InitAsync();
        await PlaywrightTestFactory.RunAsync();
    }

    private async Task InitAsync()
    {
        var sfaLogin = Environment.GetEnvironmentVariable("user") ?? "jtorrado";
        var sfaLoginEmp = Environment.GetEnvironmentVariable("userEmp") ?? "jtorradoemp";
        var sfaLoginPassword = Environment.GetEnvironmentVariable("sfaPass");
        var clientType = Environment.GetEnvironmentVariable("clientType") ?? "SRV";
        var clientId = Environment.GetEnvironmentVariable("clientId") ?? "1702744516";
        var offerIdOrName = Environment.GetEnvironmentVariable("offer") ?? "1090129";
        var offerMacroSegment = Environment.GetEnvironmentVariable("macroSegment"); //TODO
        var env = Environment.GetEnvironmentVariable("env") ?? "DEV_LOCAL";

        var input = new SfaContextInput(clientType, clientId, offerIdOrName, env)
        {
            LoginId = sfaLogin,
            LoginEmpId = sfaLoginEmp,
            LoginPassword = sfaLoginPassword ?? string.Empty
        };

        await PlaywrightTestFactory.InitAsync(input);
    }
}