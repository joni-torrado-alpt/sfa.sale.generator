using System.Diagnostics;

public class PerformanceSimpleTest
{

    public static async Task Start()
    {
        // Configure the test parameters
        int totalRequests = 1_000; // Total number of concurrent requests to simulate
        // string url = "https://portalsfa-p2p.telecom.pt"; // URL of the application to test
        // string url = "https://portalsfa-p2p.telecom.pt/seguranca/login.aspx"; // URL of the application to test
        string url = "http://localhost:8080/seguranca/login.aspx"; // URL of the application to test

        // Create a HttpClient instance for sending requests
        var httpClient = new HttpClient();

        // Create a stopwatch to measure the execution time
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // Send concurrent requests
        var tasks = new Task[totalRequests];
        for (int i = 0; i < totalRequests; i++)
        {
            tasks[i] = SendRequest(httpClient, url);
        }
        await Task.WhenAll(tasks);

        // Stop the stopwatch and calculate the elapsed time
        stopwatch.Stop();
        var elapsedTime = stopwatch.Elapsed;

        // Display the test results
        Console.WriteLine($"Total Requests: {totalRequests}");
        Console.WriteLine($"Elapsed Time: {elapsedTime}");
        Console.WriteLine($"Average Response Time: {elapsedTime.TotalMilliseconds / totalRequests} ms");

        // Dispose the HttpClient instance
        httpClient.Dispose();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task SendRequest(HttpClient httpClient, string url)
    {
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = response.Content;
            // Handle the response content if needed
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
        }
    }

}