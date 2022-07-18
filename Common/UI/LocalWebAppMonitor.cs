using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.UI;

/// <summary>
/// The monitor of the local web app page.
/// </summary>
public interface ILocalWebAppPageMonitor
{
    /// <summary>
    /// Occurs on creating.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="webview">The WebView2 instance.</param>
    public void OnCreate(LocalWebAppPage page, WebView2 webview);

    /// <summary>
    /// Occurs on navigated to.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="e">The navigation event arguments.</param>
    public void OnNavigatedTo(LocalWebAppPage page, NavigationEventArgs e);

    /// <summary>
    /// Occurs on navigating from.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="e">The navigation event arguments.</param>
    public void OnNavigatingFrom(LocalWebAppPage page, NavigatingCancelEventArgs e);

    /// <summary>
    /// Occurs on navigated from.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="e">The navigation event arguments.</param>
    public void OnNavigatedFrom(LocalWebAppPage page, NavigationEventArgs e);

    /// <summary>
    /// Occurs on the error notification is shown.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="element">The error notification.</param>
    /// <param name="ex">The exception thrown.</param>
    public void OnErrorNotification(LocalWebAppPage page, InfoBar element, Exception ex);
}

/// <summary>
/// The monitor of the local web app page.
/// </summary>
public class BaseLocalWebAppPageMonitor : ILocalWebAppPageMonitor
{
    /// <summary>
    /// Occurs on creating.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="webview">The WebView2 instance.</param>
    public virtual void OnCreate(LocalWebAppPage page, WebView2 webview)
    {
    }

    /// <summary>
    /// Occurs on navigated to.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="e">The navigation event arguments.</param>
    public virtual void OnNavigatedTo(LocalWebAppPage page, NavigationEventArgs e)
    {
    }

    /// <summary>
    /// Occurs on navigating from.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="e">The navigation event arguments.</param>
    public virtual void OnNavigatingFrom(LocalWebAppPage page, NavigatingCancelEventArgs e)
    {
    }

    /// <summary>
    /// Occurs on navigated from.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="e">The navigation event arguments.</param>
    public virtual void OnNavigatedFrom(LocalWebAppPage page, NavigationEventArgs e)
    {
    }

    /// <summary>
    /// Occurs on the error notification is shown.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="element">The error notification.</param>
    /// <param name="ex">The exception thrown.</param>
    public virtual void OnErrorNotification(LocalWebAppPage page, InfoBar element, Exception ex)
    {
    }
}
