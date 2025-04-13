using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Components;

public partial class CronSamplesDialog
{
    [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }

    Modal modalRef;

    public Func<string, Task> Save { get; set; }

    private readonly List<string> _cronSamples =
    [
        "0 15 10 ? * *",
        "0 * 14 * * ?",
        "0 0/5 10 ? * MON-FRI",
        "0 15 10 ? * 6L",
        "0 15 10 ? * 6#3",
        "0 15 10 L-2 * ?"
    ];

    public CronSamplesDialog()
    {
        LocalizationResource = typeof(QuartzAdminResource);
    }

    private static string GetCronDescription(string cron)
    {
        var options = new CronExpressionDescriptor.Options()
        {
            ThrowExceptionOnParseError = false,
            Verbose = false,
            DayOfWeekStartIndexZero = true,
            Locale = CultureInfo.DefaultThreadCurrentCulture?.ToString()
        };
        return $"{cron} ({CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cron, options)})";
    }

    private async Task OnSelectExpression(string cronExpression)
    {
        await Save!.Invoke(cronExpression);
        await modalRef.Hide();
    }

    public async Task OpenModalAync(Func<string, Task> save)
    {
        Save = save;
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