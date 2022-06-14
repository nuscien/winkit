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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Trivial.UI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LocalWebAppPage : Page
{
    private LocalWebAppManifest manifest;

    /// <summary>
    /// Initializes a new instance of the LocalWebAppPage class.
    /// </summary>
    public LocalWebAppPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Adds or removes an event occured on title changed.
    /// </summary>
    public event DataEventHandler<string> TitleChanged;

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="options">The options of the standalone web app.</param>
    public async Task LoadDataAsync(LocalWebAppOptions options)
    {
        //Browser.NavigateToString(@"<html><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" ><base target=""_blank"" /></head><body></body></html>");
        if (options == null) return;
        manifest = await LocalWebAppManifest.LoadAsync(options);
        await Browser.EnsureCoreWebView2Async();
        var settings = Browser.CoreWebView2.Settings;
        settings.AreDevToolsEnabled = false;
        settings.AreDefaultContextMenusEnabled = false;
        Browser.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
        Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(options.VirtualHost, options.RootDirectory.FullName, Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
        Browser.CoreWebView2.Navigate(options.GetVirtualPath(manifest.HomepagePath ?? "index.html"));
    }

    private void OnDocumentTitleChanged(CoreWebView2 sender, object args)
        => TitleChanged?.Invoke(this, new DataEventArgs<string>(sender.DocumentTitle));

    /// <summary>
    /// Stops navigating.
    /// </summary>
    public void Stop()
    {
        var webview2 = Browser.CoreWebView2;
        if (webview2 == null) return;
        Browser.CoreWebView2.Stop();
    }

    private void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {
    }

    private void OnNavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
    {
    }

    private void OnNavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
    {
    }

    private void OnWebMessageReceived(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
    {
    }

    private void OnCoreProcessFailed(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs args)
    {
    }
}
