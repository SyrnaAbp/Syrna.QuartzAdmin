using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using Syrna.QuartzAdmin.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Components
{
    public partial class HttpJobUI : IJobUI
    {
        [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
        const string JOB_CLASS = "Syrna.QuartzAdmin.MainDemo.Jobs.HttpJob";

        public string JobClass => JOB_CLASS;

        [Parameter] public bool IsReadOnly { get; set; }
        [Parameter] public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private DataMapValue DataMapUrl = new(DataMapValueType.InterpolatedString, 1);
        private DataMapValue DataMapHeaders = new(DataMapValueType.InterpolatedString, 1);
        private DataMapValue DataMapParameters = new DataMapValue(DataMapValueType.InterpolatedString, 1);
        private string HttpAction { get; set; }
        private bool IgnoreSsl { get; set; }
        private int? TimeoutInSec { get; set; }

        public HttpJobUI()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }

        protected override void OnInitialized()
        {
            if (JobDataMap.ContainsKey(HttpJobKeys.PropertyRequestAction))
            {
                HttpAction = Convert.ToString(JobDataMap[HttpJobKeys.PropertyRequestAction], CultureInfo.InvariantCulture);
            }
            if (JobDataMap.ContainsKey(HttpJobKeys.PropertyRequestUrl))
            {
                DataMapUrl = DataMapValue.Create(JobDataMap[HttpJobKeys.PropertyRequestUrl],
                    DataMapValueType.InterpolatedString, 1);
            }
            if (JobDataMap.ContainsKey(HttpJobKeys.PropertyRequestHeaders))
            {
                DataMapHeaders = DataMapValue.Create(JobDataMap[HttpJobKeys.PropertyRequestHeaders],
                    DataMapValueType.InterpolatedString, 1);
            }
            if (JobDataMap.ContainsKey(HttpJobKeys.PropertyRequestParameters))
            {
                DataMapParameters = DataMapValue.Create(JobDataMap[HttpJobKeys.PropertyRequestParameters],
                    DataMapValueType.InterpolatedString, 1);
            }
            if (JobDataMap.ContainsKey(HttpJobKeys.PropertyIgnoreVerifySsl))
            {
                IgnoreSsl = Convert.ToBoolean(JobDataMap[HttpJobKeys.PropertyIgnoreVerifySsl]);
            }
            if (JobDataMap.ContainsKey(HttpJobKeys.PropertyRequestTimeoutInSec))
            {
                TimeoutInSec = Convert.ToInt32(JobDataMap[HttpJobKeys.PropertyRequestTimeoutInSec]);
            }

        }

        public Task<bool> ApplyChanges()
        {
            if (HttpAction == null)
            {
                JobDataMap.Remove(HttpJobKeys.PropertyRequestAction);
            }
            else
            {
                JobDataMap[HttpJobKeys.PropertyRequestAction] = HttpAction;
            }

            if (DataMapUrl.Value == null)
            {
                JobDataMap.Remove(HttpJobKeys.PropertyRequestUrl);
            }
            else
            {
                JobDataMap[HttpJobKeys.PropertyRequestUrl] = DataMapUrl.ToString();
            }

            if (DataMapHeaders.Value == null)
            {
                JobDataMap.Remove(HttpJobKeys.PropertyRequestHeaders);
            }
            else
            {
                JobDataMap[HttpJobKeys.PropertyRequestHeaders] = DataMapHeaders.ToString();
            }

            if (DataMapParameters.Value == null)
            {
                JobDataMap.Remove(HttpJobKeys.PropertyRequestParameters);
            }
            else
            {
                JobDataMap[HttpJobKeys.PropertyRequestParameters] = DataMapParameters.ToString();
            }

            if (!IgnoreSsl)
            {
                JobDataMap.Remove(HttpJobKeys.PropertyIgnoreVerifySsl);
            }
            else
            {
                JobDataMap[HttpJobKeys.PropertyIgnoreVerifySsl] = IgnoreSsl.ToString();
            }

            if (!TimeoutInSec.HasValue)
            {
                JobDataMap.Remove(HttpJobKeys.PropertyRequestTimeoutInSec);
            }
            else
            {
                JobDataMap[HttpJobKeys.PropertyRequestTimeoutInSec] = TimeoutInSec.Value.ToString();
            }

            return Task.FromResult(true);
        }

        public Task ClearChanges()
        {
            JobDataMap.Remove(HttpJobKeys.PropertyRequestAction);
            JobDataMap.Remove(HttpJobKeys.PropertyRequestUrl);
            JobDataMap.Remove(HttpJobKeys.PropertyRequestHeaders);
            JobDataMap.Remove(HttpJobKeys.PropertyRequestParameters);
            JobDataMap.Remove(HttpJobKeys.PropertyIgnoreVerifySsl);

            return Task.CompletedTask;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //modalRef?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

