using Blazorise;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Localization;
using Syrna.QuartzAdmin.Scheduler;
using Syrna.QuartzAdmin.Triggers;
using System;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Components
{
    public class TriggerDetailModelValidator(ISchedulerAppService schSvc, IStringLocalizer<QuartzAdminResource> L) : ITriggerDetailModelValidator
    {

        /// <summary>
        /// Returns true if Days of week validation is successful
        /// </summary>
        /// <param name="triggerModel"></param>
        /// <returns></returns>
        public bool ValidateDaysOfWeek(TriggerDetailModel triggerModel)
        {
            if (triggerModel.TriggerType != TriggerType.Daily)
            {
                return true;
            }

            foreach (var val in triggerModel.DailyDayOfWeek)
            {
                if (val)
                {
                    return true;
                }
            }

            return false;
        }

        public void ValidateTime(TimeSpan? start, ValidatorEventArgs e)
        {
            var endSpan = (TimeSpan?)e.Value;
            if (start.HasValue && endSpan.HasValue)
            {
                if (start.Value > endSpan.Value)
                {
                    e.Status = ValidationStatus.Error;
                    return;
                }
            }

            e.Status = ValidationStatus.Success;
        }

        public void ValidateFirstLastDateTime(TriggerDetailModel model, ValidatorEventArgs e)
        {
            if (!model.StartDate.HasValue || !model.EndDate.HasValue)
            {
                e.Status = ValidationStatus.None;
                return;
            }
            else
            {
                var start = model.StartDate.Value.Add(model.StartTimeSpan ?? TimeSpan.Zero);
                var end = model.EndDate.Value.Add(model.EndTimeSpan ?? TimeSpan.Zero);

                if (start > end)
                {
                    e.Status = ValidationStatus.Error;
                }
                else
                {
                    e.Status = ValidationStatus.Success;
                }

                return;
            }
        }

        public void ValidateCronExpression(ValidatorEventArgs eventArgs)
        {
            var cronExpression = Convert.ToString(eventArgs.Value);
            if (string.IsNullOrEmpty(cronExpression))
            {
                eventArgs.ErrorText = L["Error:CronExpressionRequired"];
                eventArgs.Status = ValidationStatus.Error;
                return;
            }

            if (!CronExpressionHelper.IsValidExpression(cronExpression))
            {
                eventArgs.ErrorText = L["Error:CheckCronExpression"];
                eventArgs.Status = ValidationStatus.Error;
                return;
            }

            eventArgs.Status = ValidationStatus.Success;
        }

        public async Task ValidateTriggerName(ValidatorEventArgs eventArgs, TriggerDetailModel triggerModel, Key triggerKey)
        {
            if (triggerKey == null)
            {
                return;
            }

            var name = Convert.ToString(eventArgs.Value);
            if (string.IsNullOrEmpty(name))
            {
                eventArgs.ErrorText = L["Error:TriggerNameRequired"];
                eventArgs.Status = ValidationStatus.Error;
                return;
            }

            if (triggerKey.Equals(name, triggerModel.Group))
            {
                eventArgs.Status = ValidationStatus.None;
                return;
            }
            var exists = await schSvc.ContainsTriggerKey(name, triggerModel.Group);

            if (exists)
            {
                eventArgs.ErrorText = L["Error:TriggerNameAlreadyInUsed"];
                eventArgs.Status = ValidationStatus.Error;
                return;
            }

            eventArgs.Status = ValidationStatus.Success;
            await Task.CompletedTask;
        }
    }
}

