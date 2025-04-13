using Blazorise;
using Syrna.QuartzAdmin.Triggers;
using System;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Components
{
    public interface ITriggerDetailModelValidator
    {
        bool ValidateDaysOfWeek(TriggerDetailModel triggerModel);
        Task ValidateTriggerName(ValidatorEventArgs eventArgs, TriggerDetailModel triggerModel, Key triggerKey);
        void ValidateTime(TimeSpan? start, ValidatorEventArgs e);

        void ValidateFirstLastDateTime(TriggerDetailModel model, ValidatorEventArgs e);
        void ValidateCronExpression(ValidatorEventArgs eventArgs);
    }
}