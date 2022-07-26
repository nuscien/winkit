using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.IO;
using Trivial.Security;
using Trivial.Text;
using Trivial.UI;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.Demo;

/// <summary>
/// The homepage
/// </summary>
public sealed partial class HomePage : Page
{
    private LocalWebAppHost webAppHost;

    /// <summary>
    /// Initializes a new instance of the HomePage class.
    /// </summary>
    public HomePage()
    {
        InitializeComponent();
        for (var i = 0; i < 1000; i++)
        {
            TextViewElement.Append(new[]
            {
                "Welcome!",
                "This is a demo for Trivial.WindowsKit library.",
                "It can be used for pages of news, video and product.",
                string.Empty,
                "This demo shows basic visual samples based on some 3rd-party data sources.",
                "Copyrights of these data sources are reserved by their owner.",
                string.Empty
            });
        }

        FileBrowserElement.NavigateAsync(new DirectoryInfo("C:\\"));
    }

    private async Task<LocalWebAppHost> CreateWebAppHostAsync()
    {
        if (webAppHost == null || webAppHost.NewVersionAvailable != null) webAppHost = await LocalWebAppHost.LoadAsync("WinKitDemo", null);
        return webAppHost;
    }

    private void LaunchWebAppClick(object sender, RoutedEventArgs e)
    {
        var hostTask = CreateWebAppHostAsync();
        Load(hostTask);
    }

    private void SignWebAppClick(object sender, RoutedEventArgs e)
    {
        //var window = new LocalWebAppWindow()
        //{
        //    IsDevEnvironmentEnabled = true
        //};
        //var hostTask = CreateWebAppHostAsync();
        //_ = window.SelectDevPackageAsync("WinKitDemo");
        //var appWin = VisualUtility.TryGetAppWindow(window);
        //window.Activate();
        //return;

        var rootDir = "";  // The root path of the repo.
        var dir = new DirectoryInfo(Path.Combine(rootDir, "FileBrowser"));
        foreach (var subDir in dir.EnumerateDirectories())
        {
            var name = subDir.Name;
            if (name == "css" || name == "dist" || name == "images" || name == "data")
                subDir.CopyTo(Path.Combine(rootDir, "bin\\LocalWebApp\\app", name));
        }

        foreach (var html in dir.EnumerateFiles("*.html"))
        {
            html.CopyTo(Path.Combine(rootDir, "bin\\LocalWebApp\\app", html.Name), true);
        }

        var package = LocalWebAppHost.Package("WinKitDemo", new DirectoryInfo(Path.Combine(rootDir, "FileBrowser")));
        var hostTask = LocalWebAppHost.LoadAsync(package, true);
        Load(hostTask);
    }

    private void BrowserClick(object sender, RoutedEventArgs e)
    {
        var win = new TabbedWebViewWindow();
        win.Add(new Uri("https://kingcean.net"));
        win.Activate();
    }

    private void Load(Task<LocalWebAppHost> hostTask)
    {
        var window = new LocalWebAppWindow()
        {
            IsDevEnvironmentEnabled = true
        };
        _ = window.LoadAsync(hostTask, host => _ = host?.UpdateAsync());
        window.IconImageSource = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage
        {
            UriSource = new Uri(BaseUri, "\\Assets\\Square44x44Logo.scale-100.png")
        };
        var appWin = VisualUtility.TryGetAppWindow(window);
        window.Activate();
    }
}
