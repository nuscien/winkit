using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Text;
using Trivial.Web;

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

/// <summary>
/// The options for local web app hub page.
/// </summary>
public sealed partial class LocalWebAppHubPageNavigationOptions
{
    /// <summary>
    /// Gets the additional dev apps.
    /// </summary>
    public List<LocalWebAppInfo> AdditionalDevApps { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the dev mode is disabled.
    /// </summary>
    public bool IsDevModeDisabled { get; set; }

    /// <summary>
    /// Gets or sets the handler to prevent app opening.
    /// </summary>
    public Func<LocalWebAppHubPage, LocalWebAppInfo, bool, bool> PreventAppHandler { get; set; }

    /// <summary>
    /// Gets or sets the handler to open local web app.
    /// </summary>
    public Func<LocalWebAppHubPage, LocalWebAppInfo, bool, Task> OpenHandler { get; set; }

    /// <summary>
    /// Gets or sets the handler to open a dialog to load a dev app.
    /// </summary>
    public Func<LocalWebAppHubPage, Task<DirectoryInfo>> SelectDevAppHandler { get; set; }

    /// <summary>
    /// Gets or sets the handler to open a dialog to create a dev app.
    /// </summary>
    public Func<LocalWebAppHubPage, LocalWebAppPage, bool> CreateDevAppHandler { get; set; }
}
