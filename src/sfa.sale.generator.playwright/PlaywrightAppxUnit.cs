using Microsoft.Playwright;
using sfa.sale.generator.core;
using Xunit;
using Xunit.Abstractions;

public class PlaywrightAppxUnit : IAsyncLifetime
{
    private readonly ITestOutputHelper _outputHelper;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public PlaywrightAppxUnit(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public async Task InitializeAsync()
    {
        var sfaLogin = Environment.GetEnvironmentVariable("user") ?? "jtorrado";
        var sfaLoginEmp = Environment.GetEnvironmentVariable("userEmp") ?? "jtorradoemp";
        var sfaLoginPassword = Environment.GetEnvironmentVariable("sfaPass");
        var clientType = Environment.GetEnvironmentVariable("clientType") ?? "SRV";
        var clientId = Environment.GetEnvironmentVariable("clientId") ?? "1702744516";
        var offerIdOrName = Environment.GetEnvironmentVariable("offer") ?? "1090129";
        var offerMacroSegment = Environment.GetEnvironmentVariable("macroSegment"); //TODO
        var env = Environment.GetEnvironmentVariable("env") ?? "DEV_LOCAL";
        var forceExecution = Environment.GetEnvironmentVariable("f") ?? "0";

        var input = new SfaContextInput(clientType, clientId, offerIdOrName, env, forceExecution)
        {
            LoginId = sfaLogin,
            LoginEmpId = sfaLoginEmp,
            LoginPassword = sfaLoginPassword ?? string.Empty
        };

        await PlaywrightTestFactory.InitAsync(input, _outputHelper);
    }

    [Fact]
    public async Task SFA_Should_Create_Sale()
    {
        //await using var context = await TraceInit();
        // _page.SetDefaultTimeout(60000);

        await PlaywrightTestFactory.RunAsync();

        // await _page.PauseAsync();
        // await Task.Delay(-1);
        // await _page.ScreenshotAsync(new()
        // {
        //     Path = "screenshot.png",
        //     FullPage = true,
        // });

        // Stop tracing and export it into a zip archive.
        // await TraceEnd(context);
    }

    private static async Task TraceEnd(IBrowserContext context)
    {
        await context.Tracing.StopAsync(new()
        {
            Path = "trace.zip"
        });
    }

    private async Task<IBrowserContext?> TraceInit()
    {
        if (_browser is null) return null;
        // Start tracing before creating / navigating a page.
        var context = await _browser.NewContextAsync();
        await context.Tracing.StartAsync(new()
        {
            // Screenshots = true,
            Snapshots = true,
            Sources = true
        });
        return context;
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

}