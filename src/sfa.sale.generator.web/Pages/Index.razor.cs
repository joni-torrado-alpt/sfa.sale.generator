using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using sfa.sale.generator.core;

namespace sfa.sale.generator.web.Pages;

public partial class Index : ComponentBase
{
    [Inject] public IDialogService? DialogService { get; set; }
    [Inject] public ISnackbar? Snackbar { get; set; }
    [Inject] public SfaDbContext? SfaContextRepository { get; set; }
    [Inject] public IJSRuntime? JSRuntime { get; set; }

    private MudDataGrid<SfaContext>? _dataGridRef;
    private SfaContext? _selectedSfaContext;
    private string? _search;
    private bool _isLoading;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private Task Refresh()
        => _dataGridRef!.ReloadServerData();

    private async Task GetLogs(DataGridRowClickEventArgs<SfaContext> e)
    {
        if (e.Item is not null)
        {
            //var parameters = new DialogParameters
            //{
            //    { nameof(SfaContextLogsGridDialog.SfaContextId), e.Item.Id }
            //};
            var options = new DialogOptions { CloseOnEscapeKey = true };
            //var dialog = DialogService.Show<SfaContextLogsGridDialog>("Logs", parameters, options);
            //await dialog.Result;
            await Refresh();
        }
    }

    private async Task<GridData<SfaContext>> GetSfaContextsData(GridState<SfaContext> state)
    {
        //Add default Sort by Id Desc
        var sortDef = state.SortDefinitions;
        if (sortDef is not null && sortDef.Count == 0) sortDef.Add(new(nameof(SfaContext.Id),true, 0, null));

        const string splitSearch = "|";
        var doWithCustomSearch = !string.IsNullOrEmpty(_search);
        var result = await SfaContextRepository!.SfaContext!
            .AsNoTracking()
            .EFOrderBySortDefinitions(state.SortDefinitions)
            // .Skip(state.Page * state.PageSize).Take(state.PageSize)
            .Where(x => !doWithCustomSearch || (doWithCustomSearch && (x.Id.ToString() + splitSearch + x.ClientIdType + splitSearch + x.ClientIdValue + splitSearch + x.Environment + splitSearch + x.CreatedBy + splitSearch).Contains(_search!)))
            .ToArrayAsync();
        return new GridData<SfaContext>()
        {
            Items = result
            //TotalItems = result.Count()
        };
    }

    private Task SearchChanged(string text)
    {
        _search = text;
        return Refresh();
    }
}