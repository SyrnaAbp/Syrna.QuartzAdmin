using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Blazor.Components;
using Syrna.QuartzAdmin.ExecutionLog;
using Syrna.QuartzAdmin.ExecutionLog.Dtos;
using Syrna.QuartzAdmin.Localization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Pages.QuartzAdmin.Schedules;

public partial class HistoryDialog
{
    [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
    [Inject] private IExecutionLogAppService LogSvc { get; set; } = null!;

    [EditorRequired]
    public Key JobKey { get; set; } = null!;

    [EditorRequired]
    public Key TriggerKey { get; set; }

    private ObservableCollection<ExecutionLogDto> ExecutionLogs { get; } = new();
    private bool HasMore { get; set; }

    private PageMetadata _lastPageMeta;
    private long _firstLogId;
    Modal modalRef;

    public HistoryDialog()
    {
        LocalizationResource = typeof(QuartzAdminResource);
    }

    public async Task OpenModalAsync(Key jobKey, Key triggerKey)
    {
        JobKey = jobKey;
        TriggerKey = triggerKey;
        await modalRef.Show();
        await OnRefreshHistory();
    }

    protected async Task Close()
    {
        await modalRef.Hide();
    }

    private async Task GetMoreLogs()
    {
        PageMetadata pageMeta;
        if (_lastPageMeta == null)
        {
            pageMeta = PageMetadata.New(0, 5);
        }
        else
        {
            pageMeta = new PageMetadata { Page = _lastPageMeta.Page + 1, PageSize = _lastPageMeta.PageSize, TotalCount = _lastPageMeta.TotalCount };
        }
        LatestExecutionLogReadArgs latestExecutionLogReadArgs = new()
        {
            JobName = JobKey.Name,
            JobGroup = JobKey.Group ?? Constants.DEFAULT_GROUP,
            TriggerName = TriggerKey?.Name,
            TriggerGroup = TriggerKey?.Group,
            PageMetadata = pageMeta,
            FirstLogId = _firstLogId
        };
        var result = await LogSvc.GetLatestExecutionLog(latestExecutionLogReadArgs);
        var items = result.Items.ToList();
        if (pageMeta.Page == 0)
        {
            _firstLogId = items.FirstOrDefault()?.Id ?? 0;
        }

        items.ForEach(ExecutionLogs.Add);

        HasMore = result.TotalCount == pageMeta.PageSize;
    }

    private async Task OnRefreshHistory()
    {
        ExecutionLogs.Clear();
        _lastPageMeta = null;
        _firstLogId = 0;
        HasMore = false;

        await GetMoreLogs();
    }

    ExecutionDetailsDialog ExecutionDetailsDialogRef;
    private async Task OnMoreDetails(ExecutionLogDto log, string titleSuffix)
    {
        await ExecutionDetailsDialogRef.OpenModalAsync(log, titleSuffix);
    }

    private static string GetExecutionTime(ExecutionLogDto log)
    {
        // when fire time is available, display time range
        // otherwise just display date added
        if (log.FireTimeUtc.HasValue)
        {
            StringBuilder strBuilder = new(log.FireTimeUtc.Value.LocalDateTime.ToShortDateString() +
                    " " +
                    log.FireTimeUtc.Value.LocalDateTime.ToLongTimeString());

            var finishTime = log.GetFinishTimeUtc();
            if (finishTime.HasValue)
            {
                strBuilder.Append(" - ");
                if (finishTime.Value.LocalDateTime.Date != log.FireTimeUtc.Value.LocalDateTime.Date)
                {
                    // display ending date
                    strBuilder.Append(finishTime.Value.LocalDateTime.ToShortDateString() + " ");
                }

                strBuilder.Append(finishTime.Value.LocalDateTime.ToLongTimeString());
            }
            return strBuilder.ToString();
        }
        else
        {
            return log.DateAddedUtc.LocalDateTime.ToShortDateString() + " " +
                log.DateAddedUtc.LocalDateTime.ToLongTimeString();
        }
    }

    private static Color GetTimelineDotColor(ExecutionLogDto log)
    {
        return log.LogType switch
        {
            LogType.ScheduleJob => log.IsException ?? false ? Color.Danger : Color.Success,
            _ => Color.Default
        };
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

