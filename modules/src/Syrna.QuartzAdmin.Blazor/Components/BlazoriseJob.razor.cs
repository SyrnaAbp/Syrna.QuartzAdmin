using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Blazor.Services;
using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using Syrna.QuartzAdmin.Localization;
using Syrna.QuartzAdmin.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Components
{
    public partial class BlazoriseJob
    {
        [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
        [Inject] private ISchedulerDefinitionService SchedulerDefSvc { get; set; } = null!;
        [Inject] private ISchedulerAppService SchedulerSvc { get; set; } = null!;
        [Inject] private IJobUIProvider JobUIProvider { get; set; } = null!;

        [Parameter]
        [EditorRequired]
        public JobDetailModel JobDetail { get; set; } = new();
        [Parameter] public bool IsReadOnly { get; set; }

        [Parameter] public bool IsValid { get; set; }

        [Parameter] public EventCallback<bool> IsValidChanged { get; set; }

        private Key OriginalJobKey = new(string.Empty, "No Group");

        private IEnumerable<Type> AvailableJobTypes = Enumerable.Empty<Type>();
        private IEnumerable<SelectListItem> ExistingJobGroups;
        private Validations _validations = null!;
        private Type JobUIType = null;
        private Dictionary<string, object> JobUITypeParameters = new();
        private DynamicComponent _jobUIComponent;

        public BlazoriseJob()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }

        protected override async Task OnInitializedAsync()
        {
            var typeNames = await SchedulerDefSvc.GetJobTypeNames(false);
            var typeList = new HashSet<Type>();
            foreach (var typeName in typeNames)
            {
                var type = TypeInfo.GetType(typeName);
                typeList.Add(type);
            }

            if (JobDetail.JobClass != null)
            {
                typeList.Add(JobDetail.JobClass);
            }
            AvailableJobTypes = typeList;
            await OnJobClassValueChanged(JobDetail.JobClass?.FullName);

            OriginalJobKey = new(JobDetail.Name, JobDetail.Group);
            await GetJobGroups();
        }

        private async Task GetJobGroups()
        {
            if (ExistingJobGroups == null)
            {
                ExistingJobGroups = (await SchedulerSvc.GetJobGroups()).Select(s => new SelectListItem(s, s));
            }
        }

        private void OnSetIsValid(ValidationsStatusChangedEventArgs eventArgs)
        {
            if (eventArgs.Status != ValidationStatus.Success)
                return;
            IsValid = eventArgs.Status == ValidationStatus.Success;
            IsValidChanged.InvokeAsync(IsValid).RunSynchronously();
        }

        public async Task Validate()
        {
            if (_jobUIComponent?.Instance is IJobUI jobUi)
            {
                if (!await jobUi.ApplyChanges())
                {
                    //TODO:
                    //OnSetIsValid();
                    return;
                }
            }

            await _validations.ValidateAll();
        }

        public static void IsObjectSelected(ValidatorEventArgs e)
        {
            e.Status = e.Value != null ? ValidationStatus.Success : ValidationStatus.Error;
        }

        private async Task ValidateJobName(ValidatorEventArgs e, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            e.Status = ValidationStatus.Success;

            var name = Convert.ToString(e.Value);

            if (string.IsNullOrEmpty(name))
            {
                e.Status = ValidationStatus.Error;
                return;
            }
            var detail = await SchedulerSvc.GetJobDetail(name, JobDetail.Group);
            if (detail != null)
            {
                e.Status = ValidationStatus.Error;
                e.ErrorText = @L["Error:JobNameAlreadyInUsed"];
                return;
            }

            // accept if same as original
            //if (OriginalJobKey.Equals(name, JobDetail.Group))
            //    return null;

            //if (IsReadOnly)
            //{
            //    Logger.LogDebug("Skip checking of job name uniqueness if in readonly mode");
            //    return null;
            //}
        }

        private async Task OnJobClassValueChanged(string jobTypeName)
        {
            var jobType = AvailableJobTypes.FirstOrDefault(w => w.FullName == jobTypeName);
            JobDetail.JobClass = jobType;
            if (jobType != null)
            {
                // clear previous changes
                if (_jobUIComponent?.Instance is IJobUI jobUi)
                    await jobUi.ClearChanges();

                var jobUiType = JobUIProvider.GetJobUIType(jobType!.FullName);
                JobUITypeParameters.Clear();
                JobUITypeParameters[nameof(IsReadOnly)] = IsReadOnly;
                if (jobUiType == typeof(DefaultJobUI))
                    JobUITypeParameters[nameof(JobDetail)] = JobDetail;
                else
                    JobUITypeParameters[nameof(JobDetail.JobDataMap)] = JobDetail.JobDataMap;
                JobUIType = jobUiType;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //
            }
            base.Dispose(disposing);
        }
    }
}

