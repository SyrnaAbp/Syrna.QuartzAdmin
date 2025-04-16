using System;
using System.Collections.Generic;
using System.Text;

namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public class QuartzTriggerAttribute : Attribute
    {
        public QuartzTriggerAttribute()
        {

        }
        public QuartzTriggerAttribute(double days, double hours, double minutes, double seconds, double milliseconds, string _identity, string _desciption) : this(days, hours, minutes, seconds, milliseconds, 0, _identity, _desciption)
        {
        }

        public QuartzTriggerAttribute(double hours, double minutes, double seconds, string _identity, string _desciption) : this(0, hours, minutes, seconds, 0, 0, _identity, _desciption)
        {
        }

        public QuartzTriggerAttribute(double minutes, double seconds, string _identity, string _desciption) : this(0, 0, minutes, seconds, 0, 0, _identity, _desciption)
        {
        }

        /// <summary>
        /// Trigger every x seconds
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="_identity"></param>
        /// <param name="_desciption"></param>
        public QuartzTriggerAttribute(double seconds, string _identity, string _desciption) : this(0, 0, 0, seconds, 0, 0, _identity, _desciption)
        {
        }


        public QuartzTriggerAttribute(double days, double hours, double minutes, double seconds, double milliseconds) : this(days, hours, minutes, seconds, milliseconds, 0, null, null)
        {
        }

        /// <summary>
        /// Trigger every x hours
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="_identity"></param>
        /// <param name="_desciption"></param>
        public QuartzTriggerAttribute(double hours, double minutes, double seconds) : this(0, hours, minutes, seconds, 0, 0, null, null)
        {
        }

        public QuartzTriggerAttribute(double minutes, double seconds) : this(0, 0, minutes, seconds, 0, 0, null, null)
        {
        }

        public QuartzTriggerAttribute(double seconds) : this(0, 0, 0, seconds, 0, 0, null, null)
        {
        }

        public QuartzTriggerAttribute(bool Manual) : this(0, 0, 0, 0, 0, 0, null, null)
        {
            this.Manual = true;
        }


        public QuartzTriggerAttribute(double days, double hours, double minutes, double seconds, double milliseconds, long ticks, string _identity, string _desciption)
        {

            WithInterval = TimeSpan.FromTicks(ticks + (long)(days * TimeSpan.TicksPerDay
                                             + hours * TimeSpan.TicksPerHour
                                             + minutes * TimeSpan.TicksPerMinute
                                             + seconds * TimeSpan.TicksPerSecond
                                             + milliseconds + TimeSpan.TicksPerMillisecond));
        }
        public string Desciption { get; set; } = null;
        public string Identity { get; set; } = null;
        public TimeSpan WithInterval { get; set; }
        public DateTimeOffset StartAt { get; set; } = DateTimeOffset.MinValue;
        public int RepeatCount { get; set; } = 0;
        public string TriggerName { get; set; } = string.Empty;
        public string TriggerGroup { get; set; } = string.Empty;
        public string TriggerDescription { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
        public bool Manual { get; set; } = false;
    }
}
