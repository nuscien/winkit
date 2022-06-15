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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
    private LocalWebAppHost host;
    private readonly Dictionary<string, LocalWebAppMessageProcessAsync> proc = new();

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
    public async Task LoadAsync(LocalWebAppOptions options)
    {
        if (options == null) return;
        var host = await LocalWebAppHost.LoadAsync(options);
        await LoadAsync(host);
    }

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    public async Task LoadAsync(Task<LocalWebAppHost> host)
    {
        if (host == null) return;
        await LoadAsync(await host);
    }

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    public async Task LoadAsync(LocalWebAppHost host)
    {
        //Browser.NavigateToString(@"<html><head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" ><base target=""_blank"" /></head><body></body></html>");
        if (host == null) return;
        this.host = host;
        await Browser.EnsureCoreWebView2Async();
        Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(host.VirtualHost, host.ResourcePackageDirectory.FullName, CoreWebView2HostResourceAccessKind.Allow);
        var homepage = host.Manifest.HomepagePath?.Trim();
        if (string.IsNullOrEmpty(homepage)) homepage = "index.html";
        if (host.Options?.DebugMode ?? false)
        {
            // ToDo: Show debug notification prompt.
        }

        Browser.CoreWebView2.Navigate(host.GetVirtualPath(homepage));
    }

    private void OnDocumentTitleChanged(CoreWebView2 sender, object args)
        => TitleChanged?.Invoke(this, new DataEventArgs<string>(sender.DocumentTitle));

    /// <summary>
    /// Stops navigating.
    /// </summary>
    public void Close()
    {
        var webview2 = Browser.CoreWebView2;
        if (webview2 == null) return;
        Browser.Close();
    }

    private void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {
        var settings = Browser.CoreWebView2.Settings;
        var isDebug = host.Options?.DebugMode ?? false;
        settings.AreDevToolsEnabled = isDebug;
        settings.AreDefaultContextMenusEnabled = false;
        Browser.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
        var sb = new StringBuilder();
        sb.Append(@"(function () { if (window.edgePlatform) return;
let postMsg = window.chrome && window.chrome.webview && typeof window.chrome.webview.postMessage === 'function' ? function (data) { window.chrome.webview.postMessage(data); } : function (data) { };
let hs = []; let stepNumber = 0;
function genRandomStr() {
  if (stepNumber >= Number.MAX_SAFE_INTEGER) stepNumber = 0; stepNumber++;
  return 'r' + Math.floor(Math.random() * 46656).toString(36) + stepNumber.toString(36) + (new Date().getTime().toString(36));
}
function sendRequest(handlerId, cmd, data, info, context, noResp) {
  let req = { handler: handlerId, cmd, data, info, context }; let promise = null;
  if (!noResp) { req.trace = genRandomStr();
    promise = new Promise(function (resolve, reject) {
      let handler = {};
      handler.proc = function (ev) {
        if (!ev || !ev.data || ev.data.trace != req.trace) return;
        handler.invalid = true;
        if (ev.data.error) reject(ev.data);
        else resolve(ev.data);
    }; hs.push(handler);
  }); }
  postMsg(req); return promise;
}
if (postMsg && typeof window.chrome.webview.addEventListener === 'function') {
  try {
    window.chrome.webview.addEventListener('message', function (ev) {
      let removing = [];
      for (let i in hs) {
        let item = hs[i];
        if (!item) continue;
        if (typeof item.proc === 'function') {
          if (item.invalid != null) {
            let toRemove = false;
            if (item.invalid === true) {
              toRemove = true;
            } else if (typeof item.invalid === 'function') {
              if (item.test(ev)) toRemove = true;
            } else if (typeof item.invalid === 'number') {
              if (item.invalid <= 0) toRemove = true;
              else item.invalid--;
            }
            if (toRemove && !item.keep) {
              removing.push(item); continue;
            }
          }
          item.proc(ev);
          continue;
        }
        if (typeof item === 'function') item(ev);
      }
      for (let i in removing) {
        let item = removing[i]; if (!item) continue;
        let j = hs.indexOf(removing[i]);
        if (j >= 0) hs.splice(j, 1);
        if (typeof item.dispose === 'function') item.dispose();
      }
    });
  } catch (ex) { }
}
window.edgePlatform = { 
  onMessage(callback) {
    if (!callback) return;
    if (typeof callback === 'function' || typeof callback.proc === 'function') hs.push(callback);
  },
  files: {
    list(dir, options) {
      if (!options) options = {};
      return sendRequest(null, 'list-file', { path: dir, q: options.q, showHidden: options.showHidden }, null, options.context);
    },
    get(path, options) {
      if (!options) options = {};
      return sendRequest(null, 'get-file', { path, read: options.read }, null, options.context);
    }
  },
  hostInfo: ");
        sb.Append(LocalWebAppExtensions.GetEnvironmentInformation(host.Manifest, isDebug).ToString(IndentStyles.Compact));
        sb.Append(", resources: ");
        var resourceReg = new JsonObjectNode();
        sb.Append(resourceReg.ToString(IndentStyles.Compact));
        sb.AppendLine(" }; })();");
        _ = Browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(sb.ToString());
    }

    private void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
    }

    private void OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
    }

    private void OnWebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        if (sender == null) return;
        JsonObjectNode json = null;
        try
        {
            var msg = args.WebMessageAsJson;
            json = JsonObjectNode.TryParse(msg);
        }
        catch (FormatException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        _ = OnWebMessageReceivedAsync(sender, json);
    }

    private async Task OnWebMessageReceivedAsync(WebView2 sender, JsonObjectNode json)
    {
        if (json == null) return;
        json = await LocalWebAppExtensions.OnWebMessageReceivedAsync(host, json, sender.Source, proc);
        sender.CoreWebView2.PostWebMessageAsJson(json.ToString());
    }

    private void OnCoreProcessFailed(WebView2 sender, CoreWebView2ProcessFailedEventArgs args)
    {
    }
}
