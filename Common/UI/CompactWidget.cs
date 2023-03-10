using Microsoft.Windows.Widgets.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.UI;

/// <summary>
/// The base compact widget.
/// </summary>
public abstract class BaseCompactWidget
{
    /// <summary>
    /// Initializes a new instance of the BaseCompactWidget class.
    /// </summary>
    /// <param name="context">The widget context.</param>
    public BaseCompactWidget(WidgetContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Gets the widget context.
    /// </summary>
    public WidgetContext Context { get; }

    /// <summary>
    /// Gets the widget identifier.
    /// </summary>
    public string Id => Context?.Id;

    /// <summary>
    /// Gets the definition identifier of the widget.
    /// </summary>
    public string DefinitionId => Context?.DefinitionId;

    /// <summary>
    /// Gets the custom state.
    /// </summary>
    public string CustomState { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the widget is active.
    /// </summary>
    public bool IsActive => Context.IsActive;

    /// <summary>
    /// Updates view.
    /// </summary>
    /// <param name="request">The widget update request options.</param>
    /// <param name="args">The widget action invoked arguments; or null, if it turns active or its context is changed.</param>
    internal protected abstract void Update(WidgetUpdateRequestOptions request, WidgetActionInvokedArgs args);
}

/// <summary>
/// The base widget provider.
/// </summary>
public abstract class BaseCompactWidgetProvider : IWidgetProvider
{
    /// <summary>
    /// The widget instances.
    /// </summary>
    private readonly Dictionary<string, BaseCompactWidget> widgets = new();

    /// <inheritdoc />
    public virtual void CreateWidget(WidgetContext widgetContext)
    {
        var widget = OnCreateWidget(widgetContext);
        if (widget?.Context is null) return;
        widgets[widgetContext.Id] = widget;
        UpdateWidget(widget);
    }

    /// <inheritdoc />
    public virtual void Activate(WidgetContext widgetContext)
    {
        var widgetId = widgetContext.Id;
        if (widgets.TryGetValue(widgetId, out BaseCompactWidget widget))
        {
            UpdateWidget(widget);
        }
    }

    /// <inheritdoc />
    public virtual void Deactivate(string widgetId)
    {
    }

    /// <inheritdoc />
    public virtual void DeleteWidget(string widgetId, string customState)
        => widgets.Remove(widgetId);

    /// <inheritdoc />
    public virtual void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        => UpdateWidget(actionInvokedArgs?.WidgetContext?.Id);

    /// <inheritdoc />
    public virtual void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
        => UpdateWidget(contextChangedArgs?.WidgetContext?.Id);

    /// <summary>
    /// Gets all identifiers of widget.
    /// </summary>
    /// <returns>The collection of widget identifier.</returns>
    public IEnumerable<string> GetWidgetIds() => widgets.Keys;

    /// <summary>
    /// Tries to get the specific widget.
    /// </summary>
    /// <param name="id">The widget identifier.</param>
    /// <param name="result">The widget instance.</param>
    /// <returns>true if exists; otherwise, false.</returns>
    public bool TryGetWidget(string id, out BaseCompactWidget result)
        => widgets.TryGetValue(id, out result);

    /// <summary>
    /// Tries to get the specific widget.
    /// </summary>
    /// <param name="id">The widget identifier.</param>
    /// <returns>The widget instance; or null, if does not exist.</returns>
    public BaseCompactWidget TryGetWidget(string id)
        => widgets.TryGetValue(id, out var result) ? result : null;

    /// <summary>
    /// Creates a compact widget.
    /// </summary>
    /// <param name="widgetContext">The widget context.</param>
    /// <returns>An instance of the compact widget.</returns>
    protected abstract BaseCompactWidget OnCreateWidget(WidgetContext widgetContext);

    /// <summary>
    /// Updates widget to view.
    /// </summary>
    /// <param name="request">The widget update request options.</param>
    protected virtual void UpdateWidget(WidgetUpdateRequestOptions request)
        => WidgetManager.GetDefault().UpdateWidget(request);

    private void UpdateWidget(string id, WidgetActionInvokedArgs args = null)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (widgets.TryGetValue(id, out BaseCompactWidget widget))
        {
            UpdateWidget(widget, args);
        }
    }

    private void UpdateWidget(BaseCompactWidget widget, WidgetActionInvokedArgs args = null)
    {
        var request = new WidgetUpdateRequestOptions(widget.Id)
        {
            CustomState = widget.CustomState
        };
        widget.Update(request, args);
        widget.CustomState = request.CustomState;
        UpdateWidget(request);
    }
}
