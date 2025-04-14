using System;
using System.Collections.Generic;

namespace Syrna.QuartzAdmin
{
    public class JobExecutionStatusSummaryModel
    {
        public DateTime StartDateTimeUtc { get; set; }
        public List<KeyValue<JobExecutionStatus, int>> Data { get; set; } = new();
    }
}

