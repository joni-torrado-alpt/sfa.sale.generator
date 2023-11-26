using BenchmarkDotNet.Attributes;

// [MemoryDiagnoser()]
public partial class BenchmarkStressPerformance
{
    private HttpClient? httpClient = null;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Perform any setup logic here, such as initializing the HttpClient instance
        httpClient = new HttpClient();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // Perform any cleanup logic here, such as disposing the HttpClient instance
        httpClient?.Dispose();
    }

    //[Params(10, 50, 100)] // Number of concurrent users to simulate
    // [Params(10, 20, 30)]
    public int NumberOfUsers { get; set; }

    //[Benchmark]
    public void BenchmarkStressTesting()
    {
        // Simulate multiple concurrent login requests
        var tasks = new List<Task>();

        for (int i = 0; i < NumberOfUsers; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                // Perform your login process here
                await PerformLoginProcess();
            }));
        }

        Task.WaitAll(tasks.ToArray());
    }

    [Benchmark]
    public async Task PerformLoginProcess()
    {
        // Simulate the login process with multiple steps
        // Replace these steps with the actual steps of your login process

        // Step 1: Send a GET request to the login page
        const string RequestLoginUri = "http://localhost:8080/seguranca/login.aspx";
        await httpClient!.GetAsync(RequestLoginUri);

        // Step 2: Extract any necessary data from the login page response
        // For example, extract hidden form fields or cookies

        // Step 3: Create a form content with login credentials and any necessary data
        var formContent = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("txtUserName", "jtorradoemp"),
                new KeyValuePair<string, string>("txtPassword", "Rcpgen!?406"),
                // Add any additional form fields
            });

        // Step 4: Send the login POST request with the form content
        var loginResponse = await httpClient.PostAsync(RequestLoginUri, formContent);

        // Step 5: Process the login response if needed
        // For example, check the status code or read the response content
    }
}