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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Trivial.UI;

/// <summary>
/// The browser window.
/// </summary>
public sealed partial class TabbedWebViewWindow : Window
{
    private readonly SystemBackdropClient backdrop;

    /// <summary>
    /// Initializes a new instance of the TabbedWebViewWindow class.
    /// </summary>
    public TabbedWebViewWindow()
    {
        InitializeComponent();
        try
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomDragRegion);
        }
        catch (ArgumentException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        var theme = HostElement.RequestedTheme;
        backdrop = new();
        backdrop.UpdateWindowBackground(this, theme);
    }

    /// <summary>
    /// Occurs on the tab is closed.
    /// </summary>
    public event TypedEventHandler<TabbedWebViewWindow, TabViewTabCloseRequestedEventArgs> TabCloseRequested;

    /// <summary>
    /// Occurs on the tab drags starting.
    /// </summary>
    public event TypedEventHandler<TabbedWebViewWindow, TabViewTabDragStartingEventArgs> TabDragStarting;

    /// <summary>
    /// Occurs on the tab drags completed.
    /// </summary>
    public event TypedEventHandler<TabbedWebViewWindow, TabViewTabDragCompletedEventArgs> TabDragCompleted;

    /// <summary>
    /// Occurs on the tab is dropped outside.
    /// </summary>
    public event TypedEventHandler<TabbedWebViewWindow, TabViewTabDroppedOutsideEventArgs> TabDroppedOutside;

    /// <summary>
    /// Occurs on the web view tab has created.
    /// </summary>
    public event EventHandler<TabbedWebView.WebViewTabEventArgs> WebViewTabCreated;

    /// <summary>
    /// Occurs on the selection has changed.
    /// </summary>
    public event SelectionChangedEventHandler SelectionChanged;

    /// <summary>
    /// Gets the download list.
    /// </summary>
    public List<CoreWebView2DownloadOperation> DownloadList
    {
        get => HostElement.DownloadList;
        internal set => HostElement.DownloadList = value;
    }

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public Uri Source
    {
        get => HostElement.Source;
        set => HostElement.Source = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the navigation input box is read-only..
    /// </summary>
    public bool IsReadOnly
    {
        get => HostElement.IsReadOnly;
        set => HostElement.IsReadOnly = value;
    }

    /// <summary>
    /// Gets or sets the tab width mode.
    /// </summary>
    public TabViewWidthMode TabWidthMode
    {
        get => HostElement.TabWidthMode;
        set => HostElement.TabWidthMode = value;
    }

    /// <summary>
    /// Gets the tabbed web view element.
    /// </summary>
    public TabbedWebView TabView => HostElement;

    /// <summary>
    /// Gets all web view instances.
    /// </summary>
    public IReadOnlyList<SingleWebView> WebViews => HostElement.WebViews;

    /// <summary>
    /// Gets all tab view items with web view.
    /// </summary>
    public IReadOnlyList<TabViewItem> ItemsWithWebView => HostElement.ItemsWithWebView;

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
    public Func<Uri> DefaultUriCreator
    {
        get => HostElement.DefaultUriCreator;
        set => HostElement.DefaultUriCreator = value;
    }

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
        => HostElement.GetTabItem(webview);

    /// <summary>
    /// Adds a new tab.
    /// </summary>
    /// <param name="tab">The tab view item to add.</param>
    public void Add(TabViewItem tab)
        => HostElement.Add(tab);

    /// <summary>
    /// Adds a new web view.
    /// </summary>
    /// <param name="source">The source URI.</param>
    /// <param name="callback">The callback.</param>
    /// <returns>The web view instance.</returns>
    public SingleWebView Add(Uri source, Action<TabViewItem> callback = null)
        => HostElement.Add(source, callback);

    /// <summary>
    /// Adds a new web view.
    /// </summary>
    /// <param name="tab">The tab view item.</param>
    /// <param name="source">The source URI.</param>
    /// <param name="callback">The callback.</param>
    /// <returns>The web view instance.</returns>
    public SingleWebView NavigateTo(TabViewItem tab, Uri source, Action<TabViewItem> callback = null)
        => HostElement.NavigateTo(tab, source, callback);

    /// <summary>
    /// Inserts a new tab.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="tab">The tab view item to insert.</param>
    public void Insert(int index, TabViewItem tab)
        => HostElement.Insert(index, tab);

    /// <summary>
    /// Removes a specific tab.
    /// </summary>
    /// <param name="item">The tab view item to remove.</param>
    /// <returns>true if remove succeeded; otherwise, false.</returns>
    public bool Remove(object item)
        => HostElement.Remove(item);

    /// <summary>
    /// Removes a specific tab.
    /// </summary>
    /// <param name="index">The index to remove.</param>
    /// <returns>true if remove succeeded; otherwise, false.</returns>
    public void RemoveAt(int index)
        => HostElement.RemoveAt(index);

    /// <summary>
    /// Clears all tabs.
    /// </summary>
    public void Clear()
        => HostElement.Clear();

    /// <summary>
    /// Determines whether the tabs contains a specific value.
    /// </summary>
    /// <param name="tab">The tab view item to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(TabViewItem tab)
        => HostElement.Contains(tab);

    /// <summary>
    /// Determines whether the tabs contains a specific value.
    /// </summary>
    /// <param name="webview">The web view to test.</param>
    /// <returns>true if contains; otherwise, false.</returns>
    public bool Contains(SingleWebView webview)
        => HostElement.Contains(webview);

    private void OnClosed(object sender, WindowEventArgs args)
    {
        if (backdrop != null) backdrop.Dispose();
        try
        {
            var col = HostElement.WebViews.ToList();
            foreach (var wv in col)
            {
                wv.Close();
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
    }

    private void OnTabCloseRequested(TabbedWebView sender, TabViewTabCloseRequestedEventArgs args)
    {
        TabCloseRequested?.Invoke(this, args);
        if (HostElement.WebViews.Count < 1) Close();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (backdrop == null) return;
        var isActive = args.WindowActivationState != WindowActivationState.Deactivated;
        backdrop.IsInputActive = isActive;
    }

    private void OnWebViewTabCreated(object sender, TabbedWebView.WebViewTabEventArgs e)
        => WebViewTabCreated?.Invoke(this, e);

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        => SelectionChanged?.Invoke(this, e);

    private void HostElement_TabDragStarting(TabbedWebView sender, TabViewTabDragStartingEventArgs args)
        => TabDragStarting?.Invoke(this, args);

    private void HostElement_TabDragCompleted(TabbedWebView sender, TabViewTabDragCompletedEventArgs args)
        => TabDragCompleted?.Invoke(this, args);

    private void HostElement_TabDroppedOutside(TabbedWebView sender, TabViewTabDroppedOutsideEventArgs args)
        => TabDroppedOutside?.Invoke(this, args);

    private void HostElement_ContainsFullScreenElementChanged(object sender, Data.DataEventArgs<int> e)
        => VisualUtility.SetFullScreenMode(e.Data > 0, this);
}
