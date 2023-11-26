using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using CommandLine;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;

public class SeleniumTest
{
    private const string COMMAND_HEADLESS = "--headless";
    // private const string _url = "http://localhost:8080";
    private const string _url = "http://portalsfa-qfix.telecom.pt";
    private const string _login_User = "jtorrado";
    //private const string _login_User = "61473222";
    private const string _login_Pass = "Rcpgen!?408";

    public static void Start(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunOptions)
            .WithNotParsed(HandleParseError);
    }

    public static void StartForBenchmark(IWebDriver driver)
    {
        Options options = new();
        options.Login = _login_User;
        PerformExecution(driver, new());
    }

    private static void RunOptions(Options opts)
    {
        var defaultEdgeBrowserExec = !opts.BrowserChrome && !opts.BrowserEdge && !opts.BrowserFirefox;
        opts.Login = _login_User;

        if (opts.BrowserChrome) StartInstanceHandler(opts, StartWithChrome);
        if (opts.BrowserEdge || defaultEdgeBrowserExec) StartInstanceHandler(opts, StartWithEdge);
        if (opts.BrowserFirefox) StartInstanceHandler(opts, StartWithFirefox);
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        //handle errors
        foreach (var error in errs)
        {
            System.Console.WriteLine(error.ToString());
        }
    }

    private static void StartInstanceHandler(Options opts, Action<Options> action)
    {
        // Measure execution time for the entire test
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var elapsedTime = TimeSpan.Zero;

        int numberOfInstances = opts.WithMultipleInstancesNumber;
        if (numberOfInstances == 1)
        {
            action.Invoke(opts);

            // Stop the stopwatch and calculate the elapsed time
            stopwatch.Stop();
            elapsedTime = stopwatch.Elapsed;
            Console.WriteLine($"Elapsed Time: {elapsedTime}");
            return;
        }

        // On multiple instances run with headless
        opts.WithHeadless = true;

        // Create an array to store the threads
        Thread[] threads = new Thread[numberOfInstances];

        string[] users = GetSfaLoginUsers(numberOfInstances);

        // Start each instance on a separate thread
        for (int i = 0; i < numberOfInstances; i++)
        {
            int instanceNumber = i + 1;
            var currentUser = users[i];
            threads[i] = new Thread(() =>
            {
                // Perform actions or interactions in this instance
                Console.WriteLine($"Instance {instanceNumber} started for '{currentUser}'.");
                var newOpts = (Options)opts.Clone();
                newOpts.Login = currentUser;
                action.Invoke(newOpts);
                Console.WriteLine($"Instance {instanceNumber} completed for '{currentUser}'.");
            });
            threads[i].Start();
        }

        // Wait for all threads to complete
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        stopwatch.Stop();
        elapsedTime = stopwatch.Elapsed;
        Console.WriteLine($"Elapsed Time: {elapsedTime}");
        Console.WriteLine($"Average Response Time: {elapsedTime.TotalMilliseconds / numberOfInstances} ms");

        Console.WriteLine("All instances have completed.");

        // // Press any key to exit
        // Console.ReadKey();
    }

    private static string[] GetSfaLoginUsers(int numberOfInstances)
    {
        List<string> users = new();
        string connectionString = "server=PJDSFA01;database=PortalPRODUCAO;Integrated Security=SSPI;TrustServerCertificate=True;";

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string sqlQuery = $"SELECT TOP {numberOfInstances} Login FROM Login (NOLOCK) WHERE ModificadoPor = 'RateLimit'";
            using var command = new SqlCommand(sqlQuery, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(reader.GetString(0));
            }
            if (!users.Any()) throw new Exception($"No login users found on SFA for query: {Environment.NewLine}'{sqlQuery}'.");

        }
        return users.ToArray();
    }

    private static void StartWithEdge(Options opts)
    {

        // // Set the path to the MicrosoftWebDriver.exe for Edge
        // string edgeDriverPath = @"c:\tools\msedgedriver.exe";

        // Create a new instance of the EdgeDriver
        EdgeOptions edgeOptions = new();
        if (opts.WithHeadless)
        {
            edgeOptions.AddArgument(COMMAND_HEADLESS);
        }
        // edgeOptions.AddArgument("--inprivate");
        // edgeOptions.AddArgument("inprivate");
        //edgeOptions.BinaryLocation = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

        string strEdgeProfilePath = @"C:\Users\" + Environment.UserName + @"\AppData\Local\Microsoft\Edge\User Data\Default";
        // Here you set the path of the profile ending with User Data not the profile folder
        edgeOptions.AddArgument("--user-data-dir=" + strEdgeProfilePath);
        // Here you specify the actual profile folder
        // If it is Default profile, no need to use this line of code
        // edgeOptions.AddArgument("--profile-directory=joni.torrado@hotmail.com");


        // string strEdgeProfilePath = @"C:\Users\" + Environment.UserName + @"\AppData\Local\Microsoft\Edge\User Data";
        // // Here you set the path of the profile ending with User Data not the profile folder
        // edgeOptions.AddArgument("--user-data-dir=" + strEdgeProfilePath);

        // // Here you specify the actual profile folder
        // // If it is Default profile, no need to use this line of code
        // edgeOptions.AddArgument("--profile-directory=Selenium");

        // edgeOptions.AddArgument("profile-directory=Work");
        // var driverService = EdgeDriverService.CreateDefaultService(edgeOptions);

        IWebDriver driver = new EdgeDriver(edgeOptions);
        // IWebDriver driver = new EdgeDriver(edgeDriverPath, edgeOptions);
        // IWebDriver driver = new EdgeDriver(driverService);
        //IWebDriver driver = new EdgeDriver();

        PerformExecution(driver, opts);
    }

    private static void StartWithChrome(Options opts)
    {
        // // Set the path to the WebDriver executable
        // string chromeDriverPath = "C:\\path\\to\\chromedriver.exe";

        // Configure Chrome to run in headless mode
        ChromeOptions chromeOptions = new();
        if (opts.WithHeadless)
        {
            chromeOptions.AddArgument(COMMAND_HEADLESS);
        }
        chromeOptions.AddArgument("--incognito");
        IWebDriver chromeDriver = new ChromeDriver(chromeOptions);

        PerformExecution(chromeDriver, opts);
    }

    private static void StartWithFirefox(Options opts)
    {
        // Set the path to the WebDriver executable
        string firefoxDriverPath = "";//"C:\\path\\to\\geckodriver.exe";

        // Configure Firefox to run in headless mode
        FirefoxOptions firefoxOptions = new();
        if (opts.WithHeadless)
        {
            firefoxOptions.AddArgument(COMMAND_HEADLESS);
        }
        firefoxOptions.AddArgument("-private");
        IWebDriver firefoxDriver = new FirefoxDriver(firefoxDriverPath, firefoxOptions);

        PerformExecution(firefoxDriver, opts);
    }

    private static void PerformExecution(IWebDriver driver, Options opts)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        driver.Navigate().GoToUrl($"{_url}");
        stopwatch.Stop();
        var elapsedTime = stopwatch.Elapsed;
        Console.WriteLine($"{elapsedTime} [Elapsed Time] Navigate to '{_url}'");

        // Measure execution time for the entire test
        stopwatch.Restart();
        PerformLogin(driver, opts);
        stopwatch.Stop();
        elapsedTime = stopwatch.Elapsed;
        Console.WriteLine($"{elapsedTime} [Elapsed Time] 'PerformLogin' with user '{opts.Login}'");

        // Wait for the page to load or perform further actions
        stopwatch.Restart();
        PerformWirelineOrderDetail(driver);
        stopwatch.Stop();
        elapsedTime = stopwatch.Elapsed;
        Console.WriteLine($"{elapsedTime} [Elapsed Time] 'PerformWirelineOrderDetail'");

        //return;
        // Execute RateLimit validations
        const int numberOfIterations = 0;
        for (int i = 0; i <= numberOfIterations; i++)
        {
            stopwatch.Restart();
            PerformSalesScriptWirelineSearch(driver);
            stopwatch.Stop();
            elapsedTime = stopwatch.Elapsed;
            Console.WriteLine($"{elapsedTime} [Elapsed Time] 'PerformSalesScriptWirelineSearch'");

            CheckSalesScriptWirelineErrors(driver);
            var hasRedirected = CheckPageRedirect(driver);
            if (hasRedirected && i == numberOfIterations)
            {
                Console.WriteLine("Success");
            }
        }

        // Close the browser
        // Console.ReadKey();
        driver.Quit();
        Console.WriteLine("DONE");
    }

    private static void PerformLogin(IWebDriver driver, Options opts)
    {
        // Navigate to the login page
        driver.Navigate().GoToUrl($"{_url}/seguranca/login.aspx");

        // Find the username and password input fields and fill them
        IWebElement usernameField = driver.FindElement(By.Id("txtUserName"));
        usernameField.SendKeys(opts.Login);
        IWebElement passwordField = driver.FindElement(By.Id("txtPassword"));
        passwordField.SendKeys(_login_Pass);

        // Submit the login form
        IWebElement loginButton = driver.FindElement(By.Id("btnLogin"));
        loginButton.Click();
    }

    private static void PerformWirelineOrderDetail(IWebDriver driver)
    {
        // Find the child element
        var childElement = driver.FindElements(By.XPath("//div[contains(text(), 'Itens de Venda')]"));
        // IWebElement childElement = driver.FindElement(By.ClassName("m0l1iout"));
        if (!childElement.Any())
        {
            System.Console.WriteLine("Módulo Itens de Venda não encontrado");
            return;
        }

        // Get the parent element of the child element
        IWebElement parentElement = childElement.First().FindElement(By.XPath(".."));

        // Click on the parent element
        if (!parentElement.Displayed)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", parentElement);
        }
        else
        {
            parentElement.Click();
        }

        //Do a quick Search
        // Go to the search page
        IWebElement searchButton = driver.FindElement(By.Name("ctl11$GridToolBar$ctl03"));
        searchButton.Click();

        // Find the fields and fill them
        var searchValue = "61473222";
        IWebElement fieldType = driver.FindElement(By.Id("ctl11_Query_QueryGrid_cmbQueryField_0"));
        fieldType.SendKeys("ID");
        IWebElement searchField = driver.FindElement(By.Id("ctl11_Query_QueryGrid_txtQueryValue_0"));
        searchField.Clear();
        searchField.SendKeys(searchValue);

        // Submit
        IWebElement searchButtonOnSearch = driver.FindElement(By.Id("ctl11_Query_btnPesquisar"));
        searchButtonOnSearch.Click();

        CheckWirelineOrderDetailErrors(driver);

        //Check if the search returned data
        ReadOnlyCollection<IWebElement> result = driver.FindElements(By.Id("ctl11_Grid_col00_0"));
        if (result.Any())
        {
            if (result?.FirstOrDefault()?.Text == $"PRT{searchValue}")
            {
                Console.WriteLine($"Found Order Detail SaleId: '{searchValue}'");
            }
        }
        else
        {
            Console.WriteLine($"Not Found Order Detail SaleId: '{searchValue}'");
        }
    }

    private static void PerformSalesScriptWirelineSearch(IWebDriver driver)
    {
        // Navigate to the specific page
        if (!driver.Url.Contains("wireline.aspx"))
        {
            driver.Navigate().GoToUrl($"{_url}/Scripts/ScriptVendas/wireline.aspx");
        }

        //Check if the user has access
        // IWebElement hasAccess = driver.FindElements(By.Id("DocumentType"));
        // // Find the element containing the text
        // IWebElement element = driver.FindElement(By.XPath("//div[@id='myElement']"));
        string searchText = "O utilizador não tem permissões para aceder";//O utilizador não tem permissões para aceder ao módulo requisitado
        // Find the element by its text content using XPath
        string xpathExpression = $"//*[contains(text(), '{searchText}')]";
        var elements = driver.FindElements(By.XPath(xpathExpression));
        if (!elements.Any() || !string.IsNullOrEmpty(elements.First().Text))
        {
            System.Console.WriteLine($"User has no permissions to access Wireline Order. '{elements.First().Text}'");
            return;
        }

        // Find the fields and fill them
        IWebElement docType = driver.FindElement(By.Id("DocumentType"));
        docType.SendKeys("NIF");
        IWebElement docValue = driver.FindElement(By.Id("DocumentValue"));
        docValue.Clear();
        docValue.SendKeys("254722326");

        // Submit the form
        IWebElement searchButton = driver.FindElement(By.Id("Order.NSOMSearchClient.doDTHSearchClientAction"));
        searchButton.Click();

        // Wait for the result page to load or perform further actions as needed
        CheckModalEmailSync(driver);
    }

    private static bool CheckPageRedirect(IWebDriver driver)
    {
        // Get the current URL and compare with the expected URL
        string currentUrl = driver.Url;
        string redirectedUrl = "RateLimitError.aspx";

        if (currentUrl.Contains(redirectedUrl))
        {
            Console.WriteLine("Page redirect has occurred.");
            return true;
        }

        return false;
        // else
        // {
        //     Console.WriteLine("No page redirect has occurred.");
        // }


        // // Get the HTTP status code of the current page
        // int httpStatusCode = int.Parse(driver.ExecuteScript("return window.performance.timing.responseStart;").ToString());

        // if (httpStatusCode >= 300 && httpStatusCode < 400)
        // {
        //     Console.WriteLine("Page redirect has occurred.");
        // }
        // else
        // {
        //     Console.WriteLine("No page redirect has occurred.");
        // }

        //  // Check for the presence of elements on the redirected page
        // By redirectedElementLocator = By.CssSelector("#redirectedElement");
        // bool isRedirected = driver.FindElements(redirectedElementLocator).Count > 0;

        // if (isRedirected)
        // {
        //     Console.WriteLine("Page redirect has occurred.");
        // }
        // else
        // {
        //     Console.WriteLine("No page redirect has occurred.");
        // }
    }

    private static void CheckModalEmailSync(IWebDriver driver)
    {
        const string IdToFind = "EmailClientSyncPopupWindow";
        By locator = By.Id(IdToFind);
        int elementCount = driver.FindElements(locator).Count;
        if (elementCount > 0)
        {
            // Check if the modal window is displayed
            IWebElement modalWindow = driver.FindElement(locator);
            bool isModalDisplayed = modalWindow.Displayed;

            if (isModalDisplayed)
            {
                Console.WriteLine("Modal window 'EmailClientSyncPopupWindow' is displayed.");

                IWebElement checkBoxEmail = driver.FindElement(By.Id("Order.EmailClientSyncPopup.EmailClientSyncPopupNoEmailChecked"));
                checkBoxEmail.Click();

                // Submit the form
                IWebElement submitButton = driver.FindElement(By.Id("Order.EmailClientSyncPopup.doEmailClientSyncPopupAction"));
                submitButton.Click();
            }
            // else
            // {
            //     Console.WriteLine("Modal window is not displayed.");
            // }
        }
    }

    private static void CheckSalesScriptWirelineErrors(IWebDriver driver)
    {
        // Validate the result
        // Find the element by its locator
        const string IdToFind = "IsError";
        By locator = By.Id(IdToFind);
        int elementCount = driver.FindElements(locator).Count;
        if (elementCount > 0)
        {
            // Console.WriteLine($"Element '{IdToFind}' is present.");
            IWebElement resultElement = driver.FindElement(locator);
            string resultText = resultElement.Text;
            Console.WriteLine($"Error occurred! {Environment.NewLine}'{resultText}'");
            Console.ReadKey();
        }
    }

    private static void CheckWirelineOrderDetailErrors(IWebDriver driver)
    {
        return;
        // Validate the result
        // Find the element by its locator
        const string IdToFind = "IsError";
        By locator = By.Id(IdToFind);
        int elementCount = driver.FindElements(locator).Count;
        if (elementCount > 0)
        {
            // Console.WriteLine($"Element '{IdToFind}' is present.");
            IWebElement resultElement = driver.FindElement(locator);
            string resultText = resultElement.Text;
            Console.WriteLine($"Error occurred! {Environment.NewLine}'{resultText}'");
            Console.ReadKey();
        }
    }

}

class Options : ICloneable
{
    // [Option('r', "read", Required = true, HelpText = "Input files to be processed.")]
    [Option('c', "chrome", HelpText = "Execute with Chrome browser.")]
    public bool BrowserChrome { get; set; }
    [Option('e', "edge", HelpText = "Execute with Edge browser.")]
    public bool BrowserEdge { get; set; }
    [Option('f', "firefox", HelpText = "Execute with Firefox browser.")]
    public bool BrowserFirefox { get; set; }

    [Option('s', "headless", HelpText = "Execute browser in background. Silent mode.")]
    public bool WithHeadless { get; internal set; }

    [Option('i', "instances-number", Default = 1, HelpText = "Number of instances for multiple execution. It will be executed the browsers in background. Silent mode.", Group = "numInstance")]
    public int WithMultipleInstancesNumber { get; internal set; }

    // // Omitting long name, defaults to name of property, ie "--verbose"
    // [Option(
    //   Default = false,
    //   HelpText = "Prints all messages to standard output.")]
    // public bool Verbose { get; set; }

    // [Option("stdin",
    //   Default = false,
    //   HelpText = "Read from stdin")]
    // public bool stdin { get; set; }

    // [Value(0, MetaName = "offset", HelpText = "File offset.")]
    // public long? Offset { get; set; }
    public string? Login { get; set; }

    public object Clone()
    {
        return new Options
        {
            BrowserChrome = this.BrowserChrome,
            BrowserEdge = this.BrowserEdge,
            BrowserFirefox = this.BrowserFirefox,
            Login = this.Login,
            WithHeadless = this.WithHeadless,
            WithMultipleInstancesNumber = this.WithMultipleInstancesNumber
        };
    }

}