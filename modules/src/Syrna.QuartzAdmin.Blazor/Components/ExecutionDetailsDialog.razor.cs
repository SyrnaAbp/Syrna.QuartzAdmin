using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.ExecutionLog.Dtos;
using Syrna.QuartzAdmin.Localization;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Components;

public partial class ExecutionDetailsDialog
{
    [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }

    Modal modalRef;

    public ExecutionLogDto ExecutionLog { get; set; } = new();
    public string TitleSuffix { get; set; } = "ExecutionDetails";

    public ExecutionDetailsDialog()
    {
        LocalizationResource = typeof(QuartzAdminResource);
    }

    public async Task OpenModalAsync(ExecutionLogDto executionLog, string titleSuffix)
    {
        ExecutionLog = executionLog;
        TitleSuffix = titleSuffix;
        await modalRef.Show();
    }

    protected async Task Close()
    {
        await modalRef.Hide();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            modalRef?.Dispose();
        }
        base.Dispose(disposing);
    }
}