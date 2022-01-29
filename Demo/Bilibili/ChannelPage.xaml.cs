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
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.IO;
using Trivial.Net;
using Trivial.Text;
using Trivial.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;

namespace Trivial.Demo.Bilibili;

/// <summary>
/// The channel page for Bilibili.
/// </summary>
public sealed partial class ChannelPage : Page
{
    private bool webViewInit;

    /// <summary>
    /// Initializes a new instance of the ChannelPage class.
    /// </summary>
    public ChannelPage()
    {
        InitializeComponent();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var cacheTime = DataService.LatestCache;
        if (!cacheTime.HasValue || (DateTime.Now - cacheTime.Value).TotalHours > 1)
            await DataService.LoadDataAsync();
        if (!cacheTime.HasValue)
        {
            await LoadDataByWebViewAsync(); // Fallback
            return;
        }

        LoadData();
    }

    private void LoadData()
    {
        LoadingElement.IsActive = false;
        AddTileCollection("动画 Animation", DataService.Animation);
        AddTileCollection("番剧 Bangumi Series", DataService.BangumiSeries);
        AddTileCollection("国创 China Original Content", DataService.ChinaOriginalContent);
        AddTileCollection("电影 Movie", DataService.Movie);
        AddTileCollection("电视剧 Series", DataService.Series);
        AddTileCollection("舞蹈 Dance", DataService.Dance);
        AddTileCollection("音乐 Music", DataService.Music);
        AddTileCollection("游戏 Game", DataService.Game);
        AddTileCollection("生活 Life", DataService.Life);
        AddTileCollection("科技 Technology", DataService.Technology);
        AddTileCollection("鬼畜 Kichiku", DataService.Kichiku);
        AddTileCollection("时尚 Fashion", DataService.Fashion);
        AddTileCollection("娱乐 Entertainment", DataService.Entertainment);
        WebViewElement.Visibility = Visibility.Collapsed;
    }

    private void MainPanel_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Item is TileCollection c) c.UsePrepareImageUri();

        //if (args.ItemIndex < MainPanel.Items.Count - 1) return;
    }

    private void AddTileCollection(string title, List<ItemModel> col)
    {
        var c = Helper.CreateTileCollection(title, col, OnTileClick);
        if (c == null || c.Children.Count < 1) return;
        MainPanel.Items.Add(c);
    }

    private void OnTileClick(TileItem c, ItemModel m, RoutedEventArgs e)
    {
        var bv = m?.Source?.TryGetStringValue("bvid");
        if (string.IsNullOrWhiteSpace(bv)) return;
        var url = string.Concat("https://www.bilibili.com/video/", bv);
        _ = Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    }

    private async Task LoadDataByWebViewAsync()
    {
        WebViewElement.Visibility = Visibility.Visible;
        await WebViewElement.EnsureCoreWebView2Async();
        if (!webViewInit)
        {
            webViewInit = true;
            WebViewElement.CoreWebView2.WebResourceResponseReceived += (sender, ev) =>
            {
                if (ev.Response == null || ev.Response.StatusCode >= 400 || !ev.Request.Uri.Contains(DataService.ApiUrl)) return;
                _ = OnReceiveData(ev.Response.GetContentAsync());
            };
        }

        WebViewElement.Source = new Uri(DataService.ApiUrl);
    }

    private async Task OnReceiveData(IAsyncOperation<IRandomAccessStream> streaming)
    {
        var stream = await streaming;
        LoadingElement.IsActive = false;
        var reader = new CharsReader(stream.AsStream());
        var s = reader.ReadToEnd();
        var json = JsonObjectNode.TryParse(s.Replace(",\"\":", ",\"_\":"));
        if (json == null) return;
        DataService.LoadData(json, true);
        LoadData();
    }
}
