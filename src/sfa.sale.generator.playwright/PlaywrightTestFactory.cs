using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using sfa.sale.generator.core;
using Xunit.Abstractions;

public class PlaywrightTestFactory : IAsyncDisposable
{
    private static SfaContext? _sfaContext;
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static IPage? _page;
    private static ITestOutputHelper? _outputHelper;
    private static string? _url { get; set; }
    private static string _urlScriptVendas => $"{_url}/Scripts/ScriptVendas/wireline.aspx";
    private static bool _forceExecution;

    public static async Task InitAsync(SfaContextInput input, ITestOutputHelper? outputHelper = null)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));
        _outputHelper = outputHelper;

        _url = input.EnvURL;
        _forceExecution = input.ForceExecution;
        var sfaLoginPassword = input.LoginPassword;
        _sfaContext = SFAContextFactory.LoadSfaContext(input);

        await SaveInitialContext();
        if (_sfaContext.IsCompleted) return;
        _sfaContext.LoginPassword = sfaLoginPassword;

        var args = Environment.GetCommandLineArgs();
        var browserHeadless = args.Contains("headless");

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = browserHeadless,
        });
        _page = await _browser.NewPageAsync();
    }

    public static async Task RunAsync()
    {
        if (_sfaContext?.IsCompleted ?? false)
        {
            SfaSale? saleUrl = _sfaContext.SfaSales?.FirstOrDefault(x => x.IsCompleted);
            TextCopy.ClipboardService.SetText(saleUrl!.Url);
            return;
        }

        if (_page is null || _browser is null) return;

        Exception? hasException = null;
        try
        {
            await PerformLogin();
            await PerformOrder();

            //Assertion
            var productBasketElement = _page.Locator("div#Products");
            await Assertions.Expect(productBasketElement).ToBeVisibleAsync();
        }
        catch (System.Exception ex)
        {
            hasException = ex;
            await _page.PauseAsync();
            throw;
        }
        finally
        {
            await SetInfoToOutputConsole(hasException);
        }
    }

    private static async Task SaveInitialContext()
    {
        if (_sfaContext is null) return;
        using var context = new SfaDbContext(DbOptionsFactory.DbContextOptions);
        var sfaContextExists = await context.SfaContext!.FirstOrDefaultAsync(s => s.ClientIdValue.Equals(_sfaContext.ClientIdValue)
                                                                                && s.Offer.PromoId == _sfaContext.Offer.PromoId
                                                                                && s.Environment == _sfaContext.Environment);
        if (sfaContextExists is null || _forceExecution)
        {
            var insertedContext = context.SfaContext!.Add(_sfaContext);
            _sfaContext.Id = insertedContext is null ? _sfaContext.Id : insertedContext.Entity.Id;
            await context.SaveChangesAsync();
        }
        else
        {
            context.Update(sfaContextExists);
            await context.SaveChangesAsync();
            _sfaContext = await context.FindWithLoadNavigationsAsync<SfaContext>(sfaContextExists.Id);
            _sfaContext.Offer = await context.FindWithLoadNavigationsAsync<SfaContextOffer>(_sfaContext.Offer.Id);
        }

    }

    private static async Task PerformLogin()
    {
        if (_page is null || _sfaContext is null) return;
        
        await _page.GotoAsync($"{_url}/seguranca/login.aspx");

        //Impersonate
        if (_sfaContext.Environment == "PROD")
        {
            await _page.Locator("#txtUserName").FillAsync("jtorrado");
            await _page.Locator("#txtUserName").PressAsync("Tab");
            await _page.Locator("#txtPassword").FillAsync("Rcpgen!?411");
            await _page.Locator("#txtPassword").PressAsync("Enter", new() { Timeout = 90000 });

            await _page.GotoAsync($"{_url}/Modulos/MasterSlavePageManagerFull.aspx?ModuloID=9&idx=50");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Perfis" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Todos os Logins" }).ClickAsync();
            await _page.Locator("input[name=\"TodososLogins\\$GridToolBar\\$ctl03\"]").ClickAsync();
            await _page.Locator("#TodososLogins_Query_QueryGrid_cmbQueryField_0").SelectOptionAsync(new[] { "Login" });
            await _page.Locator("#TodososLogins_Query_QueryGrid_txtQueryValue_0").ClickAsync();
            await _page.Locator("#TodososLogins_Query_QueryGrid_txtQueryValue_0").FillAsync("hcosta_emp");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Pesquisar" }).ClickAsync();
            await _page.Locator("input[name=\"TodososLogins\\$GridToolBar\\$ctl05\"]").ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Impersonate" }).ClickAsync();
        }
        else
        {
            await _page.Locator("#txtUserName").FillAsync(_sfaContext.LoginId);
            await _page.Locator("#txtUserName").PressAsync("Tab");
            await _page.Locator("#txtPassword").FillAsync(_sfaContext.LoginPassword);
            await _page.Locator("#txtPassword").PressAsync("Enter", new() { Timeout = 90000 });
        }
        //await Expect(page.Url.Contains("Home.aspx"));
    }

    private static async Task PerformOrder()
    {
        if (_page is null || _sfaContext is null) return;
        await _page.GotoAsync(_urlScriptVendas, new() { Timeout = 60000 });

        //Search Client:=> Campaign, Email and MultiClient validation too
        await PerformOrderClientSearch();
        await PerformOrderMasterUser();

        // Select Client Tree
        if (_sfaContext.Offer.IsNewInstallation)
        {
            var treeElement = _page.Locator("#TreeDiv");
            await treeElement.Locator("//img[contains(@src, 'CriarNovaContaFaturacao.gif')]").ClickAsync();
            var eniDropDownElement = treeElement.Locator("#ENI001-101");
            if (await eniDropDownElement.IsVisibleAsync() && await eniDropDownElement.IsEditableAsync())
            {
                await eniDropDownElement.SelectOptionAsync(_sfaContext.Offer.MacroSegment.Equals("CRP") ? "R" : "B");
            }
            await treeElement.Locator("#RemIma001-101").ClickAsync();
        }
        else
        {
            var treeElement = _page.Locator("#TreeDiv");
            var selectTreeElement = treeElement.Locator(@$"//input[@value='{_sfaContext.ClientIdValue}']/ancestor::td/img[contains(@src, 'bt_Seleccionar_nsom.gif')]");
            await selectTreeElement.ClickAsync();
        }

        // Select Offer Familly
        var familiesElement = _page.Locator("#ProductFamilies");
        foreach (var item in _sfaContext.Offer.Families)
        {
            var familyElement = familiesElement.Locator($"//input[@type='radio' and @value='{item.FamilyId}']");
            var familyTVElement = familiesElement.GetByLabel("Produtos Televisão");
            var performAddresValidation = await familyTVElement.IsVisibleAsync() && !await familyElement.IsVisibleAsync();
            if (performAddresValidation)
            {
                await familyTVElement.ClickAsync();
            }

            if (!await familyElement.IsCheckedAsync())
            {
                await familyElement.CheckAsync();
                await _page.WaitForResponseAsync("**/AjaxDTH2.aspx");
            }

            await PerformAddressViabilityAsync();
        }
        // Search Offer
        await PerformOrderSearchOffer();
    }

    private static async Task PerformOrderClientSearch()
    {
        if (_page is null || _sfaContext is null) return;
        await _page.Locator("#DocumentType").SelectOptionAsync(_sfaContext.ClientIdType);
        var clientInputValue = _page.GetByPlaceholder("Insira o termo a pesquisar");
        await clientInputValue.ClickAsync();
        await clientInputValue.FillAsync(_sfaContext.ClientIdValue);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Procurar" }).ClickAsync(new() { Timeout = 60000 });

        await _page.WaitForResponseAsync("**/CustomerInputRulesAjax.aspx", new() { Timeout = 60000 });

        var emailPopUpElement = _page.Locator("#EmailClientSyncPopupWindow");
        if (await emailPopUpElement.IsVisibleAsync())
        {
            if (string.IsNullOrWhiteSpace(await emailPopUpElement.Locator("#EmailClientSyncPopupEmail").InputValueAsync()))
            {
                await emailPopUpElement.Locator("[id=\"Order\\.EmailClientSyncPopup\\.EmailClientSyncPopupNoEmailChecked\"]").CheckAsync();
            }
            await emailPopUpElement.GetByText("Aceitar").ClickAsync();
        }

        await PerformOrderCampaign();

        // In case client's don't have all identification values on CRM
        var docTypeElement = _page.Locator("#PrincipalDocumentType");
        var isDocTypeValueMissed = string.IsNullOrEmpty(await docTypeElement.InputValueAsync()) && await docTypeElement.IsEnabledAsync();
        if (isDocTypeValueMissed)
        {
            await docTypeElement.SelectOptionAsync("1");
            await _page.Locator("#PrincipalDocumentValue").FillAsync("9496747"); //TODO: Add a variable
        }

        //In case client's contacts are invalid... fill only the mobile and email
        var mainClientContactElement = _page.Locator("#MainContact");
        var mainClientMobileElement = mainClientContactElement.Locator("#ClientMobile");
        var mainClientPhoneElement = mainClientContactElement.Locator("#ClientPhone");
        var mainContactElementsWithError = await mainClientContactElement.Locator(":has(input.set-error-in-input)").ElementHandlesAsync();

        bool isClientWithContactsFilled = false;
        if (string.IsNullOrEmpty(await mainClientPhoneElement.InputValueAsync()) && string.IsNullOrEmpty(await mainClientMobileElement.InputValueAsync()))
        {
            isClientWithContactsFilled = true;
        }
        else if (mainContactElementsWithError.Any())
        {
            isClientWithContactsFilled = true;
        }

        if (isClientWithContactsFilled)
        {
            await mainClientPhoneElement.FillAsync(""); //TODO: Create a variable
            await mainClientMobileElement.FillAsync("966013492"); //TODO: Create a variable
            var mainClientEmailElement = mainClientContactElement.Locator("#ClientEmail");
            if (await mainClientEmailElement.IsEditableAsync())
            {
                await mainClientEmailElement.FillAsync("KAUT-UI10@remax.pt"); //TODO: Create a variable
            }
        }

        await _page.GetByRole(AriaRole.Button, new() { Name = "Validar Dados Validar Dados" }).ClickAsync();
        // await _page.WaitForResponseAsync("**/wireline.aspx");

        var clientMultiIdentifiersElement = _page.Locator("#CustomerSearchResults");
        if (await clientMultiIdentifiersElement.IsVisibleAsync())
        {
            await clientMultiIdentifiersElement.GetByRole(AriaRole.Button, new() { Name = "Fechar", Exact = true }).ClickAsync();
        }
    }

    private static async Task PerformOrderCampaign()
    {
        if (_page is null || _sfaContext is null) return;
        var vendorInput = _page.Locator("#vendedr_text");
        await vendorInput.ClickAsync();
        await vendorInput.TypeAsync(_sfaContext.Offer.SalesAgent);
        await _page.GetByText(_sfaContext.Offer.SalesAgent).ClickAsync();

        var campaignInput = _page.Locator("#Multicanal_InfoVenda");
        await campaignInput.GetByRole(AriaRole.Textbox).Nth(3).ClickAsync();
        await campaignInput.TypeAsync(_sfaContext.Offer.Campaign);
        await _page.GetByRole(AriaRole.Menuitem, new() { Name = _sfaContext.Offer.Campaign, Exact = true }).Locator("a").ClickAsync();

        var passworInput = _page.GetByLabel("Password");
        await passworInput.ClickAsync();
        await passworInput.FillAsync(_sfaContext.Offer.CampaignPassword);
    }

    private static async Task PerformOrderMasterUser()
    {
        if (_page is null || _sfaContext is null) return;
        var masterUserElement = _page.Locator("#MasterUser");
        await Task.Delay(500);
        if (!await masterUserElement.IsVisibleAsync()) return;

        var masterUserNameElement = masterUserElement.Locator("#MasterUserName");
        if (await masterUserNameElement.IsEditableAsync())
        {
            await masterUserNameElement.FillAsync(_sfaContext!.MasterUser!.Name);
            await masterUserElement.Locator("#MasterUserPhone").FillAsync(_sfaContext.MasterUser.Telephone);
            await masterUserElement.Locator("#MasterUserMobile").FillAsync(_sfaContext.MasterUser.Mobile);
            await masterUserElement.Locator("#MasterUserDIF").FillAsync(_sfaContext.MasterUser.NIF);
            await masterUserElement.Locator("#MasterUserDIP").FillAsync(_sfaContext.MasterUser.BI);
            await masterUserElement.Locator("#MasterUserEmail").FillAsync(_sfaContext.MasterUser.Email);
        }
        else if (string.IsNullOrEmpty(await masterUserNameElement.InputValueAsync())) // Case MasterUser info is empty
        {
            // await masterUserElement.Locator("#Order.MasterUser.doSearchMasterUserAction").ClickAsync();
            await masterUserElement.GetByRole(AriaRole.Button, new() { Name = "Escolha um administrador de conta" }).ClickAsync();
            await _page.WaitForResponseAsync("**/CustomerInputRulesAjax.aspx");

            var rows = await _page.QuerySelectorAllAsync("#MasterUserChooserList");
            foreach (var row in rows)
            {
                // Get all cells in the row
                IReadOnlyList<IElementHandle> cells = await row.QuerySelectorAllAsync("td");

                // Check if all cells in the row have the same value
                var allCellsHaveValue = cells.All(cell =>
                {
                    if (cells.First() == cell) return true; //Ignore checkbox
                    return !string.IsNullOrWhiteSpace(cell.InnerTextAsync().Result);
                });

                if (allCellsHaveValue)
                {
                    Console.WriteLine("Found a row where all cells have value.");
                    await row.ClickAsync();
                    await _page.Locator("#MasterUserChooserPopup").GetByText("Aceitar").ClickAsync();
                    break;
                }
            }
        }
    }

    private static async Task PerformAddressViabilityAsync()
    {
        if (_page is null || _sfaContext is null) return;
        var verifyCoverageElement = _page.Locator("#CoverageVerify");
        if (!await verifyCoverageElement.IsVisibleAsync()) return;
        var coverageResultsElement = _page.Locator("#CoverageResults");
        bool isResultDone = await coverageResultsElement.Locator("#ResultAdslWithCoverage").IsVisibleAsync() || await coverageResultsElement.Locator("#ResultTvWithCoverage").IsVisibleAsync();
        if (isResultDone) return;

        var addessTextAreaElement = verifyCoverageElement.Locator("#AddressPanel");
        var isAddressFulfilled = string.IsNullOrEmpty(await addessTextAreaElement.Locator(".sfa-textarea-address").InputValueAsync());
        if (!isAddressFulfilled)
        {
            await _page.GetByText("Editar Morada").ClickAsync();
            var addressDetail = _page.Locator("#addressDetails");
            await addressDetail.GetByText("Pesquisa CP7").ClickAsync();

            //CP4
            var cp4DropDownElement = addressDetail.Locator("//div[@class='select2-container']");
            await cp4DropDownElement.ClickAsync();
            await _page.Locator("#select2-drop").GetByRole(AriaRole.Textbox).FillAsync(_sfaContext.ClientAddress.CP4.ToString());
            await _page.GetByText(_sfaContext.ClientAddress.CP4.ToString(), new() { Exact = true }).ClickAsync();
            //CP3
            var cp3DropDownElement = addressDetail.Locator("//div[@class='select2-container']");
            await cp3DropDownElement.ClickAsync();
            await _page.Locator("#select2-drop").GetByRole(AriaRole.Textbox).FillAsync(_sfaContext.ClientAddress.CP3.ToString());
            await _page.GetByText(_sfaContext.ClientAddress.CP3.ToString(), new() { Exact = true }).ClickAsync();

            //Morada
            await addressDetail.GetByText(_sfaContext.ClientAddress.Name, new() { Exact = true }).ClickAsync();

            //NPolicia, Floor, fraction
            await addressDetail.GetByRole(AriaRole.Button, new() { Name = "Filtrar" }).ClickAsync();
            await _page.WaitForResponseAsync("**/WebApi/Address/GetNumPolicia*", new() { Timeout = 61000 });
            var useAddressButtonElement = addressDetail.GetByRole(AriaRole.Button, new() { Name = "Utilizar Morada" });
            List<string> validateFields = new();
            int countIterations = 0;
            while (validateFields.Count() < 3 && countIterations < 3)
            {
                if (!string.IsNullOrWhiteSpace(_sfaContext.ClientAddress.PoliceNumber) && !validateFields.Any(f => f.Equals(nameof(_sfaContext.ClientAddress.PoliceNumber))))
                {
                    var nPoliceDropElement = addressDetail.GetByRole(AriaRole.Group).Locator("div").Filter(new() { HasText = "N. Polícia: " }).GetByRole(AriaRole.Combobox);
                    var nPoliceValues = await nPoliceDropElement.SelectOptionAsync(_sfaContext.ClientAddress.PoliceNumber);
                    if (await useAddressButtonElement.IsVisibleAsync()) await useAddressButtonElement.ClickAsync();
                    validateFields.Add(nameof(_sfaContext.ClientAddress.PoliceNumber));
                }
                else if (!string.IsNullOrWhiteSpace(_sfaContext.ClientAddress.Floor) && !validateFields.Any(f => f.Equals(nameof(_sfaContext.ClientAddress.Floor))))
                {
                    var floorDropElement = addressDetail.GetByRole(AriaRole.Group).Locator("div").Filter(new() { HasText = "Andar: " }).GetByRole(AriaRole.Combobox);
                    var floorValues = await floorDropElement.SelectOptionAsync(_sfaContext.ClientAddress.Floor);
                    if (await useAddressButtonElement.IsVisibleAsync()) await useAddressButtonElement.ClickAsync();
                    validateFields.Add(nameof(_sfaContext.ClientAddress.Floor));
                }
                else if (!string.IsNullOrWhiteSpace(_sfaContext.ClientAddress.Fraction) && !validateFields.Any(f => f.Equals(nameof(_sfaContext.ClientAddress.Fraction))))
                {
                    var fractionDropElement = addressDetail.GetByRole(AriaRole.Group).Locator("div").Filter(new() { HasText = "Fração: " }).GetByRole(AriaRole.Combobox);
                    var fractionValues = await fractionDropElement.SelectOptionAsync(_sfaContext.ClientAddress.Fraction);
                    if (await useAddressButtonElement.IsVisibleAsync()) await useAddressButtonElement.ClickAsync();
                    validateFields.Add(nameof(_sfaContext.ClientAddress.Fraction));
                }
                countIterations++;
            }
        }

        var verifyViabilityButtonElement = _page.GetByRole(AriaRole.Button, new() { Name = "Verificar Viabilidade" });
        await verifyViabilityButtonElement.ClickAsync();
        await _page.WaitForResponseAsync("**/AjaxDTH2.aspx", new() { Timeout = 70000 });
    }

    private static async Task PerformOrderSearchOffer()
    {
        if (_page is null || _sfaContext is null) return;
        var categoryElement = _page.Locator("#SearchProductCategory");
        await categoryElement.SelectOptionAsync(new[] { _sfaContext.Offer.Category });
        var offerSearchInput = _page.Locator("#SearchProductSearchKey");
        await offerSearchInput.ClickAsync();
        await offerSearchInput.FillAsync(_sfaContext.Offer.PromoId.ToString());

        await _page.GetByRole(AriaRole.Button, new() { Name = "Pesquisar" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex($"{_sfaContext.Offer.PromoId}", RegexOptions.IgnoreCase) }).ClickAsync();

        await _page.GetByRole(AriaRole.Button, new() { Name = "Adicionar", Exact = true }).ClickAsync();

        await Task.Delay(500);
        var pop = _page.Locator("#WarningWindow");
        if (await pop.IsVisibleAsync())
        {
            var yesElement = pop.GetByRole(AriaRole.Button, new() { NameString = "Sim" });
            if (await yesElement.IsVisibleAsync())
            {
                // await _page.PauseAsync();
                await yesElement.ClickAsync();
            }
            else
            {
                await Assertions.Expect(yesElement).ToBeVisibleAsync();
            }
        }
    }

    private static async Task SetInfoToOutputConsole(Exception? exception)
    {
        if (_page is null) return;
        var hasException = exception is not null;
        var guidElement = _page.Locator("label#Guid");
        string? guid = string.Empty;
        string guidURL = string.Empty;
        if (await guidElement.IsVisibleAsync())
        {
            guid = await guidElement.TextContentAsync();
            guidURL = $"{_urlScriptVendas}?guid={guid}";
            _outputHelper?.WriteLine($"User: {_sfaContext?.LoginId}");
            _outputHelper?.WriteLine(guidURL);
            // _outputHelper.WriteLine($"_sfaContext.Id = {_sfaContext?.Id}");
            if (!hasException) TextCopy.ClipboardService.SetText(guidURL);
        }

        TimeSpan duration = TimeSpan.MinValue;
        try
        {
            using (var context = new SfaDbContext(DbOptionsFactory.DbContextOptions))
            {
                var currentSfaContext = await context!.SfaContext!.FindAsync(_sfaContext?.Id);
                if (currentSfaContext is null) return;
                duration = DateTime.Now - (_sfaContext?.CreatedOn ?? DateTime.Now.AddHours(-1));
                currentSfaContext.IsCompleted = !hasException;

                if (hasException)
                {
                    context!.SfaLog!.Add(new() { Message = exception?.Message, MessageDump = exception?.ToString(), SfaContextId = _sfaContext?.Id ?? -2, Type = SfaLogTypeEnum.ERROR });
                }

                if (string.IsNullOrEmpty(guidURL))
                {
                    //Save to LOG
                    context!.SfaLog!.Add(new() { Message = $"Could not save data with no GUID: guid='{guid!}', guidURL='{guidURL}', duration='{duration}', hasException='{!hasException}'", MessageDump = exception?.ToString(), SfaContextId = _sfaContext?.Id ?? -2, Type = SfaLogTypeEnum.WARNING });
                }
                else
                {
                    currentSfaContext.SfaSales = new List<SfaSale> { new(guid!, guidURL, duration, currentSfaContext.IsCompleted) };
                }
                await context.SaveChangesAsync();
            }
        }
        catch (System.Exception ex)
        {
            using (var context = new SfaDbContext(DbOptionsFactory.DbContextOptions))
            {
                context!.SfaLog!.Add(new() { Message = $"Could not save data: guid='{guid!}', guidURL='{guidURL}', duration='{duration}', hasException='{!hasException}'", MessageDump = ex.ToString(), SfaContextId = _sfaContext?.Id ?? -2, Type = SfaLogTypeEnum.ERROR });
                await context.SaveChangesAsync();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}