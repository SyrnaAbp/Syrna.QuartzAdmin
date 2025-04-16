using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Localization;
using Syrna.QuartzAdmin.Scheduler;
using Syrna.QuartzAdmin.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Components.Messages;

namespace Syrna.QuartzAdmin.Blazor.Components
{
    public partial class BlazoriseTrigger
    {
        [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
        [Inject] private ISchedulerDefinitionAppService SchedulerDefSvc { get; set; } = null!;
        [Inject] private ISchedulerAppService SchedulerSvc { get; set; } = null!;
        [Inject] private ITriggerDetailModelValidator Validator { get; set; } = null!;
        [Inject] protected IUiMessageService UiMessageService { get; set; } = default!;

        [Parameter]
        [EditorRequired]
        public TriggerDetailModel TriggerDetail { get; set; } = new();

        [Parameter] public bool IsValid { get; set; }

        [Parameter] public EventCallback<bool> IsValidChanged { get; set; }

        private ISet<TriggerType> ExcludedTriggerTypeChoices = new HashSet<TriggerType> { TriggerType.Unknown, TriggerType.Calendar };

        private IEnumerable<SelectListItem> ExistingTriggerGroups;

        private string CronDescription;
        private Validations _validations = null!;
        private bool _isDaysOfWeekValid = true;
        private IReadOnlyCollection<SelectListItem> _calendars;
        private IReadOnlyCollection<TimeZoneInfo> _timeZones;
        private TimePicker<TimeSpan?> _endDailyTimePicker = null!;
        private DatePicker<DateTime?> _endDatePicker = null!;
        private Key OriginalTriggerKey { get; set; }

        private Dictionary<TriggerType, string> TriggerTypeIcons = new()
        {
            { TriggerType.Cron, TriggerType.Cron.GetTriggerTypeIcon() },
            { TriggerType.Daily, TriggerType.Daily.GetTriggerTypeIcon() },
            { TriggerType.Simple, TriggerType.Simple.GetTriggerTypeIcon() },
            { TriggerType.Calendar, TriggerType.Calendar.GetTriggerTypeIcon() },
        };

        public BlazoriseTrigger()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }

        protected override void OnInitialized()
        {
            Task.Run(() => OnCronExpressionInputElapsed(TriggerDetail.CronExpression));
            OriginalTriggerKey = new(TriggerDetail.Name, TriggerDetail.Group);
            Task.Run(GetTimeZones);
        }

        string GetDataMapTypeDescription(KeyValuePair<string, object> kv)
        {
            var mapType = kv.GetDataMapType();
            if (mapType == DataMapType.Object)
            {
                return L[$"DataMapType:{mapType}"] + $" ({kv.Value.GetType().FullName})";
            }
            return L[$"DataMapType:{mapType}"];
        }

        public static void DailyDayOfWeekValidation(ValidatorEventArgs e)
        {
            if (e.Value != null)
            {
                var items = e.Value as bool[];
                if (items != null && items.Length > 0)
                {
                    if (!items.Any())
                    {
                        e.Status = ValidationStatus.Error;
                        return;
                    }
                    e.Status = ValidationStatus.Success;
                    return;
                }
            }
            e.Status = ValidationStatus.None;
        }

        private async Task OnCronExpressionInputElapsed(string cronExpression)
        {
            TriggerDetail.CronExpression = cronExpression;
            try
            {
                var options = new CronExpressionDescriptor.Options()
                {
                    ThrowExceptionOnParseError = false,
                    Verbose = false,
                    DayOfWeekStartIndexZero = true,
                    Locale = CultureInfo.DefaultThreadCurrentCulture?.ToString()
                };
                CronDescription = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cronExpression, options);
            }
            catch
            {
                CronDescription = L["CronHelpText"];
            }

            await Task.CompletedTask;
        }

        private async Task SetCronExpression(string cronExpression)
        {
            await OnCronExpressionInputElapsed(cronExpression);
            await InvokeAsync(StateHasChanged);
            await Task.CompletedTask;
        }
        
        List<IntervalUnit> TriggerIntervalUnits;
        List<MisfireAction> MisfireActions;
        private async Task TriggerTypeChanged(TriggerType triggerType)
        {
            TriggerDetail.TriggerType = triggerType;
            TriggerIntervalUnits = await SchedulerDefSvc.GetTriggerIntervalUnits(triggerType);
            MisfireActions=await SchedulerDefSvc.GetMisfireActions(triggerType);
            await InvokeAsync(StateHasChanged);
        }

        private async Task GetTriggerGroups()
        {
            ExistingTriggerGroups ??= (await SchedulerSvc.GetTriggerGroups()).Select(s => new SelectListItem(s, s));
        }

        private async Task OnShowSampleCron()
        {
            await CronSamplesDialogRef.OpenModalAync(SetCronExpression);
        }

        private async Task GetTimeZones()
        {
            _timeZones ??= await Task.Run(TimeZoneInfo.GetSystemTimeZones);
        }

        private async Task GetCalendars()
        {
            _calendars ??= (await SchedulerSvc.GetCalendarNames()).Select(s => new SelectListItem(s, s)).ToImmutableList();
        }

        private void OnSetValidationStatusChanged(ValidationsStatusChangedEventArgs eventArgs)
        {
            var value = eventArgs.Status == ValidationStatus.Success;
            if (IsValid == value)
                return;
            IsValid = value;
            IsValidChanged.InvokeAsync(value).RunSynchronously();
        }

        private void OnSetIsValid(bool value)
        {
            if (IsValid == value)
                return;
            IsValid = value;
            IsValidChanged.InvokeAsync(value).RunSynchronously();
        }

        public async Task Validate()
        {
            var isValid = await _validations.ValidateAll();

            _isDaysOfWeekValid = Validator.ValidateDaysOfWeek(TriggerDetail);
            if (isValid)
                OnSetIsValid(_isDaysOfWeekValid);

            // if daily trigger does not have end time, assign end time
            if (TriggerDetail is { TriggerType: TriggerType.Daily, EndDailyTime: null })
            {
                TriggerDetail.EndDailyTime = TriggerDetail.StartDailyTime;
            }
        }

        public async Task AddDataMap(DataMapItemModel dataMap)
        {
            if (dataMap is { Key: not null, Value: not null })
                TriggerDetail.TriggerDataMap.Add(dataMap.Key, dataMap.Value);
            else
            {
                // TODO print error message. Data map is null
            }

            await Task.CompletedTask;
        }

        JobDataMapDialog JobDataMapDialogRef;
        private async Task OnAddDataMap()
        {
            var dataMapItem = new DataMapItemModel();
            await JobDataMapDialogRef.OpenModalAsync(new Dictionary<string, object>(TriggerDetail.TriggerDataMap, StringComparer.OrdinalIgnoreCase), dataMapItem, AddDataMap);
        }

        public async Task UpdateDataMap(DataMapItemModel dataMap)
        {
            if (dataMap is { Key: not null, Value: not null })
            {
                TriggerDetail.TriggerDataMap[dataMap.Key] = dataMap.Value;
            }
            else
            {
                // TODO print error message. Data map is null
            }

            await Task.CompletedTask;
        }

        private async Task OnEditDataMap(KeyValuePair<string, object> item)
        {
            var dataMapItem = new DataMapItemModel(item);
            await JobDataMapDialogRef.OpenModalAsync(new Dictionary<string, object>(TriggerDetail.TriggerDataMap, StringComparer.OrdinalIgnoreCase), dataMapItem, UpdateDataMap, true);
        }

        private async Task OnCloneDataMap(KeyValuePair<string, object> item)
        {
            var index = 1;
            var key = item.Key + index++;

            while (TriggerDetail.TriggerDataMap.ContainsKey(key))
            {
                if (index == int.MaxValue)
                {
                    key = string.Empty;
                    break;
                }

                key = item.Key + index++;
            }
            var clonedItem = new KeyValuePair<string, object>(key, item.Value);
            var dataMapItem = new DataMapItemModel(clonedItem);
            await JobDataMapDialogRef.OpenModalAsync(new Dictionary<string, object>(TriggerDetail.TriggerDataMap, StringComparer.OrdinalIgnoreCase), dataMapItem, UpdateDataMap);
        }
        
        private string DeleteConfirnationMessage(KeyValuePair<string, object> item) => string.Format(L["DeleteConfirmationMessage"], item.Key);

        CronSamplesDialog CronSamplesDialogRef;
        private async Task OnDeleteDataMap(KeyValuePair<string, object> item)
        {
            bool? yes = await UiMessageService.Confirm(DeleteConfirnationMessage(item));

            if (yes == null || !yes.Value)
            {
                return;
            }

            TriggerDetail.TriggerDataMap.Remove(item);
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

