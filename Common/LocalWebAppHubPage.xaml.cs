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
    private bool devModeDisabled;
    private bool noAdd;
    private readonly List<LocalWebAppInfo> additionalDevApps = new();

    /// <summary>
    /// Initializes a new instance of the LocalWebAppHubPage class.
    /// </summary>
    public LocalWebAppHubPage()
    {
        InitializeComponent();
        _ = OnInitAsync();
    }

    /// <summary>
    /// Gets or sets a value indicating whether need to hide the button which is to enable dev mode.
    /// </summary>
    public bool IsDevModeButtonHidden
    {
        get
        {
            return devModeDisabled;
        }

        set
        {
            devModeDisabled = value;
            DevShowButton.Visibility = value || DevList.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
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

    /// <summary>
    /// Gets or sets the handler to filter the apps.
    /// </summary>
    public Func<LocalWebAppInfo, bool> PredicateHandler { get; set; }

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
        DevShowButton.Visibility = devModeDisabled || value ? Visibility.Collapsed : Visibility.Visible;
        DevList.Visibility = MoreItemsContainer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Adds an additional dev local web app.
    /// </summary>
    /// <param name="info">The information of the dev local web app.</param>
    /// <returns>true if add succeeded; otherwise, false.</returns>
    public bool AddAdditionalDevApp(LocalWebAppInfo info)
    {
        if (string.IsNullOrWhiteSpace(info?.ResourcePackageId) || additionalDevApps.Contains(info)) return false;
        additionalDevApps.Add(info);
        return true;
    }

    /// <summary>
    /// Removes an additional dev local web app.
    /// </summary>
    /// <param name="info">The information of the dev local web app.</param>
    /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the collection.</returns>
    public bool RemoveAdditionalDevApp(LocalWebAppInfo info)
    {
        if (info == null) return false;
        return additionalDevApps.Remove(info);
    }

    /// <summary>
    /// Removes an additional dev local web app.
    /// </summary>
    /// <param name="id">The identifier of the dev local web app.</param>
    /// <returns>The number of elements removed from the collection.</returns>
    public int RemoveAdditionalDevApp(string id)
        => additionalDevApps.RemoveAll(ele => ele?.ResourcePackageId?.Equals(id, StringComparison.OrdinalIgnoreCase) != false);

    /// <inheritdoc />
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is IPageNavigationCallbackParameter<LocalWebAppHubPage> c) c.OnNavigate(this, e);
        else return;
        _ = OnInitAsync();
    }

    private async Task OnInitAsync(Task previous = null)
    {
        UpdateText(DevShowButtonText, LocalWebAppSettings.CustomizedLocaleStrings.DevModeShowTitle);
        UpdateText(DevTitleText, LocalWebAppSettings.CustomizedLocaleStrings.DevModeTitle);
        if (previous != null)
        {
            await previous;
            await Task.Delay(100);
        }

        (var list1, var list2) = await LocalWebAppHost.ListAllPackageAsync();
        var filter = PredicateHandler;
        InstalledList.ItemsSource = filter != null ? FormatList(list1.Where(filter).ToList()) : FormatList(list1);
        if (!noAdd) AddPlus(list2);
        list2 = FormatList(list2);
        var list3 = new List<LocalWebAppInfo>(FormatList(additionalDevApps, true));
        foreach (var item in list3)
        {
            if (string.IsNullOrWhiteSpace(item?.ResourcePackageId) || string.IsNullOrWhiteSpace(item?.DisplayName) || list2.Contains(item)) continue;
            list2.Add(item);
        }

        DevList.ItemsSource = list2;
    }

    private void AddPlus(List<LocalWebAppInfo> list)
    {
        var icon = LocalWebAppSettings.SelectDevAppIconPath;
        if (string.IsNullOrWhiteSpace(icon)) icon = new Uri(BaseUri, "Assets\\SearchLwa_128.png").OriginalString;
        list.Add(new()
        {
            ResourcePackageId = "+",
            LocalPath = "+",
            Icon = icon,
            DisplayName = GetString(LocalWebAppSettings.CustomizedLocaleStrings.DevModeAddTitle, "Open")
        });
    }

    private List<LocalWebAppInfo> FormatList(List<LocalWebAppInfo> list, bool doNotReverse = false)
    {
        if (list == null) return null;
        if (!doNotReverse) list.Reverse();
        var defaultIcon = new Uri(BaseUri, "Assets\\DefaultLwa_128.png").OriginalString;
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item?.ResourcePackageId)) continue;
            if (string.IsNullOrWhiteSpace(item.DisplayName)) item.DisplayName = item.ResourcePackageId;
            if (string.IsNullOrEmpty(item.Icon) || (!item.Icon.Contains("://") && !File.Exists(item.Icon)))
            {
                item.Icon = LocalWebAppSettings.DefaultIconPath;
                if (string.IsNullOrEmpty(item.Icon)) item.Icon = defaultIcon;
            }
        }

        return list;
    }

    private void OnItemButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not LocalWebAppInfo info) return;
        if (string.IsNullOrWhiteSpace(info.ResourcePackageId)) return;
        if (PreventAppHandler?.Invoke(this, info, false) == true) return;
        var h = OpenHandler;
        if (h != null)
        {
            _ = h(this, info, false);
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
            dir = await SelectDevAppHandler(this);
            if (dir == null || !dir.Exists) return false;
            try
            {
                info = new()
                {
                    LocalPath = dir.FullName
                };
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

        if (PreventAppHandler?.Invoke(this, info, true) == true) return false;
        if (h != null)
        {
            await h(this, info, true);
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
        additionalDevApps.Remove(info);
        if (info.ResourcePackageId == "+" && info.LocalPath == "+") noAdd = true;
        _ = OnInitAsync(LocalWebAppHost.RemovePackageAsync(info.ResourcePackageId, true));
    }

    private void OnDevShowButtonClick(object sender, RoutedEventArgs e)
        => ShowDevPanel(true);

    private static string GetString(string value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value;

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
            SelectDevAppHandler = p => VisualUtility.SelectFolderAsync(win)
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
