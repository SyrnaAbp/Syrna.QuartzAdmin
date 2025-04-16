using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Quartz;
using Syrna.QuartzAdmin.Blazor.Components;
using Syrna.QuartzAdmin.Blazor.Services;
using Syrna.QuartzAdmin.ExecutionLog;
using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Localization;
using Syrna.QuartzAdmin.Scheduler;
using Syrna.QuartzAdmin.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Components.Messages;

namespace Syrna.QuartzAdmin.Blazor.Pages.QuartzAdmin.Schedules
{
    public partial class Schedules : IDisposable
    {
        public Schedules()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }

        [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
        [Inject] private ISchedulerAppService SchedulerSvc { get; set; } = null!;
        [Inject] private IExecutionLogAppService ExecutionLogSvc { get; set; } = null!;
        [Inject] protected IUiMessageService UiMessageService { get; set; } = default!;

        private ObservableCollection<ScheduleModel> ScheduledJobs { get; set; } = new();
        private string _searchJobKeyword;
        private DataGrid<ScheduleModel> _scheduleDataGrid = null!;

        private bool _openFilter;

        private ScheduleJobFilter _filter = new();
        private ScheduleJobFilter _origFilter = new();

        internal bool IsEditActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobStatus == JobStatus.Running ||
            model.JobGroup == Constants.SYSTEM_GROUP;

        private bool IsRunActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
                                                                 model.JobStatus == JobStatus.NoTrigger;

        internal bool IsPauseActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
                                            model.JobStatus == JobStatus.Error ||
                                            model.JobStatus == JobStatus.NoTrigger;

        internal bool IsTriggerNowActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobStatus == JobStatus.Running;

        internal bool IsAddTriggerActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobGroup == Constants.SYSTEM_GROUP;

        internal bool IsCopyActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobGroup == Constants.SYSTEM_GROUP;

        internal bool IsHistoryActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule;

        internal bool IsDeleteActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.Running;

        readonly Func<ScheduleModel, object> _groupDefinition = x => x.JobGroup;

        static string GetTooltipText(ScheduleModel context) => $"<div style='max-width: 220px; overflow-wrap: break-word;'>{(!string.IsNullOrEmpty(context.ExceptionMessage) ? "Job has error." + context.ExceptionMessage : "Job has error.")}</div>";

        static string ExceptionMessageToolTipText(ScheduleModel context) =>
            $"<div style='max-width: 220px; overflow-wrap: break-word;'>{context.ExceptionMessage}</div>";

        string TriggerDetailToolTipText(ScheduleModel context) =>
            $"<div style='max-width: 220px; overflow-wrap: break-word;'>{context.TriggerDetail?.ToSummaryString(L)}</div>";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await RefreshJobs();
            }
        }

        private async Task RefreshJobs()
        {
            ScheduledJobs.Clear();

            var jobs = await SchedulerSvc.GetAllJobsAsync(_filter);
            foreach (var job in jobs)
            {
                ScheduledJobs.Add(job);
            }
            if (ScheduledJobs.Any())
            {
                _scheduleDataGrid?.ExpandAllGroups();
            }

            await UpdateScheduleModelsLastExecution();
        }

        private async Task UpdateScheduleModelsLastExecution()
        {
            var latestResult = PageMetadata.New(0, 1);
            var scheduleJobType = new HashSet<LogType> { LogType.ScheduleJob };

            foreach (var schModel in ScheduledJobs)
            {
                if (string.IsNullOrEmpty(schModel.JobName))
                {
                    continue;
                }

                LatestExecutionLogReadArgs latestExecutionLogReadArgs = new()
                {
                    JobName = schModel.JobName,
                    JobGroup = schModel.JobGroup,
                    TriggerName = schModel.TriggerName,
                    TriggerGroup = schModel.TriggerGroup,
                    PageMetadata = latestResult,
                    FirstLogId = 0,
                    LogTypes = scheduleJobType
                };
                var latestLogList = await ExecutionLogSvc.GetLatestExecutionLog(latestExecutionLogReadArgs);

                if (latestLogList != null && latestLogList.Items.Any())
                {
                    var latestLog = latestLogList.Items.First();
                    if (!schModel.PreviousTriggerTime.HasValue)
                    {
                        schModel.PreviousTriggerTime = latestLog.FireTimeUtc;
                    }
                    if (latestLog.IsSuccess.HasValue && !latestLog.IsSuccess.Value)
                    {
                        schModel.ExceptionMessage = latestLog.GetShortResultMessage();
                    }
                    else if (latestLog.IsException ?? false)
                    {
                        schModel.ExceptionMessage = latestLog.GetShortExceptionMessage();
                    }
                }
            }
        }

        ScheduleDialog ScheduleDialogRef;
        private async Task OnNewSchedule()
        {
            JobDetailModel jobDetail = new JobDetailModel();
            TriggerDetailModel triggerDetail = new TriggerDetailModel();
            await ScheduleDialogRef.OpenModalAsync(jobDetail, triggerDetail, true);
        }

        private async Task OnEditScheduleJob(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                await Notify.Error(L["CantEditScheduleJobExists"]);
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            if (currentJobDetail == null)
            {
                await Notify.Error(L["CantEditScheduleJobExists"]);
                return;
            }
            var origJobKey = Key.Create(currentJobDetail.Name, currentJobDetail.Group);

            TriggerDetailModel currentTriggerModel = null;
            Key origTriggerKey = null;
            if (model.TriggerName != null)
            {
                currentTriggerModel = await SchedulerSvc.GetTriggerDetail(model.TriggerName,
                    model?.TriggerGroup ?? Constants.DEFAULT_GROUP);

                if (currentTriggerModel != null)
                {
                    origTriggerKey = Key.Create(currentTriggerModel.Name, currentTriggerModel.Group);

                    ResetStartEndDateTimeIfEarlier(ref currentTriggerModel);
                }
            }

            await ScheduleDialogRef.OpenModalAsync(currentJobDetail, currentTriggerModel ?? new TriggerDetailModel(), false, ScheduleDialogTab.Job, false, origJobKey, origTriggerKey);
        }

        private async Task OnResumeScheduleJob(ScheduleModel model)
        {
            if (model.TriggerName == null)
            {
                await Notify.Error(L["CannotResumeSchedule"]);
                return;
            }

            await SchedulerSvc.ResumeTrigger(model.TriggerName, model.TriggerGroup);
        }

        private async Task OnPauseScheduleJob(ScheduleModel model)
        {
            if (model.TriggerName == null)
            {
                await Notify.Error(L["CannotPauseSchedule"]);
                return;
            }

            await SchedulerSvc.PauseTrigger(model.TriggerName, model.TriggerGroup);
        }

        private string DeleteConfirnationMessage(ScheduleModel item) => string.Format(L["DeleteConfirmationMessage"], item.JobName);

        private async Task OnDeleteScheduleJob(ScheduleModel model)
        {
            if (model.JobStatus == JobStatus.NoSchedule)
            {
                ScheduledJobs.Remove(model);
            }
            else
            {
                // confirm delete
                bool? yes = await UiMessageService.Confirm(DeleteConfirnationMessage(model));
                if (yes == null || !yes.Value)
                {
                    return;
                }

                var success = await SchedulerSvc.DeleteSchedule(model);

                if (!success)
                {
                    await Notify.Error(string.Format(L["FailedDeleteSchedule"], model.JobName));
                }
                else
                {
                    await Notify.Info(L["DeletedSuccessfully"]);
                }
            }
        }

        private async Task OnDuplicateScheduleJob(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                await Notify.Error(L["Error:CantCloneScheduleJobExists"]);
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            if (currentJobDetail == null)
            {
                await Notify.Error(L["Error:CantCloneScheduleJobExists"]);
                return;
            }

            TriggerDetailModel currentTriggerModel = null;
            if (model.TriggerName != null)
            {
                currentTriggerModel = await SchedulerSvc.GetTriggerDetail(model.TriggerName,
                    model?.TriggerGroup ?? Constants.DEFAULT_GROUP);
                if (currentTriggerModel != null)
                {
                    currentTriggerModel.Name = string.Empty;
                    ResetStartEndDateTimeIfEarlier(ref currentTriggerModel);
                }
            }

            currentJobDetail.Name = string.Empty;

            await ScheduleDialogRef.OpenModalAsync(currentJobDetail, currentTriggerModel ?? new(), true);
        }

        HistoryDialog HistoryDialogRef;
        private async Task OnJobHistory(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                // not possible?
                return;
            }
            await HistoryDialogRef.OpenModalAsync(Key.Create(model.JobName, model.JobGroup), model.TriggerName != null ? Key.Create(model.TriggerName, model.TriggerGroup ?? Constants.DEFAULT_GROUP) : null);
        }

        private async Task OnTriggerNow(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                await Notify.Error(L["Error:CantAddTriggerJobExists"]);
                return;
            }

            await SchedulerSvc.TriggerJob(model.JobName, model.JobGroup);
        }

        private async Task OnAddTrigger(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                await Notify.Error(L["Error:CantAddTriggerJobExists"]);
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            TriggerDetailModel triggerDetail = new TriggerDetailModel();
            await ScheduleDialogRef.OpenModalAsync(currentJobDetail, triggerDetail, false, ScheduleDialogTab.Trigger, true);
        }

        private string DeleteConfirnationMessage(List<ScheduleModel> items) => string.Format(L["SchedulesDeleteConfirmationMessage"], items.Count);

        private async Task OnDeleteSelectedScheduleJobs()
        {
            if (_scheduleDataGrid is null)
            {
                return;
            }

            var selectedItems = _scheduleDataGrid.SelectedRows;

            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            // confirm delete
            bool? yes = await UiMessageService.Confirm(DeleteConfirnationMessage(selectedItems));
            if (yes == null || !yes.Value)
            {
                return;
            }

            var skipCount = 0;

            var deleteTasks = selectedItems.Select(model =>
            {
                if (model.JobStatus == JobStatus.Running)
                {
                    skipCount++;
                    return Task.FromResult(true);
                }

                ScheduledJobs.Remove(model);
                return SchedulerSvc.DeleteSchedule(model);
            });
            var results = await Task.WhenAll(deleteTasks);

            if (results == null)
            {
                await RefreshJobs();
                await Notify.Error("Failed to delete schedules");
            }
            else
            {
                var deletedCount = results.Count(t => t);
                var notDeletedCount = results.Count() - deletedCount - skipCount;

                if (skipCount > 0)
                {
                    await Notify.Info($"Deleted {deletedCount} schedule(s). Skip {skipCount} executing schedule(s)");
                }
                else
                {
                    await Notify.Info($"Deleted {deletedCount} schedule(s)");
                }

                if (notDeletedCount > 0)
                {
                    await RefreshJobs();
                    await Notify.Warn($"Failed to deleted {notDeletedCount} schedule(s)");
                }
            }
        }

        private static void ResetStartEndDateTimeIfEarlier(ref TriggerDetailModel triggerModel)
        {
            var startDateTime = triggerModel.StartDateTimeUtc;
            if (startDateTime.HasValue && startDateTime <= DateTimeOffset.UtcNow)
            {
                // clear start date if already past
                triggerModel.StartTimeSpan = null;
                triggerModel.StartDate = null;
                triggerModel.StartTimezoneId = TimeZoneInfo.Utc.Id;
            }

            var endTime = triggerModel.EndDateTimeUtc;
            if (endTime.HasValue && endTime <= DateTimeOffset.UtcNow)
            {
                // clear end date if already past
                triggerModel.EndDate = null;
                triggerModel.EndTimeSpan = null;
            }
        }

        #region Filter
        private void OnFilterClicked()
        {
            // backup original filter
            _origFilter = (ScheduleJobFilter)_filter.Clone();

            _openFilter = true;
        }

        private void OnSaveFilter()
        {
            _openFilter = false;
        }

        private async Task OnClearFilter()
        {
            _filter = new();
            await RefreshJobs();
            _openFilter = false;
        }

        private async Task OnCancelFilter()
        {
            _filter = _origFilter;
            await RefreshJobs();
            _openFilter = false;
        }

        private async Task OnIncludeSystemJobsChanged(bool value)
        {
            _filter.IncludeSystemJobs = value;
            await RefreshJobs();
        }
        #endregion Filter
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //HistoryDialogRef.Dispose(disposing);
                //ScheduleDialogRef.Dispose(disposing);
                //UnRegisterEventListeners();
            }
            base.Dispose(disposing);
        }
    }
}

