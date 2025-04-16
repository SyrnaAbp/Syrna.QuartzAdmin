using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Localization;
using Syrna.QuartzAdmin.Scheduler;
using Syrna.QuartzAdmin.Triggers;
using System;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Components;

public partial class ScheduleDialog
{
    [Inject]
    protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
    [Inject] private ISchedulerAppService SchedulerSvc { get; set; } = null!;
    public JobDetailModel JobDetail { get; set; } = new();
    [Parameter] public TriggerDetailModel TriggerDetail { get; set; } = new();
    public bool IsNew { get; set; }
    [Parameter] public Key JobKey { get; set; }
    [Parameter] public Key TriggerKey { get; set; }
    [Parameter] public bool IsReadOnlyJobDetail { get; set; }
    [Parameter] public ScheduleDialogTab SelectedTab { get; set; } = ScheduleDialogTab.Job;

    private bool _jobDetailIsValid;
    private bool _triggerDetailIsValid;
    private string _nextText;
    private string _nextIcon = IconName.AngleRight.ToString();
    private BlazoriseJob _jobPanel = null!;
    private BlazoriseTrigger _triggerPanel = null!;
    Modal modalRef;

    public ScheduleDialog()
    {
        LocalizationResource = typeof(QuartzAdminResource);
    }

    protected override void OnInitialized()
    {
        _nextText = L["Next"];
        if (SelectedTab == ScheduleDialogTab.Trigger)
        {
            _jobDetailIsValid = true;
            _nextText = @L["Save"];
            _nextIcon = null;
        }
    }

    private async Task OnSelectedTabChanged(ScheduleDialogTab tab)
    {
        if (SelectedTab == tab)
            return;

        // validate before change tab
        if (SelectedTab == ScheduleDialogTab.Job)
        {
            await _jobPanel.Validate();
            if (!_jobDetailIsValid)
                return;
        }

        SelectedTab = tab;

        // update text
        if (SelectedTab == ScheduleDialogTab.Job)
        {
            _nextText = @L["Next"];
            _nextIcon = IconName.AngleLeft.ToString();
        }
        else if (SelectedTab == ScheduleDialogTab.Trigger)
        {
            if (string.IsNullOrEmpty(TriggerDetail.Name) &&
                !string.IsNullOrEmpty(JobDetail.Name))
            {
                // use job name as trigger name when trigger name not yet specified
                // determine if trigger name can be used
                var exists = await SchedulerSvc.ContainsTriggerKey(JobDetail.Name, TriggerDetail.Group);
                if (!exists)
                    TriggerDetail.Name = JobDetail.Name;
            }

            _nextText = @L["Save"];
            _nextIcon = null;
        }
    }

    private async Task OnBack()
    {
        await OnSelectedTabChanged(ScheduleDialogTab.Job);
    }

    private async Task OnSubmit()
    {
        if (SelectedTab == ScheduleDialogTab.Job)
        {
            await OnSelectedTabChanged(ScheduleDialogTab.Trigger);
            return;
        }

        await _triggerPanel.Validate();
        if (!_jobDetailIsValid || !_triggerDetailIsValid)
        {
            return;
        }

        if (IsNew)
        {
            // create schedule
            try
            {
                CreateScheduleArgs createScheduleArgs = new()
                {
                    JobDetailModel = JobDetail,
                    TriggerDetailModel = TriggerDetail
                };  
                await SchedulerSvc.CreateSchedule(createScheduleArgs);
            }
            catch (Exception ex)
            {
                await Notify.Error(L["Error:FailedCreateSchedule"], ex.Message);
                Logger.LogError(ex, "Failed to create new schedule.");
                // TODO show schedule dialog again?
            }
        }
        else
        {
            try
            {
                await SchedulerSvc.UpdateSchedule(JobKey, TriggerKey, JobDetail, TriggerDetail);
            }
            catch (Exception ex)
            {
                await Notify.Error(string.Format(L["Error:FailedUpdateSchedule"],ex.Message));
                Logger.LogError(ex, "Failed to update schedule.");
                // TODO display the dialog again?
            }
        }

        await modalRef.Hide();
    }

    public async Task OpenModalAsync(JobDetailModel jobDetail, TriggerDetailModel triggerDetail, bool isNew = false, ScheduleDialogTab selectedTab = ScheduleDialogTab.Job, bool isReadOnlyJobDetail = false, Key jobKey=null,Key triggerKey=null)
    {
        JobDetail = jobDetail;
        TriggerDetail = triggerDetail;
        SelectedTab = selectedTab;
        IsReadOnlyJobDetail = isReadOnlyJobDetail;
        IsNew = isNew;
        await modalRef.Show();
    }

    protected async Task OnCancel()
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