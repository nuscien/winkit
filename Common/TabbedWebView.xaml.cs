using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Trivial.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<TabbedWebView>;

/// <summary>
/// The tabbed web view.
/// </summary>
public sealed partial class TabbedWebView : UserControl
{
    /// <summary>
    /// The event arguments of web view tab.
    /// </summary>
    public class WebViewTabEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the WebViewTabEventArgs class.
        /// </summary>
        /// <param name="tab">The tab view item instance.</param>
        /// <param name="webview">The single web view instance.</param>
        /// <param name="source">The initialized source URI to navigate.</param>
        public WebViewTabEventArgs(TabViewItem tab, SingleWebView webview, Uri source)
        {
            Tab = tab;
            WebView = webview;
            Source = source;
        }

        /// <summary>
        /// Gets or sets the tab view item instance.
        /// </summary>
        public TabViewItem Tab { get; set; }

        /// <summary>
        /// Gets or sets the web view instance.
        /// </summary>
        public SingleWebView WebView { get; set; }

        /// <summary>
        /// Gets or sets the initializied source URI to navigate.
        /// </summary>
        public Uri Source { get; set; }
    }

    /// <summary>
    /// The dependency property of source
    /// </summary>
    public static readonly DependencyProperty SourceProperty = DependencyObjectProxy.RegisterProperty<Uri>(nameof(Source), (c, e, p) => c.NavigateTo(c.ItemsWithWebView.FirstOrDefault(), e.NewValue));

    /// <summary>
    /// The dependency property of the flag indicating whether the navigation input box is read-only.
    /// </summary>
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyObjectProxy.RegisterProperty(nameof(IsReadOnly), UpdateReadOnly, false);

    /// <summary>
    /// The dependency property of the tab width mode.
    /// </summary>
    public static readonly DependencyProperty TabWidthModeProperty = DependencyObjectProxy.RegisterProperty<TabViewWidthMode>(nameof(TabWidthMode));

    /// <summary>
    /// Occurs when fullscreen request sent including to enable and disable.
    /// </summary>
    public event DataEventHandler<int> ContainsFullScreenElementChanged;

    /// <summary>
    /// Initialized a new instance of the TabbedWebView class.
    /// </summary>
    public TabbedWebView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Occurs on the tab is closed.
    /// </summary>
    public event TypedEventHandler<TabbedWebView, TabViewTabCloseRequestedEventArgs> TabCloseRequested;

    /// <summary>
    /// Occurs on the tab drags starting.
    /// </summary>
    public event TypedEventHandler<TabbedWebView, TabViewTabDragStartingEventArgs> TabDragStarting;

    /// <summary>
    /// Occurs on the tab drags completed.
    /// </summary>
    public event TypedEventHandler<TabbedWebView, TabViewTabDragCompletedEventArgs> TabDragCompleted;

    /// <summary>
    /// Occurs on the tab is dropped outside.
    /// </summary>
    public event TypedEventHandler<TabbedWebView, TabViewTabDroppedOutsideEventArgs> TabDroppedOutside;

    /// <summary>
    /// Occurs on the web view tab has created.
    /// </summary>
    public event EventHandler<WebViewTabEventArgs> WebViewTabCreated;

    /// <summary>
    /// Occurs on the selection has changed.
    /// </summary>
    public event SelectionChangedEventHandler SelectionChanged;

    /// <summary>
    /// Gets the download list.
    /// </summary>
    public List<CoreWebView2DownloadOperation> DownloadList { get; internal set; } = new();

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public Uri Source
    {
        get => (Uri)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the navigation input box is read-only..
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets the tab width mode.
    /// </summary>
    public TabViewWidthMode TabWidthMode
    {
        get => (TabViewWidthMode)GetValue(TabWidthModeProperty);
        set => SetValue(TabWidthModeProperty, value);
    }

    /// <summary>
    /// Gets all web view instances.
    /// </summary>
    public IReadOnlyList<SingleWebView> WebViews => HostElement.TabItems?.OfType<TabViewItem>()?.Select(ele => ele?.Content as SingleWebView)?.Where(ele => ele != null)?.ToList() ?? new List<SingleWebView>();

    /// <summary>
    /// Gets all tab view items with web view.
    /// </summary>
    public IReadOnlyList<TabViewItem> ItemsWithWebView => HostElement.TabItems?.OfType<TabViewItem>()?.Where(ele => ele?.Content is SingleWebView)?.ToList() ?? new List<TabViewItem>();

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public object SelectedItem
    {
        get => HostElement.SelectedItem;
        set => HostElement.SelectedItem = value;
    }

    /// <summary>
    /// Gets the selected web view.
    /// </summary>
    public SingleWebView SelectedWebView => HostElement.SelectedItem as SingleWebView;

    /// <summary>
    /// Gets or sets the selected index.
    /// </summary>
    public int SelectedIndex
    {
        get => HostElement.SelectedIndex;
        set => HostElement.SelectedIndex = value;
    }

    /// <summary>
    /// Gets or sets the tab strip header
    /// </summary>
    public object TabStripHeader
    {
        get => HostElement.TabStripHeader;
        set => HostElement.TabStripHeader = value;
    }

    /// <summary>
    /// Gets or sets the tab strip footer
    /// </summary>
    public object TabStripFooter
    {
        get => HostElement.TabStripFooter;
        set => HostElement.TabStripFooter = value;
    }

    /// <summary>
    /// Gets or sets the handler to create default URI.
    /// </summary>
    public Func<Uri> DefaultUriCreator { get; set; }

    /// <summary>
    /// Gets the the web view in first tab.
    /// </summary>
    /// <returns>The web view instance.</returns>
    public SingleWebView GetFirstWebView()
        => WebViews.FirstOrDefault() ?? Add(null as Uri);

    /// <summary>
    /// Gets the the web view in first tab.
    /// </summary>
    /// <returns>The web view instance.</returns>
    public SingleWebView GetLastWebView()
        => WebViews.LastOrDefault() ?? Add(null as Uri);

    /// <summary>
    /// Gets the tab view item.
    /// </summary>
    /// <param name="webview">The web view.</param>
    /// <returns>The tab view item which contains the web view; or null, if non-exists.</returns>
    public TabViewItem GetTabItem(SingleWebView webview)
        => webview == null ? null : HostElement.TabItems?.OfType<TabViewItem>()?.FirstOrDefault(ele => ele?.Content is SingleWebView v && v == webview);

    /// <summary>
    /// Adds a new tab.
    /// </summary>
    /// <param name="tab">The tab view item to add.</param>
    public void Add(TabViewItem tab)
    {
        if (tab != null) HostElement.TabItems.Add(tab);
    }

    /// <summary>
    /// Adds a new web view.
    /// </summary>
    /// <param name="source">The source URI.</param>
    /// <param name="callback">The callback.</param>
    /// <returns>The web view instance.</returns>
    public SingleWebView Add(Uri source, Action<TabViewItem> callback = null)
        => NavigateTo(null, source, callback);

    /// <summary>
    /// Adds a new web view.
    /// </summary>
    /// <param name="tab">The tab view item.</param>
    /// <param name="source">The source URI.</param>
    /// <param name="callback">The callback.</param>
    /// <returns>The web view instance.</returns>
    public SingleWebView NavigateTo(TabViewItem tab, Uri source, Action<TabViewItem> callback = null)
    {
        SingleWebView c;
        var name = source == null ? "Blank" : (source.Host ?? "Loading…");
        if (tab == null)
        {
            if (source == null)
            {
                foreach (var testTab in HostElement.TabItems)
                {
                    if (testTab is not TabViewItem tabItem || tabItem.Content is not SingleWebView webview) continue;
                    if (webview.Source != null) continue;
                    HostElement.SelectedItem = tabItem;
                    return webview;
                }
            }

            c = new SingleWebView
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsReadOnly = IsReadOnly,
                DownloadList = DownloadList
            };
            tab = new TabViewItem
            {
                Header = name,
                Content = c,
                IsSelected = true,
            };
            callback?.Invoke(tab);
            HostElement.TabItems.Add(tab);
            WebViewTabCreated?.Invoke(this, new WebViewTabEventArgs(tab, c, source));
        }
        else
        {
            c = tab.Content as SingleWebView;
            tab.Header = name;
            if (c == null) return null;
            callback?.Invoke(tab);
        }

        c.DocumentTitleChanged += (sender, e) =>
        {
            tab.Header = e.Data ?? string.Empty;
        };
        c.NewWindowRequested += OnNewWindowRequested;
        c.ContainsFullScreenElementChanged += OnContainsFullScreenElementChanged; 
        c.WindowCloseRequested += (sender, e) =>
        {
            try
            {
                c.Close();
            }
            catch (InvalidOperationException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ApplicationException)
            {
            }
            catch (ExternalException)
            {
            }

            HostElement.TabItems.Remove(tab);
        };
        if (source != null) c.Source = source;
        return c;
    }

    /// <summary>
    /// Inserts a new tab.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="tab">The tab view item to insert.</param>
    public void Insert(int index, TabViewItem tab)
    {
        if (tab == null) return;
        HostElement.TabItems.Insert(index, tab);
    }

    /// <summary>
    /// Removes a specific tab.
    /// </summary>
    /// <param name="item">The tab view item to remove.</param>
    /// <returns>true if remove succeeded; otherwise, false.</returns>
    public bool Remove(object item)
        => item is not null && HostElement.TabItems.Remove(item);

    /// <summary>
    /// Removes a specific tab.
    /// </summary>
    /// <param name="index">The index to remove.</param>
    /// <returns>true if remove succeeded; otherwise, false.</returns>
    public void RemoveAt(int index)
    {
        var item = HostElement.TabItems.Skip(index).FirstOrDefault();
        if (item == null) return;
        try
        {
            GetWebView(item as TabViewItem)?.Close();
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ApplicationException)
        {
        }
        catch (ExternalException)
        {
        }

        HostElement.TabItems.Remove(item);
    }

    /// <summary>
    /// Clears all tabs.
    /// </summary>
    public void Clear()
    {
        var col = HostElement.TabItems.OfType<TabViewItem>().ToList();
        foreach (var tab in col)
        {
            try
            {
                GetWebView(tab)?.Close();
            }
            catch (InvalidOperationException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ApplicationException)
            {
            }
            catch (ExternalException)
            {
            }
        }

        HostElement.TabItems.Clear();
    }

    /// <summary>
    /// Determines whether the tabs contains a specific value.
    /// </summary>
    /// <param name="tab">The tab view item to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(TabViewItem tab)
        => tab != null && HostElement.TabItems.Contains(tab);

    /// <summary>
    /// Determines whether the tabs contains a specific value.
    /// </summary>
    /// <param name="webview">The web view to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(SingleWebView webview)
        => webview != null && HostElement.TabItems.Any(ele => ele is TabViewItem tab && tab.Content is SingleWebView v && v == webview);

    private void OnNewWindowRequested(SingleWebView sender, CoreWebView2NewWindowRequestedEventArgs e)
        => _ = OnNewWindowRequestedAsync(e);

    private async Task OnNewWindowRequestedAsync(CoreWebView2NewWindowRequestedEventArgs e)
    {
        if (e.WindowFeatures != null && (e.WindowFeatures.HasPosition || e.WindowFeatures.HasSize)) return;
        e.Handled = true;
        var deferral = e.GetDeferral();
        var n = Add(null as Uri);
        await n.EnsureCoreWebView2Async();
        e.NewWindow = n.CoreWebView2;
        deferral.Complete();
    }

    private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        try
        {
            GetWebView(args.Tab)?.Close();
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ApplicationException)
        {
        }
        catch (ExternalException)
        {
        }

        sender.TabItems.Remove(args.Tab);
        TabCloseRequested?.Invoke(this, args);
    }

    private void OnContainsFullScreenElementChanged(object sender, DataEventArgs<bool> e)
    {
        var i = 0;
        foreach (var item in WebViews)
        {
            try
            {
                if (item.ContainsFullScreenElement) i++;
            }
            catch (InvalidOperationException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ApplicationException)
            {
            }
            catch (ExternalException)
            {
            }
        }

        ContainsFullScreenElementChanged?.Invoke(this, new DataEventArgs<int>(i));
    }

    private void HostElement_AddTabButtonClick(TabView sender, object args)
        => Add(DefaultUriCreator?.Invoke());

    private void HostElement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => SelectionChanged?.Invoke(this, e);

    private static void UpdateReadOnly(TabbedWebView c, ChangeEventArgs<bool> e, DependencyProperty d)
    {
        c.HostElement.IsAddTabButtonVisible = !e.NewValue;
        foreach (var webview in c.WebViews)
        {
            webview.IsReadOnly = e.NewValue;
        }
    }

    /// <summary>
    /// Gets the web view instance.
    /// </summary>
    /// <param name="tab">The tab view item.</param>
    /// <returns>The web view contained by the tab view item; or null, if non-exists.</returns>
    private static SingleWebView GetWebView(TabViewItem tab)
        => tab?.Content as SingleWebView;

    private void HostElement_TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
        => TabDragStarting?.Invoke(this, args);

    private void HostElement_TabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
        => TabDragCompleted?.Invoke(this, args);

    private void HostElement_TabDroppedOutside(TabView sender, TabViewTabDroppedOutsideEventArgs args)
        => TabDroppedOutside?.Invoke(this, args);
}
