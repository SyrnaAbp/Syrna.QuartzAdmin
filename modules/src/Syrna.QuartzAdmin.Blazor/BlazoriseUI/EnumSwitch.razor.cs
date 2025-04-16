using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Syrna.QuartzAdmin.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Syrna.QuartzAdmin.Blazor.BlazoriseUI;

public partial class EnumSwitch<T>
{
    [Inject] protected new IStringLocalizer<QuartzAdminResource> L { get; set; }
    [Parameter] public Size Size { get; set; } = Size.Medium;

    [Parameter] public ISet<T> ExcludedValues { get; set; } = default!;

    [Parameter] public IDictionary<T, string> ValueIcons { get; set; } = new Dictionary<T, string>();

    [Parameter] public T Value { get; set; } = default!;

    [Parameter] public EventCallback<T> ValueChanged { get; set; }
    private Type Type => typeof(T);

    private string GetIcon(int intEnum)
    {
        return ValueIcons[(T)Enum.ToObject(Type, intEnum)];
    }

    private async Task OnSelected(int value)
    {
        var preValue = Value;
        Value = (T)Enum.ToObject(Type, value);
        if (preValue.Equals(Value))
        {
            return;
        }

        await ValueChanged.InvokeAsync(Value);
    }

    public EnumSwitch()
    {
        LocalizationResource = typeof(QuartzAdminResource);
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