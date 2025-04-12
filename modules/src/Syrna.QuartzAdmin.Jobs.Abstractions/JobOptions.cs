using System;
using System.Collections.Generic;
using System.Text;

namespace Syrna.QuartzAdmin.Jobs.Abstractions
{
    public class JobOptions
    {
        public TriggerOptions[] Triggers { get; set; }
    }
}
