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
using Trivial.IO;
using Trivial.Text;
using Trivial.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.Demo.Nbc;

/// <summary>
/// The news page.
/// </summary>
public sealed partial class NewsPage : Page
{
    /// <summary>
    /// Initializes a new instance of the NewsPage.
    /// </summary>
    public NewsPage()
    {
        InitializeComponent();
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        var arr = await DataService.GetRawAsync();
        LoadingElement.IsActive = false;
        if (arr == null) return;
        foreach (var item in arr)
        {
            var title = item?.TryGetStringValue("listTitle");
            var items = item?.TryGetArrayValue("items");
            if (string.IsNullOrWhiteSpace(title) || items == null || items.Length < 1) continue;
            var col = items.OfType<JsonObjectNode>().Select(ele => (ItemModel)ele.TryGetObjectValue("data")).Where(ele => ele != null);
            AddTileCollection(title, col);
        }
    }

    private void MainPanel_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Item is TileCollection c) c.UsePrepareImageUri();

        //if (args.ItemIndex < MainPanel.Items.Count - 1) return;
    }

    private void AddTileCollection(string title, IEnumerable<ItemModel> col)
    {
        var c = Helper.CreateTileCollection(title, col, OnTileClick);
        if (c == null || c.Children.Count < 1) return;
        MainPanel.Items.Add(c);
    }

    private void OnTileClick(TileItem c, ItemModel m, RoutedEventArgs e)
    {
        var path = m?.Source?.TryGetStringValue("permalink") ?? m?.Source?.TryGetStringValue("urlAlias");
        if (string.IsNullOrWhiteSpace(path)) return;
        var url = string.Concat("https://www.nbc.com", path.StartsWith('/') ? string.Empty : "/", path);
        _ = Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    }

}
