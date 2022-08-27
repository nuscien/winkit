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
        public TabViewItem Tab { get; }

        /// <summary>
        /// Gets or sets the web view instance.
        /// </summary>
        public SingleWebView WebView { get; }

        /// <summary>
        /// Gets or sets the initializied source URI to navigate.
        /// </summary>
        public Uri Source { get; }
    }

    /// <summary>
    /// The event arguments of local web app tab.
    /// </summary>
    public class LocalWebAppTabEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the LocalWebAppTabEventArgs class.
        /// </summary>
        /// <param name="tab">The tab view item instance.</param>
        /// <param name="page">The single web view instance.</param>
        public LocalWebAppTabEventArgs(TabViewItem tab, LocalWebAppPage page)
        {
            Tab = tab;
            Page = page;
        }

        /// <summary>
        /// Gets or sets the tab view item instance.
        /// </summary>
        public TabViewItem Tab { get; }

        /// <summary>
        /// Gets or sets the web view instance.
        /// </summary>
        public LocalWebAppPage Page { get; }
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
    /// Initialized a new instance of the TabbedWebView class.
    /// </summary>
    public TabbedWebView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Occurs when fullscreen request sent including to enable and disable.
    /// </summary>
    public event DataEventHandler<int> ContainsFullScreenElementChanged;

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
    /// Occurs on the local web app tab has created.
    /// </summary>
    public event EventHandler<LocalWebAppTabEventArgs> LocalWebAppTabCreated;

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
    public IList<object> TabItems => HostElement.TabItems;

    /// <summary>
    /// Gets all web view instances.
    /// </summary>
    public IReadOnlyList<SingleWebView> WebViews => HostElement.TabItems?.OfType<TabViewItem>()?.Select(ele => ele?.Content as SingleWebView)?.Where(ele => ele != null)?.ToList() ?? new List<SingleWebView>();

    /// <summary>
    /// Gets all local web app page instances.
    /// </summary>
    public IReadOnlyList<LocalWebAppPage> LocalWebApps => HostElement.TabItems?.OfType<TabViewItem>()?.Select(ele => ele?.Content as LocalWebAppPage)?.Where(ele => ele != null)?.ToList() ?? new List<LocalWebAppPage>();

    /// <summary>
    /// Gets all tab view items with web view.
    /// </summary>
    public IReadOnlyList<TabViewItem> ItemsWithWebView => HostElement.TabItems?.OfType<TabViewItem>()?.Where(ele => ele?.Content is SingleWebView)?.ToList() ?? new List<TabViewItem>();

    /// <summary>
    /// Gets all tab view items with local web app page.
    /// </summary>
    public IReadOnlyList<TabViewItem> ItemsWithLocalWebApp => HostElement.TabItems?.OfType<TabViewItem>()?.Where(ele => ele?.Content is LocalWebAppPage)?.ToList() ?? new List<TabViewItem>();

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
    /// Gets the selected local web app page.
    /// </summary>
    public LocalWebAppPage SelectedLocalWebApp => HostElement.SelectedItem as LocalWebAppPage;

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
    /// Gets or sets the window controller.
    /// </summary>
    public Web.IBasicWindowStateController WindowController { get; set; }

    /// <summary>
    /// Gets the web view in first tab.
    /// </summary>
    /// <returns>The web view instance.</returns>
    public SingleWebView GetFirstWebView()
        => WebViews.FirstOrDefault() ?? Add(null as Uri);

    /// <summary>
    /// Gets the web view in first tab.
    /// </summary>
    /// <returns>The web view instance.</returns>
    public SingleWebView GetLastWebView()
        => WebViews.LastOrDefault() ?? Add(null as Uri);

    /// <summary>
    /// Gets a specific local web app page.
    /// </summary>
    /// <param name="resourcePackageIdentifier">The identifier of the resource package.</param>
    /// <returns>The local web app page; or null, if does not exist.</returns>
    public LocalWebAppPage GetLocalWebAppPage(string resourcePackageIdentifier)
        => string.IsNullOrWhiteSpace(resourcePackageIdentifier) ? null : LocalWebApps.FirstOrDefault(ele => ele?.ResourcePackageId == resourcePackageIdentifier);

    /// <summary>
    /// Gets the tab view item.
    /// </summary>
    /// <param name="webview">The web view.</param>
    /// <returns>The tab view item which contains the web view; or null, if non-exists.</returns>
    public TabViewItem GetTabItem(SingleWebView webview)
        => webview == null ? null : HostElement.TabItems?.OfType<TabViewItem>()?.FirstOrDefault(ele => ele?.Content is SingleWebView v && v == webview);

    /// <summary>
    /// Gets the tab view item.
    /// </summary>
    /// <param name="webview">The local web app page.</param>
    /// <returns>The tab view item which contains the web view; or null, if non-exists.</returns>
    public TabViewItem GetTabItem(LocalWebAppPage webview)
        => webview == null ? null : HostElement.TabItems?.OfType<TabViewItem>()?.FirstOrDefault(ele => ele?.Content is LocalWebAppPage v && v == webview);

    /// <summary>
    /// Gets the tab view item.
    /// </summary>
    /// <param name="host">The local web app host.</param>
    /// <returns>The tab view item which contains the web view; or null, if non-exists.</returns>
    public TabViewItem GetTabItem(Web.LocalWebAppHost host)
        => host == null ? null : HostElement.TabItems?.OfType<TabViewItem>()?.FirstOrDefault(ele => ele?.Content is LocalWebAppPage v && v.IsHost(host));

    /// <summary>
    /// Adds a new tab.
    /// </summary>
    /// <param name="tab">The tab view item to add.</param>
    public void Add(TabViewItem tab)
    {
        if (tab != null) HostElement.TabItems.Add(tab);
    }

    /// <summary>
    /// Adds a new tab.
    /// </summary>
    /// <param name="title">The tab header.</param>
    /// <param name="content">The content of the tab.</param>
    /// <param name="icon">An optional icon.</param>
    /// <returns>The tab view item instance.</returns>
    public TabViewItem Add(string title, UIElement content, IconSource icon = null)
    {
        var tab = new TabViewItem
        {
            Header = title,
            Content = content,
            IconSource = icon,
            IsSelected = true
        };
        HostElement.TabItems.Add(tab);
        return tab;
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
    /// Adds a new tab.
    /// </summary>
    /// <param name="host">The host.</param>
    /// <param name="callback">The callback.</param>
    public async Task<LocalWebAppPage> AddAsync(Web.LocalWebAppHost host, Action<TabViewItem, LocalWebAppPage> callback = null)
    {
        if (host == null) return null;
        foreach (var tabItem in HostElement.TabItems)
        {
            if (tabItem is not TabViewItem tabView || tabView.Content is not LocalWebAppPage page || !page.IsHost(host)) continue;
            tabView.IsSelected = true;
            try
            {
                page.FocusBrowser(FocusState.Programmatic);
            }
            catch (InvalidOperationException)
            {
            }
            catch (ExternalException)
            {
            }

            return page;
        }

        var c = AddLocalWebAppTab(callback);
        await c.LoadAsync(host);
        try
        {
            c.FocusBrowser(FocusState.Programmatic);
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        return c;
    }

    /// <summary>
    /// Adds a new tab.
    /// </summary>
    /// <param name="hostTask">The host.</param>
    /// <param name="callback">The callback.</param>
    public Task<LocalWebAppPage> AddAsync(Task<Web.LocalWebAppHost> hostTask, Action<TabViewItem, LocalWebAppPage> callback = null)
        => AddAsync(hostTask, false, callback);

    /// <summary>
    /// Adds a new tab.
    /// </summary>
    /// <param name="hostTask">The host.</param>
    /// <param name="showInfo">true if show the information before loading; otherwise, false.</param>
    /// <param name="callback">The callback.</param>
    public async Task<LocalWebAppPage> AddAsync(Task<Web.LocalWebAppHost> hostTask, bool showInfo, Action<TabViewItem, LocalWebAppPage> callback = null)
    {
        if (hostTask == null) return null;
        var c = AddLocalWebAppTab(callback);
        await c.LoadAsync(hostTask, showInfo);
        return c;
    }

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
        try
        {
            c.Focus(FocusState.Programmatic);
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

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
            if (item is TabViewItem tab)
            {
                var webview = GetWebView(tab);
                if (webview != null)
                {
                    webview.Close();
                }
                else
                {
                    GetLocalWebApp(tab)?.Close();
                }
            }
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
                var webview = GetWebView(tab);
                if (webview != null)
                {
                    webview.Close();
                    continue;
                }

                GetLocalWebApp(tab)?.Close();
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

    /// <summary>
    /// Determines whether the tabs contains a specific value.
    /// </summary>
    /// <param name="webview">The web view to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(LocalWebAppPage webview)
        => webview != null && HostElement.TabItems.Any(ele => ele is TabViewItem tab && tab.Content is LocalWebAppPage v && v == webview);

    private void OnNewWindowRequested(SingleWebView sender, CoreWebView2NewWindowRequestedEventArgs e)
        => _ = OnNewWindowRequestedAsync(e);

    private void OnNewWindowRequested2(LocalWebAppPage sender, CoreWebView2NewWindowRequestedEventArgs e)
        => _ = OnNewWindowRequestedAsync(e);

    private LocalWebAppPage AddLocalWebAppTab(Action<TabViewItem, LocalWebAppPage> callback = null)
    {
        var c = new LocalWebAppPage
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            DownloadList = DownloadList,
            DisableNewWindowRequestHandling = true,
            WindowController = WindowController
        };
        var name = "Loading…";
        var tab = new TabViewItem
        {
            Header = name,
            Content = c,
            IsSelected = true,
        };
        callback?.Invoke(tab, c);
        HostElement.TabItems.Add(tab);
        c.TitleChanged += (sender, e) =>
        {
            tab.Header = e.Data ?? string.Empty;
        };
        c.NewWindowRequested += OnNewWindowRequested2;
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
        LocalWebAppTabCreated?.Invoke(this, new LocalWebAppTabEventArgs(tab, c));
        return c;
    }

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
            var tab = args.Tab;
            var webview = GetWebView(tab);
            if (webview != null)
            {
                webview.Close();
            }
            else
            {
                GetLocalWebApp(tab)?.Close();
            }
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

        foreach (var item in LocalWebApps)
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

    /// <summary>
    /// Gets the local web app instance.
    /// </summary>
    /// <param name="tab">The tab view item.</param>
    /// <returns>The web view contained by the tab view item; or null, if non-exists.</returns>
    private static LocalWebAppPage GetLocalWebApp(TabViewItem tab)
        => tab?.Content as LocalWebAppPage;

    private void HostElement_TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
        => TabDragStarting?.Invoke(this, args);

    private void HostElement_TabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
        => TabDragCompleted?.Invoke(this, args);

    private void HostElement_TabDroppedOutside(TabView sender, TabViewTabDroppedOutsideEventArgs args)
        => TabDroppedOutside?.Invoke(this, args);
}
