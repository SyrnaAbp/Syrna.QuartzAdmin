using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.Components;

public partial class JobDataMapDialog
{
    [Inject]
    protected new IStringLocalizer<QuartzAdminResource> L { get; set; }

    public IDictionary<string, object> JobDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public DataMapItemModel DataMapItem { get; set; } = new();

    public bool IsEditMode { get; set; }

    public Func<DataMapItemModel, Task> Save { get; set; }

    private string Value { get; set; }
    private Validations _validations = null!;
    Modal modalRef;

    public JobDataMapDialog()
    {
        LocalizationResource = typeof(QuartzAdminResource);
    }

    /// <summary>
    /// DataMapType -> Description
    /// </summary>
    private IDictionary<DataMapType, string> AvailableDataMapTypes = new Dictionary<DataMapType, string>();

    protected override void OnInitialized()
    {
        // initialize available data types
        foreach (var mapType in Enum.GetValues<DataMapType>())
        {
            if (mapType == DataMapType.Object)
            {
                continue;
            }
            AvailableDataMapTypes.Add(mapType, mapType.ToString());
        }
        var currentMapType = DataMapItem.OriginalKeyValue?.GetDataMapType();
        if (currentMapType is DataMapType.Object)
        {
            AvailableDataMapTypes.Add(currentMapType.Value,
                DataMapItem.OriginalKeyValue?.GetDataMapTypeDescription() ?? string.Empty);
        }

        Value = DataMapItem.Value?.ToString();
    }

    private void ValidateKey(ValidatorEventArgs e)
    {
        var key = Convert.ToString(e.Value);
        if (string.IsNullOrEmpty(key))
        {
            e.ErrorText = "Key is required";
            e.Status = ValidationStatus.Error;
        }

        // validate then add to dictionary
        if (!DataMapItem.IsSameKeyAsOriginal() && JobDataMap.ContainsKey(key))
        {
            e.ErrorText = "This key was already defined";
            e.Status = ValidationStatus.Error;
        }

        e.Status = ValidationStatus.Success;
    }

    private async Task OnSave()
    {
        var isValid = await _validations.ValidateAll();

        if (!isValid)
            return;

        try
        {
            DataMapItem.SetValue(Value);
            await Save!.Invoke(DataMapItem);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }

        await modalRef.Hide();
    }

    public async Task OpenModalAsync(IDictionary<string, object> jobDataMap, DataMapItemModel dataMapItem, Func<DataMapItemModel, Task> save, bool isEditMode = false)
    {
        DataMapItem = dataMapItem;
        JobDataMap = jobDataMap;
        IsEditMode = isEditMode;
        Save = save;
        await modalRef.Show();
    }

    protected async Task OnCancel()
    {
        await modalRef.Hide();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            modalRef?.Dispose();
        }
        base.Dispose(disposing);
    }
}