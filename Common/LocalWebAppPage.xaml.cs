﻿using Microsoft.UI.Xaml;
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
    /// Occurs when the web view core processes failed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2ProcessFailedEventArgs> CoreProcessFailed;

    /// <summary>
    /// Occurs when the web view core has initialized.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2InitializedEventArgs> CoreWebView2Initialized;

    /// <summary>
    /// Occurs when the navigation is completed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;

    /// <summary>
    /// Occurs when the navigation is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NavigationStartingEventArgs> NavigationStarting;

    /// <summary>
    /// Occurs when the web message core has received.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2WebMessageReceivedEventArgs> WebMessageReceived;

    /// <summary>
    /// Occurs when the new window request sent.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested;

    /// <summary>
    /// Occurs when the webpage send request to close itself.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, object> WindowCloseRequested;

    /// <summary>
    /// Occurs when the downloading is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2DownloadStartingEventArgs> DownloadStarting;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is created.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2FrameCreatedEventArgs> FrameCreated;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is completed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NavigationCompletedEventArgs> FrameNavigationCompleted;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is starting.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, CoreWebView2NavigationStartingEventArgs> FrameNavigationStarting;

    /// <summary>
    /// Occurs when the history has changed.
    /// </summary>
    public event TypedEventHandler<LocalWebAppPage, object> HistoryChanged;

    /// <summary>
    /// Occurs when fullscreen request sent including to enable and disable.
    /// </summary>
    public event DataEventHandler<bool> ContainsFullScreenElementChanged;

    /// <summary>
    /// Adds or removes an event occured on title changed.
    /// </summary>
    public event DataEventHandler<string> TitleChanged;

    /// <summary>
    /// Gets the download list.
    /// </summary>
    public List<CoreWebView2DownloadOperation> DownloadList { get; } = new();

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
        if (host.Options?.IsDevEnvironmentEnabled ?? false)
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
        var isDebug = host.Options?.IsDevEnvironmentEnabled ?? false;
        settings.AreDevToolsEnabled = isDebug;
        settings.AreDefaultContextMenusEnabled = false;
        sender.CoreWebView2.DownloadStarting += OnDownloadStarting;
        sender.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
        sender.CoreWebView2.DownloadStarting += OnDownloadStarting;
        sender.CoreWebView2.FrameCreated += OnFrameCreated;
        sender.CoreWebView2.FrameNavigationStarting += OnFrameNavigationStarting;
        sender.CoreWebView2.FrameNavigationCompleted += OnFrameNavigationCompleted;
        sender.CoreWebView2.HistoryChanged += OnHistoryChanged;
        sender.CoreWebView2.ContainsFullScreenElementChanged += OnContainsFullScreenElementChanged;
        sender.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        sender.CoreWebView2.WindowCloseRequested += OnWindowCloseRequested;
        CoreWebView2Initialized?.Invoke(this, args);
        var sb = new StringBuilder();
        sb.Append(@"(function () { if (window.edgePlatform) return;
let postMsg = window.chrome && window.chrome.webview && typeof window.chrome.webview.postMessage === 'function' ? function (data) { window.chrome.webview.postMessage(data); } : function (data) { };
let hs = []; let stepNumber = 0;
function genRandomStr() {
  if (stepNumber >= Number.MAX_SAFE_INTEGER) stepNumber = 0; stepNumber++;
  return 'r' + Math.floor(Math.random() * 46655).toString(36) + stepNumber.toString(36) + (new Date().getTime().toString(36));
}
function sendRequest(handlerId, cmd, data, info, context, noResp) {
  let req = { handler: handlerId, cmd, data, info, context, date: new Date() }; let promise = null;
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
  getCommandHandler(id) {
    if (!id || typeof id !== 'string') return null;
    return {
      id() {
        return id;
      },
      call(cmd, data, context, info) {
        sendRequest(id, cmd, data, info, context, false)
      },
      request(cmd, data, context, info) {
        sendRequest(id, cmd, data, info, context, true)
      }
    };
  },
  getCookie: function (key) {
    if (!key) return document.cookie;
    key = key + '='; let ca = document.cookie.split(';');
    for (let i in ca) {
      let c = ca[i]; while (c.charAt(0) == ' ') { c = c.substring(1); } if (c.startsWith(key)) return c.substring(key.length);
    }
    return '';
  },
  files: {
    list(dir, options) {
      if (!options) options = {};
      else if (typeof options === 'string') options = { q: options }
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'list-file', { path: dir, q: options.q, showHidden: options.showHidden }, null, options.context);
    },
    get(path, options) {
      if (!options) options = {};
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'get-file', { path, read: options.read }, null, options.context);
    },
    write(path, value, options) {
      if (!options) options = {};
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'write-file', { path, value }, null, options.context);
    },
    open(path, options) {
      if (!options) options = {};
      else if (typeof options === 'string') options = { args: options }
      if (options.appData && path) path = '.data:\\' + path;
      return sendRequest(null, 'open', { path, args: options.args, type: options.type }, null, options.context);
    }
  },
  cryptography: {
    encrypt(alg, value, key, iv, options) {
      if (!options) options = {};
      return sendRequest(null, 'symmetric', { value, alg, key, iv, decrypt: false }, null, options.context);
    },
    decrypt(alg, value, key, iv, options) {
      if (!options) options = {};
      return sendRequest(null, 'symmetric', { value, alg, key, iv, decrypt: true }, null, options.context);
    },
    hash(alg, value, options) {
      if (!options) options = {};
      else if (typeof options === 'string') options = { test: options }
      return sendRequest(null, 'hash', { value, alg, test: options.test, type: options.type }, null, options.context);
    }
  },
  text: {
    encodeBase64(s, url) {
      if (!s) return s; if (typeof s !== 'string') return null;
      s = window.btoa(s); return url ? s.replace(/\+/g, '-').replace(/\//g, '_').replace(/\=/g, '') : s;
    },
    decodeBase64(s, url) {
      if (!s) return s; if (typeof s !== 'string') return null;
      if (url) s = s.replace(/\-/g, '+').replace(/_/g, '/').replace(/\=/g, '');
      s = window.atob(s); return s;
    },
    encodeUri(s, parameter) {
      return s ? (parameter ? encodeURIComponent(s) : encodeURI(s)) : s;
    },
    decodeUri(s, parameter) {
      return s ? (parameter ? decodeURIComponent(s) : decodeURI(s)) : s;
    }
  },
  download: {
  },
  hostInfo: ");
        sb.Append(LocalWebAppExtensions.GetEnvironmentInformation(host.Manifest, isDebug).ToString(IndentStyles.Compact));
        sb.Append(", dataRes: ");
        var resourceReg = new JsonObjectNode();
        sb.Append(resourceReg.ToString(IndentStyles.Compact));
        sb.Append(", strRes: ");
        resourceReg = new JsonObjectNode();
        sb.Append(resourceReg.ToString(IndentStyles.Compact));
        sb.AppendLine(" }; })();");
        _ = Browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(sb.ToString());
    }

    private void OnDownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        DownloadList.Add(args.DownloadOperation);
        DownloadStarting?.Invoke(this, args);
    }
    private void OnContainsFullScreenElementChanged(CoreWebView2 sender, object args)
        => ContainsFullScreenElementChanged?.Invoke(this, new DataEventArgs<bool>(Browser.CoreWebView2.ContainsFullScreenElement));

    private void OnNewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        => NewWindowRequested?.Invoke(this, args);

    private void OnWindowCloseRequested(CoreWebView2 sender, object args)
        => WindowCloseRequested?.Invoke(this, args);

    private void OnFrameCreated(CoreWebView2 sender, CoreWebView2FrameCreatedEventArgs args)
        => FrameCreated?.Invoke(this, args);

    private void OnFrameNavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        => FrameNavigationStarting?.Invoke(this, args);

    private void OnFrameNavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        => FrameNavigationCompleted?.Invoke(this, args);

    private void OnHistoryChanged(CoreWebView2 sender, object args)
        => HistoryChanged?.Invoke(this, args);

    private void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        ProgressElement.IsActive = false;
        NavigationCompleted?.Invoke(this, args);
    }

    private void OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        => NavigationStarting?.Invoke(this, args);

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
        WebMessageReceived?.Invoke(this, args);
    }

    private async Task OnWebMessageReceivedAsync(WebView2 sender, JsonObjectNode json)
    {
        if (json == null) return;
        json = await LocalWebAppExtensions.OnWebMessageReceivedAsync(host, json, sender.Source, proc);
        sender.CoreWebView2.PostWebMessageAsJson(json.ToString());
    }

    private void OnCoreProcessFailed(WebView2 sender, CoreWebView2ProcessFailedEventArgs args)
    {
        ProgressElement.IsActive = false;
        CoreProcessFailed?.Invoke(this, args);
    }
}