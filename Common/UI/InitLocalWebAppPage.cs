using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Web;
using Windows.Foundation;

namespace Trivial.UI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LocalWebAppPage
{
    private void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {
        var settings = sender.CoreWebView2.Settings;
        var isDebug = IsDevEnvironmentEnabled;
        settings.AreDevToolsEnabled = isDebug;
        settings.AreDefaultContextMenusEnabled = false;
        sender.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
        sender.CoreWebView2.DownloadStarting += OnDownloadStarting;
        sender.CoreWebView2.FrameCreated += OnFrameCreated;
        sender.CoreWebView2.FrameNavigationStarting += OnFrameNavigationStarting;
        sender.CoreWebView2.FrameNavigationCompleted += OnFrameNavigationCompleted;
        sender.CoreWebView2.HistoryChanged += OnHistoryChanged;
        sender.CoreWebView2.ContainsFullScreenElementChanged += OnContainsFullScreenElementChanged;
        sender.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        sender.CoreWebView2.WindowCloseRequested += OnWindowCloseRequested;
        sender.CoreWebView2.PermissionRequested += OnPermissionRequested;
        CoreWebView2Initialized?.Invoke(this, args);
        var sb = new StringBuilder();
        sb.Append(@"(function () { if (window.localWebApp) return;
let postMsg = window.chrome && window.chrome.webview && typeof window.chrome.webview.postMessage === 'function' ? function (data) { window.chrome.webview.postMessage(data); } : function (data) { };
let hs = []; let stepNumber = 0;
function genRandomStr() {
  if (stepNumber >= Number.MAX_SAFE_INTEGER) stepNumber = 0; stepNumber++;
  return 'r' + Math.floor(Math.random() * 46655).toString(36) + stepNumber.toString(36) + (new Date().getTime().toString(36));
}
function sendRequest(handlerId, cmd, data, info, context, noResp, ref) {
  let req = { handler: handlerId, cmd, data, info, context, date: new Date(), trace: genRandomStr() }; let promise = null;
  if (!noResp) {
    promise = new Promise(function (resolve, reject) {
      let handler = {};
      handler.proc = function (ev) {
        if (!ev || !ev.data || ev.data.trace != req.trace) return;
        handler.invalid = true;
        if (context) ev.context = context;
        if (ev.data.error) reject(ev.data);
        else resolve(ev.data);
        try { if (ref) ref.response = ev.data; } catch (ex) { }
    }; hs.push({ h: handler, type: null });
  }); }
  postMsg(req); try { if (ref) ref.trace = req.trace; } catch (ex) { }
  return promise;
}
if (postMsg && typeof window.chrome.webview.addEventListener === 'function') {
  try {
    window.chrome.webview.addEventListener('message', function (ev) {
      let removing = [];
      for (let i in hs) {
        let source = hs[i] ?? {}; if (!ev || !ev.data || source.type != ev.data.type) continue;
        let item = source.h; if (!item) continue;
        if (typeof item.proc === 'function') {
          if (item.invalid != null) {
            let toRemove = false;
            if (item.invalid === true) {
              toRemove = true;
            } else if (typeof item.invalid === 'function') {
              if (item.invalid(ev)) toRemove = true;
            } else if (typeof item.invalid === 'number') {
              if (item.invalid <= 0) toRemove = true;
              else item.invalid--;
            }
            if (toRemove && !item.keep) {
              removing.push(source); continue;
            }
          }
          item.proc(ev); continue;
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
window.localWebApp = { 
  onMessage(type, callback) {
    if (!callback) return;
    if (typeof callback !== 'function' && typeof callback.proc !== 'function') return;
    hs.push({ h: callback, type });
  },
  getHandler(id) {
    if (!id || typeof id !== 'string') return null;
    return {
      id() {
        return id;
      },
      call(cmd, data, context, info, ref) {
        sendRequest(id, cmd, data, info, context, false, ref)
      },
      request(cmd, data, context, info) {
        return sendRequest(id, cmd, data, info, context, true, ref)
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
      else if (typeof options === 'string') options = { q: options };
      if (options.appData && typeof path === 'string') path = '.data:\\' + path;
      return sendRequest(null, 'list-file', { path: dir, q: options.q, showHidden: options.showHidden }, null, options.context, false, options.ref);
    },
    listDrives(options) {
      if (options === true) options = { fixed: true };
      else if (options === false) options = { fixed: false };
      else if (!options) options = {};
      return sendRequest(null, 'list-drives', { fixed: options.fixed }, null, options.context, false, options.ref);
    },
    get(path, options) {
      if (!options) options = {};
      if (options.appData && typeof path === 'string') path = '.data:\\' + path;
      return sendRequest(null, 'get-file', { path, read: options.read, maxLength: options.maxLength }, null, options.context, false, options.ref);
    },
    write(path, value, options) {
      if (!options) options = {};
      if (options.appData && typeof path === 'string') path = '.data:\\' + path;
      return sendRequest(null, 'write-file', { path, value }, null, options.context, false, options.ref);
    },
    move(path, dest, options) {
      if (options === true) options = { override: true };
      else if (options === false) options = { override: false };
      else if (!options) options = {};
      if (!dest) dest = '';
      if (options.appData && typeof path === 'string') path = '.data:\\' + path;
      return sendRequest(null, 'move-file', { path, dest, override: options.override, dir: options.dir, copy: false }, null, options.context, false, options.ref);
    },
    copy(path, dest, options) {
      if (options === true) options = { override: true };
      else if (options === false) options = { override: false };
      else if (!options) options = {};
      if (options.appData && typeof path === 'string') path = '.data:\\' + path;
      return sendRequest(null, 'move-file', { path, dest, override: options.override, dir: options.dir, copy: true }, null, options.context, false, options.ref);
    },
    delete(path, options) {
      if (!options) options = {};
      if (options.appData && typeof path === 'string') path = '.data:\\' + path;
      return sendRequest(null, 'move-file', { path, dir: options.dir, copy: false }, null, options.context, false, options.ref);
    },
    md(path, options) {
      if (!options) options = {};
      if (options.appData && typeof path === 'string') path = '.data:\\' + path;
      return sendRequest(null, 'make-dir', { path }, null, options.context, false, options.ref);
    },
    open(path, options) {
      if (!options) options = {};
      else if (typeof options === 'string') options = { args: options }
      if (options.appData && typeof path === 'string') path = '.data:\\' + path;
      return sendRequest(null, 'open', { path, args: options.args, type: options.type }, null, options.context, false, options.ref);
    },
    zip(path, dest, options) {
      if (!options) options = {};
      return sendRequest(null, 'zip-file', { path, dest, override: options.override, folder: options.folder }, null, options.context, false, options.ref);
    },
    unzip(path, dest, options) {
      if (!options) options = {};
      return sendRequest(null, 'unzip-file', { path, dest, override: options.override }, null, options.context, false, options.ref);
    },
    listDownload(options) {
      if (options === true) options = { open: true };
      else if (options === false) options = { open: false };
      else if (typeof options === 'number') options = { max: options };
      else if (!options) options = {};
      return sendRequest(null, 'download-list', { open: options.open, max: options.max }, null, options.context, false, options.ref);
    }
  },
  cryptography: {
    encrypt(alg, value, key, iv, options) {
      if (!options) options = {};
      return sendRequest(null, 'symmetric', { value, alg, key, iv, decrypt: false }, null, options.context, false, options.ref);
    },
    decrypt(alg, value, key, iv, options) {
      if (!options) options = {};
      return sendRequest(null, 'symmetric', { value, alg, key, iv, decrypt: true }, null, options.context, false, options.ref);
    },
    rsaCreate(options) {
      if (!options) options = {};
      return sendRequest(null, 'rsa-create', { pem: options.pem }, null, options.context, false, options.ref);
    },
    rsaEncrypt(value, pem, options) {
      if (!options) options = {};
      return sendRequest(null, 'rsa', { pem: options.pem, padding: options.padding, type: options.type, decrypt: false }, null, options.context, false, options.ref);
    },
    rsaDecrypt(value, pem, options) {
      if (!options) options = {};
      return sendRequest(null, 'rsa', { pem: options.pem, padding: options.padding, type: options.type, decrypt: true }, null, options.context, false, options.ref);
    },
    verify(alg, value, key, test, options) {
      if (!options) options = {};
      return sendRequest(null, 'verify', { value, alg, key, test, type: options.type }, null, options.context, false, options.ref);
    },
    sign(alg, value, key, options) {
      if (!options) options = {};
      return sendRequest(null, 'sign', { value, alg, key, type: options.type }, null, options.context, false, options.ref);
    },
    hash(alg, value, options) {
      if (!options) options = {};
      else if (typeof options === 'string') options = { test: options }
      return sendRequest(null, 'hash', { value, alg, test: options.test, type: options.type }, null, options.context, false, options.ref);
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
  hostApp: {
    theme(options) {
      if (!options) options = {};
      return sendRequest(null, 'theme', {}, null, options.context, false, options.ref);
    },
    checkUpdate(options) {
      if (options === true) options = { check: true };
      else if (options === false) options = { check: false };
      else if (!options) options = {};
      return sendRequest(null, 'check-update', { check: options.check }, null, options.context, false, options.ref);
    },
    window(value) {
      if (!value) value = {};
      else if (typeof value === 'string') value = { state: value };
      return sendRequest(null, 'window', { state: value.state, width: value.width || value.w, height: value.height || value.h, top: value.top || value.y, left: value.left || value.x, focus: value.focus, physical: value.physical }, null, value.context, false, options.ref);
    },
    handlers(options) {
      if (!options) options = {};
      return sendRequest(null, 'handlers', {}, null, options.context, false, options.ref);
    }
  },
  hostInfo: ");
        sb.Append(LocalWebAppExtensions.GetEnvironmentInformation(host.Manifest).ToString(IndentStyles.Compact));
        sb.Append(", dataRes: ");
        sb.Append(host.DataResources.ToString(IndentStyles.Compact));
        sb.Append(", strRes: ");
        var resourceReg = new JsonObjectNode();
        resourceReg.SetRange(host.DataStrings);
        sb.Append(resourceReg.ToString(IndentStyles.Compact));
        sb.AppendLine(" }; })();");
        _ = Browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(sb.ToString());
    }
}
