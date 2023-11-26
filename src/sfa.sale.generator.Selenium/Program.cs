SeleniumTest.Start(args);

//SimulateUserrequest(args);

static async Task SimulateUserrequest(string[] args)
{
    int numberOfUsers = 10; // Number of concurrent users to simulate
    string webAppUrl = "http://localhost:8080"; // Replace with your ASP.NET Web Forms app URL

    Console.WriteLine($"Simulating {numberOfUsers} users...");

    // Create a list to hold the concurrent tasks
    List<Task> tasks = new List<Task>();

    // Create an HttpClient instance
    HttpClient httpClient = new HttpClient();

    // Simulate concurrent user requests
    for (int i = 0; i < numberOfUsers; i++)
    {
        tasks.Add(SimulateUserRequest(httpClient, webAppUrl));
    }

    // Wait for all tasks to complete
    await Task.WhenAll(tasks);

    // Dispose the HttpClient instance
    httpClient.Dispose();

    Console.WriteLine("Simulation completed.");
}

static async Task SimulateUserRequest(HttpClient httpClient, string webAppUrl)
{
    // Perform login request if required
    await PerformLogin(httpClient, webAppUrl);

    // Perform a request to a custom page
    await PerformCustomPageRequest(httpClient, webAppUrl);
}

static async Task PerformLogin(HttpClient httpClient, string webAppUrl)
{
    var loginUrl = $"{webAppUrl}/login"; // Replace with your login page URL

    // Create a form content with login credentials
    var formContent = new FormUrlEncodedContent(new[]
    {
                new KeyValuePair<string, string>("username", "your-username"),
                new KeyValuePair<string, string>("password", "your-password")
            });

    // Send the login POST request
    var loginResponse = await httpClient.PostAsync(loginUrl, formContent);

    // Check the login response status and handle accordingly
    if (loginResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("Login successful");
    }
    else
    {
        Console.WriteLine("Login failed");
    }
}

static async Task PerformCustomPageRequest(HttpClient httpClient, string webAppUrl)
{
    var customPageUrl = $"{webAppUrl}/custom-page"; // Replace with your custom page URL

    // Send a GET request to the custom page
    var customPageResponse = await httpClient.GetAsync(customPageUrl);

    // Check the custom page response status and handle accordingly
    if (customPageResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("Custom page request successful");
    }
    else
    {
        Console.WriteLine("Custom page request failed");
    }
}
