namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public class QuartzTriggerAttribute : Attribute
    {
        public QuartzTriggerAttribute()
        {

        }
        public QuartzTriggerAttribute(double days, double hours, double minutes, double seconds, double milliseconds, string identity, string description) : this(days, hours, minutes, seconds, milliseconds, 0, identity, description)
        {
        }

        public QuartzTriggerAttribute(double hours, double minutes, double seconds, string identity, string description) : this(0, hours, minutes, seconds, 0, 0, identity, description)
        {
        }

        public QuartzTriggerAttribute(double minutes, double seconds, string identity, string description) : this(0, 0, minutes, seconds, 0, 0, identity, description)
        {
        }

        /// <summary>
        /// Trigger every x seconds
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="identity"></param>
        /// <param name="description"></param>
        public QuartzTriggerAttribute(double seconds, string identity, string description) : this(0, 0, 0, seconds, 0, 0, identity, description)
        {
        }


        public QuartzTriggerAttribute(double days, double hours, double minutes, double seconds, double milliseconds) : this(days, hours, minutes, seconds, milliseconds, 0, null, null)
        {
        }

        /// <summary>
        /// Trigger every x hours
        /// </summary>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="hours"></param>
        public QuartzTriggerAttribute(double hours, double minutes, double seconds) : this(0, hours, minutes, seconds, 0, 0, null, null)
        {
        }

        public QuartzTriggerAttribute(double minutes, double seconds) : this(0, 0, minutes, seconds, 0, 0, null, null)
        {
        }

        public QuartzTriggerAttribute(double seconds) : this(0, 0, 0, seconds, 0, 0, null, null)
        {
        }

        public QuartzTriggerAttribute(bool manual) : this(0, 0, 0, 0, 0, 0, null, null)
        {
            Manual = manual;
        }


        private QuartzTriggerAttribute(double days, double hours, double minutes, double seconds, double milliseconds, long ticks, string identity, string description)
        {

            WithInterval = TimeSpan.FromTicks(ticks + (long)(days * TimeSpan.TicksPerDay
                                             + hours * TimeSpan.TicksPerHour
                                             + minutes * TimeSpan.TicksPerMinute
                                             + seconds * TimeSpan.TicksPerSecond
                                             + milliseconds + TimeSpan.TicksPerMillisecond));
            Identity = identity;
            Description = description;
        }
        public string Description { get; set; }
        public string Identity { get; set; }
        public TimeSpan WithInterval { get; set; }
        public DateTimeOffset StartAt { get; set; } = DateTimeOffset.MinValue;
        public int RepeatCount { get; set; } = 0;
        public string TriggerName { get; set; } = string.Empty;
        public string TriggerGroup { get; set; } = string.Empty;
        public string TriggerDescription { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
        public bool Manual { get; set; }
    }
}
