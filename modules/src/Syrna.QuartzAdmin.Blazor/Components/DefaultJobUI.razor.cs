using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Components.Messages;

namespace Syrna.QuartzAdmin.Blazor.Components
{
    public partial class DefaultJobUI
    {
        [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
        [Inject] protected IUiMessageService UiMessageService { get; set; } = default!;

        [Parameter]
        [EditorRequired]
        public JobDetailModel JobDetail { get; set; } = new();

        [Parameter] public bool IsReadOnly { get; set; }

        public async Task AddDataMap(DataMapItemModel dataMap)
        {
            if (dataMap is { Key: not null, Value: not null })
            {
                JobDetail.JobDataMap.Add(dataMap.Key, dataMap.Value);
            }
            else
            {
                // TODO print error message. Data map is null
            }
            await InvokeAsync(StateHasChanged);
            await Task.CompletedTask;
        }
        JobDataMapDialog JobDataMapDialogRef;
        private async Task OnAddDataMap()
        {
            var dataMapItem = new DataMapItemModel();
            await JobDataMapDialogRef.OpenModalAsync(new Dictionary<string, object>(JobDetail.JobDataMap, StringComparer.OrdinalIgnoreCase), dataMapItem, AddDataMap);
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

        public DefaultJobUI()
        {
            LocalizationResource = typeof(QuartzAdminResource);
        }
        public async Task EditDataMap(DataMapItemModel dataMap)
        {
            if (dataMap is { Key: not null, Value: not null })
            {
                JobDetail.JobDataMap[dataMap.Key] = dataMap.Value;
            }
            else
            {
                // TODO print error message. Data map is null
            }
            await InvokeAsync(StateHasChanged);
            await Task.CompletedTask;
        }

        private async Task OnEditDataMap(KeyValuePair<string, object> item)
        {
            var dataMapItem = new DataMapItemModel(item);
            await JobDataMapDialogRef.OpenModalAsync(JobDetail.JobDataMap, dataMapItem, EditDataMap, true);
        }

        private async Task OnCloneDataMap(KeyValuePair<string, object> item)
        {
            var index = 1;
            var key = item.Key + index++;

            while (JobDetail.JobDataMap.ContainsKey(key))
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
            await JobDataMapDialogRef.OpenModalAsync(new Dictionary<string, object>(JobDetail.JobDataMap, StringComparer.OrdinalIgnoreCase), dataMapItem, EditDataMap);
        }

        private string DeleteConfirnationMessage(KeyValuePair<string, object> item) => string.Format(L["DeleteConfirmationMessage"], item.Key);

        private async Task OnDeleteDataMap(KeyValuePair<string, object> item)
        {
            bool? yes = await UiMessageService.Confirm(DeleteConfirnationMessage(item));

            if (yes == null || !yes.Value)
            {
                return;
            }

            JobDetail.JobDataMap.Remove(item);
            await InvokeAsync(StateHasChanged);
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

