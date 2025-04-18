using Blazorise;
using Blazorise.DataGrid;
using Localization.Resources.AbpUi;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Blazor.Components;
using Syrna.QuartzAdmin.ExecutionLog;
using Syrna.QuartzAdmin.ExecutionLog.Dtos;
using Syrna.QuartzAdmin.Localization;
using Syrna.QuartzAdmin.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Pages.QuartzAdmin.History
{
    public partial class History
    {
        [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
        [Inject] private IExecutionLogAppService LogSvc { get; set; } = null!;
        [Inject] private ISchedulerAppService SchedulerSvc { get; set; } = null!;
        [Inject] protected IStringLocalizer<AbpUiResource> UiLocalizer { get; set; }

        private IEnumerable<ExecutionLogDto> _pagedData;
        private DataGrid<ExecutionLogDto> _table = null!;

        private long _firstLogId;

        private int _totalItems;
        private readonly int _pageSize = 10;
        private bool _openFilter;

        private ExecutionLogFilter _filter = new();
        private ExecutionLogFilter _origFilter = new();
        private LogType? _selectedLogType;
        private IEnumerable<string> _jobNames = [];
        private IEnumerable<string> _jobGroups = [];
        private IEnumerable<string> _triggerNames = [];
        private IEnumerable<string> _triggerGroups = [];

        private async Task OnReadData()
        {
            var state = await _table.GetState();
            var pageMeta = _pagedData == null ? PageMetadata.New(0, state.PageSize) : new PageMetadata { Page = state.CurrentPage - 1, PageSize = state.PageSize };
            var args = new ExecutionLogReadArgs
            {
                Filter = _filter,
                PageMetadata = pageMeta,
                FirstLogId = _firstLogId
            };
            var data = await LogSvc.GetExecutionLogs(args);

            if (pageMeta.Page == 0)
            {
                _firstLogId = data.Items.FirstOrDefault()?.Id ?? 0;
            }

            _totalItems = (int)data.TotalCount;
            _pagedData = data.Items;
        }

        private async Task OnSearch(string text)
        {
            _filter.MessageContains = text;
            await RefreshLogs();
        }

        public async Task RefreshLogs()
        {
            _pagedData = null;
            _firstLogId = 0;
            await _table.ReadData.InvokeAsync();
        }

        private static (IconName, TextColor, string) GetLogIconAndColor(ExecutionLogDto log)
        {
            if (log.IsException ?? log.IsSuccess.HasValue && !log.IsSuccess.Value)
            {
                return (IconName.ExclamationCircle, TextColor.Danger, "Error");
            }

            switch (log.LogType)
            {
                case LogType.ScheduleJob:
                    if (log.IsVetoed ?? false)
                    {
                        return (IconName.InfoCircle, TextColor.Warning, "Vetoed");
                    }

                    return log.IsSuccess is null ?
                        // still running
                        (IconName.Palette, TextColor.Secondary, "Executing") : (IconName.Check, TextColor.Info, "Success");
                case LogType.Trigger:
                    return (IconName.Alert, TextColor.Warning, "Trigger");
                default:
                    return (IconName.Info, TextColor.Warning, "System Info");
            }
        }

        private async Task OnInterruptScheduleJob(ExecutionLogDto log)
        {
            if (log.FireInstanceId == null)
            {
                await Notify.Error(L["CannotInterruptJob"]);
                return;
            }

            if (await SchedulerSvc.InterruptJob(log.FireInstanceId))
            {
                await Notify.Info(UiLocalizer["InterruptJobSuccessfully"]);
            }
            else
            {
                await Notify.Error(L["InterruptJobFailed"]);
            }
        }

        private ExecutionDetailsDialog _executionDetailsDialogRef;
        private async Task OnMoreDetails(ExecutionLogDto log, string titleSuffix)
        {
            await _executionDetailsDialogRef.OpenModalAsync(log, titleSuffix);
        }

        public History()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }

        #region Filters
        private async Task OnFilterClicked()
        {
            // backup original filter
            _origFilter = (ExecutionLogFilter)_filter.Clone();

            if (!_jobNames.Any())
            {
                // load filter
                await ReloadFilters();
            }

            _openFilter = true;
        }

        private void OnSaveFilter()
        {
            _openFilter = false;
        }

        private async Task OnClearFilter()
        {
            _filter = new();
            await RefreshLogs();
            _openFilter = false;
        }

        private async Task OnCancelFilter()
        {
            _filter = _origFilter;
            await RefreshLogs();
            _openFilter = false;
        }

        private async Task ReloadFilters()
        {
            _jobNames = await LogSvc.GetJobNames();
            _jobGroups = await LogSvc.GetJobGroups();
            _triggerNames = await LogSvc.GetTriggerNames();
            _triggerGroups = await LogSvc.GetTriggerGroups();
        }

        private async Task OnFilterJobGroupChanged(string value)
        {
            _filter.JobGroup = value;
            await RefreshLogs();
        }

        private async Task OnFilterJobNameChanged(string value)
        {
            _filter.JobName = value;
            await RefreshLogs();
        }

        private async Task OnFilterTriggerGroupChanged(string value)
        {
            _filter.TriggerGroup = value;
            await RefreshLogs();
        }

        private async Task OnFilterTriggerNameChanged(string value)
        {
            _filter.TriggerName = value;
            await RefreshLogs();
        }

        private async Task OnSelectedLogTypesChanged(LogType? logTypes)
        {
            _selectedLogType = logTypes;
            if (logTypes == null)
            {
                _filter.LogTypes = null;
            }
            else
            {
                _filter.LogTypes = new HashSet<LogType> { logTypes.Value };
            }

            await RefreshLogs();
        }

        private async Task OnErrorOnlyChanged(bool errorOnly)
        {
            _filter.ErrorOnly = errorOnly;
            await RefreshLogs();
        }

        private async Task OnIncludeSystemJobsChanged(bool flag)
        {
            _filter.IncludeSystemJobs = flag;
            await RefreshLogs();
        }
        #endregion Filters
        //private Task OnPageChanged(DataGridPageChangedEventArgs args)
        //{
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //modalRef?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

