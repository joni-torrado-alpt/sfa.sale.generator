using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

[MemoryDiagnoser(false)]
[Config(typeof(BenchmarkConfig))]
public class BenchmarkNavigationPerformance
{
    private IWebDriver? _driver;
    private bool isInitialized = false;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // // Perform any setup logic here, such as initializing the WebDriver instance
        // // Create a new instance of the EdgeDriver
        // EdgeOptions edgeOptions = new();
        // // options.UseChromium = true; // Use the Chromium-based Edge driver
        // edgeOptions.AddArgument("--headless");
        // edgeOptions.AddArgument("--inprivate");
        // _driver = new EdgeDriver(edgeOptions);

        ChromeOptions chromeOptions = new();
        chromeOptions.AddArgument("--headless");
        chromeOptions.AddArgument("--incognito");

        _driver = new ChromeDriver(chromeOptions);
        isInitialized = true;
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // Perform any cleanup logic here, such as quitting the WebDriver instance
        // _driver?.Quit();
        // _driver?.Dispose();
        
        if (isInitialized)
        {
            _driver?.Quit();
            _driver?.Dispose();
            isInitialized = false;
        }
    }

    [Benchmark]
    public void BenchmarkPerformance()
    {
        SeleniumTest.StartForBenchmark(_driver!);
    }
}

internal class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        // AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
        AddJob(Job.Default.WithToolchain(InProcessNoEmitToolchain.Instance));
    }
}