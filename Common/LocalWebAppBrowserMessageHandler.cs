using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Text;
using Trivial.UI;

namespace Trivial.Web;


/// <summary>
/// The browser message handler for local standalone web app.
/// </summary>
public interface ILocalWebAppBrowserMessageHandler
{
    /// <summary>
    /// Gets download list information.
    /// </summary>
    /// <param name="open">true if open the default dialog; false if hide; or null, no action.</param>
    /// <param name="maxCount">The maximum count of download item to return.</param>
    JsonObjectNode DownloadListInfo(bool? open, int maxCount = 256);
}

internal class LocalWebAppBrowserMessageHandler : ILocalWebAppBrowserMessageHandler
{
    private readonly WebView2 webview;

    /// <summary>
    /// Initializes a new instance of the LocalWebAppBrowserMessageHandler class.
    /// </summary>
    /// <param name="webview">The web view element.</param>
    public LocalWebAppBrowserMessageHandler(WebView2 webview)
    {
        this.webview = webview;
    }

    public List<CoreWebView2DownloadOperation> DownloadList { get; } = new();

    /// <summary>
    /// Gets download list information.
    /// </summary>
    /// <param name="open">true if open the default dialog; false if hide; or null, no action.</param>
    /// <param name="maxCount">The maximum count of download item to return.</param>
    public JsonObjectNode DownloadListInfo(bool? open, int maxCount = 256)
    {
        if (open.HasValue)
        {
            if (open.Value) webview.CoreWebView2.OpenDefaultDownloadDialog();
            else webview.CoreWebView2.CloseDefaultDownloadDialog();
        }

        var arr = new JsonArrayNode();
        foreach (var item in DownloadList)
        {
            arr.Add(new JsonObjectNode
            {
                { "uri", item.Uri },
                { "file", item.ResultFilePath },
                { "state", item.State.ToString() },
                { "received", item.BytesReceived },
                { "length", item.TotalBytesToReceive },
                { "interrupt", item.InterruptReason.ToString() },
                { "mime", item.MimeType }
            });
        }

        return new JsonObjectNode
        {
            { "dialog", open ?? webview.CoreWebView2.IsDefaultDownloadDialogOpen },
            { "max", maxCount },
            { "list", arr },
            { "enumerated", DateTime.Now }
        };
    }
}
