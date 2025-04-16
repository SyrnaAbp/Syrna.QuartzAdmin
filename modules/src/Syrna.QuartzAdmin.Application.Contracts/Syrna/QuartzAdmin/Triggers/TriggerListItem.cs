using Microsoft.VisualBasic;
using Quartz;
using System;

namespace Syrna.QuartzAdmin.Triggers
{
    /// <summary>
    /// Data model for a trigger list item.
    /// </summary>
    public sealed class TriggerListItem
    {
        /// <summary>
        /// The Name of the <see cref="ITrigger"/>
        /// </summary>
        public string TriggerName { get; set; }
        /// <summary>
        /// The Trigger Group where the <see cref="ITrigger"/> is configured with.
        /// </summary>
        public string TriggerGroup { get; set; }
        /// <summary>
        /// True if trigger is paused.
        /// </summary>
        public bool IsPaused { get; set; }
        /// <summary>
        /// The <see cref="TriggerType"/> type of trigger.
        /// </summary>
        public TriggerType Type { get; set; }
        public string ClrType { get; set; }

        /// <summary>
        /// Short description of the <see cref="ITrigger"/>
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The time when the trigger fired first time.
        /// </summary>
        public DateTimeOffset StartTimeUtc { get; set; }
        /// <summary>
        /// The time when a trigger will stop triggering,
        /// </summary>
        public DateTimeOffset? EndTimeUtc { get; set; }
        /// <summary>
        /// The last time the trigger was fired.
        /// </summary>
        public DateTimeOffset? LastFireTimeUtc { get; set; }
        /// <summary>
        /// The next time the trigger fires.
        /// </summary>
        public DateTimeOffset? NextFireTimeUtc { get; set; }
        /// <summary>
        /// Human readable description of the <see cref="ITrigger"/> scheduling settings.
        /// </summary>
        public string ScheduleDescription { get; set; }

        public string TypeString
        {
            get
            {
                if (Type == TriggerType.Unknown)
                {
                    return ClrType;
                }
                else
                {
                    return Type.ToString();
                }
            }
        }
    }
}
