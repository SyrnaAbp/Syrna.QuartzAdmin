using Blazorise;
using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;

namespace Syrna.QuartzAdmin.Blazor.BlazoriseUI;

/// <summary>
/// A surface for grouping other components.
/// </summary>
public partial class Paper : BaseComponent
{
    protected override void BuildStyles(StyleBuilder builder)
    {
        //builder.Append(this.MaxHeight.Style(this.StyleProvider));
        base.BuildStyles(builder);
    }

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append("paper");
        builder.Append("paper-outlined",Outlined);
        builder.Append("paper-square", Square);
        builder.Append($"elevation-{Elevation}", !Outlined);
        base.BuildClasses(builder);
    }

    /// <summary>
    /// The size of the drop shadow.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>1</c>.  A higher number creates a heavier drop shadow.  Use a value of <c>0</c> for no shadow.
    /// </remarks>
    [Parameter]
    public int Elevation { set; get; } = 1;

    /// <summary>
    /// Displays a square shape.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// When <c>true</c>, the <c>border-radius</c> is set to <c>0</c>.
    /// </remarks>
    [Parameter]
    public bool Square { get; set; }

    /// <summary>
    /// Displays an outline around this component.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    public bool Outlined { get; set; }

    ///// <summary>
    ///// The height of this component.
    ///// </summary>
    ///// <remarks>
    ///// Defaults to <c>null</c>.  Can be a pixel height (<c>150px</c>), percentage (<c>30%</c>), or other CSS height value.
    ///// </remarks>
    //[Parameter]
    //public string? Height { get; set; }

    ///// <summary>
    ///// The width of this component.
    ///// </summary>
    ///// <remarks>
    ///// Defaults to <c>null</c>.  Can be a pixel width (<c>150px</c>), percentage (<c>30%</c>), or other CSS width value.
    ///// </remarks>
    //[Parameter]
    //public string? Width { get; set; }

    /// <summary>
    /// The maximum height of this component.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  Can be a pixel height (<c>150px</c>), percentage (<c>30%</c>), or other CSS height value.
    /// </remarks>
    [Parameter]
    public string MaxHeight { get; set; }

    /// <summary>
    /// The maximum width of this component.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  Can be a pixel width (<c>150px</c>), percentage (<c>30%</c>), or other CSS width value.
    /// </remarks>
    [Parameter]
    public string MaxWidth { get; set; }

    /// <summary>
    /// The minimum height of this component.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  Can be a pixel height (<c>150px</c>), percentage (<c>30%</c>), or other CSS height value.
    /// </remarks>
    [Parameter]
    public string MinHeight { get; set; }

    /// <summary>
    /// The minimum width of this component.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  Can be a pixel width (<c>150px</c>), percentage (<c>30%</c>), or other CSS width value.
    /// </remarks>
    [Parameter]
    public string MinWidth { get; set; }

    /// <summary>
    /// The content within this component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }
}