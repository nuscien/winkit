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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Text;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Trivial.UI;

/// <summary>
/// The hub page of local web app.
/// </summary>
public sealed partial class LocalWebAppHubPage : Page
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppHubPage class.
    /// </summary>
    public LocalWebAppHubPage()
    {
        InitializeComponent();
        _ = OnInitAsync();
    }

    /// <summary>
    /// Gets or sets the uniform distance between views in page.
    /// </summary>
    public double Spacing
    {
        get => HostElement.Spacing;
        set => HostElement.Spacing = value;
    }

    /// <summary>
    /// Gets or sets the content of header.
    /// </summary>
    public UIElement HeaderContent
    {
        get => HeaderContainer.Child;
        set => HeaderContainer.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of footer.
    /// </summary>
    public UIElement FooterContent
    {
        get => FooterContainer.Child;
        set => FooterContainer.Child = value;
    }

    /// <summary>
    /// Gets or sets the content of more items.
    /// </summary>
    public UIElement MoreContent
    {
        get => MoreItemsContainer.Child;
        set => MoreItemsContainer.Child = value;
    }

    /// <summary>
    /// Gets or sets the handler to open local web app.
    /// </summary>
    public Func<LocalWebAppInfo, bool, Task> OpenHandler { get; set; }

    /// <summary>
    /// Gets or sets the handler to open a dialog to load a dev app.
    /// </summary>
    public Func<Task<DirectoryInfo>> SelectDevAppHandler { get; set; }

    /// <summary>
    /// Reloads the page.
    /// </summary>
    public void Refresh()
        => _ = OnInitAsync();

    /// <summary>
    /// Shows dev panel or not.
    /// </summary>
    /// <param name="value">true if show dev panel; otherwise, false.</param>
    public void ShowDevPanel(bool value)
    {
        DevShowButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        DevList.Visibility = MoreItemsContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task OnInitAsync(Task previous = null)
    {
        UpdateText(DevShowButtonText, LocalWebAppHook.CustomizedLocaleStrings.DevModeShowTitle);
        UpdateText(DevTitleText, LocalWebAppHook.CustomizedLocaleStrings.DevModeTitle);
        if (previous != null)
        {
            await previous;
            await Task.Delay(100);
        }

        (var list1, var list2) = await LocalWebAppHost.ListAllPackageAsync();
        InstalledList.ItemsSource = FormatList(list1);
        AddPlus(list2);
        DevList.ItemsSource = FormatList(list2);
    }

    private void AddPlus(List<LocalWebAppInfo> list)
    {
        var icon = LocalWebAppHook.SelectDevAppIconPath;
        if (string.IsNullOrWhiteSpace(icon)) icon = new Uri(BaseUri, "Assets\\SearchLwa_128.png").OriginalString;
        list.Add(new()
        {
            ResourcePackageId = "+",
            LocalPath = "+",
            Icon = icon,
            DisplayName = LocalWebAppHook.CustomizedLocaleStrings.DevModeAddTitle
        });
    }

    private List<LocalWebAppInfo> FormatList(List<LocalWebAppInfo> list)
    {
        if (list == null) return null;
        list.Reverse();
        var defaultIcon = new Uri(BaseUri, "Assets\\DefaultLwa_128.png").OriginalString;
        foreach (var item in list)
        {
            if (item == null) continue;
            if (string.IsNullOrWhiteSpace(item.DisplayName)) item.DisplayName = item.ResourcePackageId;
            if (string.IsNullOrEmpty(item.Icon) || (!item.Icon.Contains("://") && !File.Exists(item.Icon)))
            {
                item.Icon = LocalWebAppHook.DefaultIconPath;
                if (string.IsNullOrEmpty(item.Icon)) item.Icon = defaultIcon;
            }
        }

        return list;
    }

    private void OnItemButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not LocalWebAppInfo info) return;
        if (string.IsNullOrWhiteSpace(info.ResourcePackageId)) return;
        var h = OpenHandler;
        if (h != null)
        {
            _ = h(info, false);
            return;
        }

        var win = new LocalWebAppWindow();
        win.Activate();
        _ = win.LoadAsync(info.ResourcePackageId);
    }

    private void OnDevItemButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not LocalWebAppInfo info) return;
        _ = OnDevItemButtonClickAsync(info);
    }

    private void OnItemRemoveButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not LocalWebAppInfo info) return;
        _ = OnInitAsync(LocalWebAppHost.RemovePackageAsync(info.ResourcePackageId));
    }

    private async Task<bool> OnDevItemButtonClickAsync(LocalWebAppInfo info)
    {
        var h = OpenHandler;
        DirectoryInfo dir;
        var needRefresh = false;
        if (info.ResourcePackageId == "+" && info.LocalPath == "+")
        {
            if (SelectDevAppHandler == null) return false;
            dir = await SelectDevAppHandler();
            if (dir == null || !dir.Exists) return false;
            try
            {
                info.LocalPath = dir.FullName;
                info.ResourcePackageId = null;
                needRefresh = true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (System.Security.SecurityException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                return false;
            }
        }
        else
        {
            dir = IO.FileSystemInfoUtility.TryGetDirectoryInfo(info.LocalPath);
        }

        if (h != null)
        {
            await h(info, true);
            if (needRefresh) Refresh();
            return true;
        }

        if (dir != null && dir.Exists)
        {
            var win = new LocalWebAppWindow();
            win.Activate();
            await win.LoadDevPackageAsync(dir);
            var list = await LocalWebAppHost.ListPackageAsync(true);
            AddPlus(list);
            DevList.ItemsSource = FormatList(list);
            if (needRefresh) Refresh();
            return true;
        }
        else if (!string.IsNullOrWhiteSpace(info.ResourcePackageId))
        {
            var win = new LocalWebAppWindow();
            win.Activate();
            _ = win.LoadAsync(info.ResourcePackageId);
            return true;
        }

        return false;
    }

    private void OnDevItemRemoveButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not LocalWebAppInfo info) return;
        _ = OnInitAsync(LocalWebAppHost.RemovePackageAsync(info.ResourcePackageId, true));
    }

    private void OnDevShowButtonClick(object sender, RoutedEventArgs e)
        => ShowDevPanel(true);

    private static bool UpdateText(TextBlock element, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(defaultValue)) return true;
        element.Text = defaultValue;
        return true;
    }

    /// <summary>
    /// Creates a window with the hub page.
    /// </summary>
    /// <param name="title">The page title.</param>
    /// <param name="page">The hub page output.</param>
    /// <returns>The window.</returns>
    public static TabbedWebViewWindow CreateWindow(string title, out LocalWebAppHubPage page)
    {
        var win = new TabbedWebViewWindow();
        page = new LocalWebAppHubPage
        {
            OpenHandler = win.OpenLocalWebApp,
            SelectDevAppHandler = () => VisualUtility.SelectFolderAsync(win)
        };
        if (string.IsNullOrWhiteSpace(title)) title = "Apps";
        win.Add(new TabViewItem
        {
            Header = title,
            Content = page,
            IsSelected = true,
        });
        return win;
    }

    /// <summary>
    /// Creates a window with the hub page.
    /// </summary>
    /// <param name="title">The page title.</param>
    /// <returns>The window.</returns>
    public static TabbedWebViewWindow CreateWindow(string title)
        => CreateWindow(title, out _);
}
