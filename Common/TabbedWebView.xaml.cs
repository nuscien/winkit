﻿using Microsoft.UI.Xaml;
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Trivial.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
    /// Initialized a new instance of the TabbedWebView class.
    /// </summary>
    public TabbedWebView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Occurs on the web view tab has created.
    /// </summary>
    public event EventHandler<WebViewTabEventArgs> WebViewTabCreated;

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
        get => (TabViewWidthMode)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
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
    /// Gets the the web view in first tab.
    /// </summary>
    /// <returns>The web view instance.</returns>
    public SingleWebView GetFirstWebView()
        => WebViews.FirstOrDefault() ?? Add(null);

    /// <summary>
    /// Adds a new web view.
    /// </summary>
    /// <param name="source">The source URI.</param>
    /// <returns>The web view instance.</returns>
    public SingleWebView Add(Uri source)
        => NavigateTo(null, source);

    /// <summary>
    /// Adds a new web view.
    /// </summary>
    /// <param name="tab">The tab view item.</param>
    /// <param name="source">The source URI.</param>
    /// <returns>The web view instance.</returns>
    public SingleWebView NavigateTo(TabViewItem tab, Uri source)
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
                IsReadOnly = IsReadOnly
            };
            tab = new TabViewItem
            {
                Header = name,
                Content = c,
                IsSelected = true,
            };
            HostElement.TabItems.Add(tab);
            WebViewTabCreated?.Invoke(this, new WebViewTabEventArgs(tab, c, source));
        }
        else
        {
            c = tab.Content as SingleWebView;
            tab.Header = name;
            if (c == null) return null;
        }

        c.DocumentTitleChanged += (sender, e) =>
        {
            tab.Header = e.Data ?? string.Empty;
        };
        c.NewWindowRequested += (sender, e) =>
        {
            _ = OnNewWindowRequestedAsync(e);
        };
        if (source != null) c.Source = source;
        return c;
    }

    private async Task OnNewWindowRequestedAsync(CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        var deferral = e.GetDeferral();
        var n = Add(null);
        await n.EnsureCoreWebView2Async();
        e.NewWindow = n.CoreWebView2;
        deferral.Complete();
    }

    private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        GetWebView(args.Tab)?.Close();
        sender.TabItems.Remove(args.Tab);
    }

    private SingleWebView GetWebView(TabViewItem tab)
        => tab?.Content as SingleWebView;

    private void HostElement_AddTabButtonClick(TabView sender, object args)
        => Add(null);

    private static void UpdateReadOnly(TabbedWebView c, ChangeEventArgs<bool> e, DependencyProperty d)
    {
        c.HostElement.IsAddTabButtonVisible = !e.NewValue;
        foreach (var webview in c.WebViews)
        {
            webview.IsReadOnly = e.NewValue;
        }
    }
}