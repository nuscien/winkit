using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.IO;
using Trivial.Text;
using Trivial.Web;

namespace Trivial.UI;

internal class LocalWebAppExtensions
{
    public static JsonObjectNode GetEnvironmentInformation(LocalWebAppManifest manifest, bool isDebug = false)
    {
        var eas = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
        var hostInfo = new JsonObjectNode
        {
            { "version", Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() },
            { "appName", manifest.Id },
            { "runtime", new JsonObjectNode
            {
                { "kind", "WindowsAppSdk" },
                { "version", "0.1" },
                { "netfx", RuntimeInformation.FrameworkDescription },
                { "id", RuntimeInformation.RuntimeIdentifier },
                { "webview2", CoreWebView2Environment.GetAvailableBrowserVersionString() },
                { "arch", RuntimeInformation.ProcessArchitecture.ToString() },
                { "winkit", typeof(LocalWebAppFileInfo).Assembly.GetName()?.Version?.ToString() }
            } },
            { "os", new JsonObjectNode
            {
                { "value", Environment.OSVersion?.ToString() ?? RuntimeInformation.OSDescription },
                { "arch", RuntimeInformation.OSArchitecture.ToString() },
                { "version", Environment.OSVersion?.Version?.ToString() },
                { "platform", Environment.OSVersion?.Platform.ToString() }
            } },
            { "cmdLine", new JsonObjectNode
            {
                { "value", Environment.CommandLine },
                { "args", Environment.GetCommandLineArgs() },
                { "systemAccount", string.Concat(Environment.UserDomainName ?? Environment.MachineName, '\\', Environment.UserName) },
            } },
            { "mkt", new JsonObjectNode {
                { "value", System.Globalization.CultureInfo.CurrentUICulture?.ToString() ?? System.Globalization.CultureInfo.CurrentCulture?.ToString() },
                { "name", System.Globalization.CultureInfo.CurrentUICulture?.Name?.Trim() ?? System.Globalization.CultureInfo.CurrentCulture?.Name?.Trim() },
                { "rtl", System.Globalization.CultureInfo.CurrentUICulture?.TextInfo?.IsRightToLeft },
                { "timeZone", TimeZoneInfo.Local.Id },
                { "timeZoneDisplayName", TimeZoneInfo.Local.DisplayName ?? TimeZoneInfo.Local.StandardName },
                { "baseOffset", TimeZoneInfo.Local.BaseUtcOffset.ToString() },
            } },
            { "device", new JsonObjectNode
            {
                { "form", Windows.System.Profile.AnalyticsInfo.DeviceForm },
                { "family", Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily },
                { "manufacturer", eas.SystemManufacturer },
                { "productName", eas.SystemProductName },
                { "productSku", eas.SystemSku }
            } }
        };
        if (isDebug) hostInfo.SetValue("devEnv", true);
        return hostInfo;
    }

    public static LocalWebAppResponseMessage ListFiles(LocalWebAppRequestMessage request, LocalWebAppOptions options)
    {
        var path = request?.Data?.TryGetStringValue("path");
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (path.StartsWith('.') || path.StartsWith('~')) path = options.GetLocalPath(path);
        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(path);
        if (dir == null || !dir.Exists) return new("Not found.");
        var resp = new LocalWebAppResponseMessage
        {
            Data = new()
            {
                { "info", ToJson(dir) }
            }
        };
        var q = request.Data.TryGetStringValue("q")?.Trim();
        if (string.IsNullOrEmpty(q)) q = null;
        var dirList = q == null ? dir.EnumerateDirectories() : dir.EnumerateDirectories(q);
        var skipHidden = !(request.Data.TryGetBooleanValue("showHidden") ?? false);
        var data = new JsonArrayNode();
        foreach (var item in dirList)
        {
            var json = ToJson(item, skipHidden);
            if (json != null) data.Add(json);
        }

        resp.Data.SetValue("dirs", data);
        var fileList = q == null ? dir.EnumerateFiles() : dir.EnumerateFiles(q);
        data = new JsonArrayNode();
        foreach (var item in fileList)
        {
            var json = ToJson(item, skipHidden);
            if (json != null) data.Add(json);
        }

        resp.Data.SetValue("files", data);
        try
        {
            var parent = dir.Parent;
            if (parent != null && parent != dir)
                resp.Data.SetValue("parent", ToJson(parent));
        }
        catch (SecurityException)
        {
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (ExternalException)
        {
        }

        return resp;
    }

    public static async Task<LocalWebAppResponseMessage> GetFileAsync(LocalWebAppRequestMessage request, LocalWebAppOptions options)
    {
        var path = request?.Data?.TryGetStringValue("path");
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (path.StartsWith('.') || path.StartsWith('~')) path = options.GetLocalPath(path);
        var file = FileSystemInfoUtility.TryGetFileInfo(path);
        if (file == null && Directory.Exists(path))
        {
            var dir = FileSystemInfoUtility.TryGetDirectoryInfo(path);
            if (dir == null || !dir.Exists) return new("Not found.");
            var resp2 = new LocalWebAppResponseMessage
            {
                Data = new()
                {
                    { "info", ToJson(dir) },
                    { "valueType", "dir" }
                }
            };
            try
            {
                var parent = dir.Parent;
                if (parent != null)
                    resp2.Data.SetValue("parent", ToJson(parent));
            }
            catch (SecurityException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (ExternalException)
            {
            }

            return resp2;
        }

        if (file == null || !file.Exists) return new("Not found.");
        var resp = new LocalWebAppResponseMessage
        {
            Data = new()
            {
                { "info", ToJson(file) }
            }
        };
        try
        {
            var parent = file.Directory;
            if (parent != null)
                resp.Data.SetValue("parent", ToJson(parent));
        }
        catch (SecurityException)
        {
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (ExternalException)
        {
        }

        var readMethod = request.Data.TryGetStringValue("read")?.Trim()?.ToLowerInvariant();
        if (readMethod == "none") return resp;
        try
        {
            if (readMethod == "text" || readMethod == "json" || file.Length < 1_000_000)
            {
                var str = await File.ReadAllTextAsync(path);
                resp.Data.SetValue("value", str);
                resp.Data.SetValue("valueType", "text");
                if (readMethod == "json" || (file.Extension?.ToLowerInvariant() == ".json" && string.IsNullOrEmpty(readMethod)))
                {
                    var valueJson = JsonObjectNode.TryParse(str);
                    if (valueJson != null)
                    {
                        resp.Data.SetValue("value", valueJson);
                        resp.Data.SetValue("valueType", "json");
                    }
                    else if (str.StartsWith('['))
                    {
                        var valueJsonArray = JsonArrayNode.TryParse(str);
                        if (valueJsonArray != null)
                        {
                            resp.Data.SetValue("value", valueJsonArray);
                            resp.Data.SetValue("valueType", "jsonArray");
                        }
                    }
                }
            }
        }
        catch (SecurityException ex)
        {
            resp.Message = ex.Message;
        }
        catch (IOException ex)
        {
            resp.Message = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            resp.Message = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            resp.Message = ex.Message;
        }
        catch (NotSupportedException ex)
        {
            resp.Message = ex.Message;
        }
        catch (ExternalException ex)
        {
            resp.Message = ex.Message;
        }

        return resp;
    }

    private static JsonObjectNode ToJson(FileSystemInfo item, bool skipHidden = false)
    {
        if (item?.Exists != true) return null;
        try
        {
            var json = new JsonObjectNode()
            {
                { "name", item.Name }
            };
            try
            {
                json.SetValue("modified", item.LastWriteTime);
                json.SetValue("created", item.CreationTime);
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (ExternalException)
            {
            }

            try
            {
                var attr = item.Attributes;
                if (skipHidden && attr.HasFlag(FileAttributes.Hidden)) return null;
                json.SetValue("attr", attr.ToString());
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (ExternalException)
            {
            }

            if (item is FileInfo file)
            {
                json.SetValue("type", "file");
                try
                {
                    json.SetValue("length", file.Length);
                }
                catch (SecurityException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (IOException)
                {
                }
                catch (NotSupportedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (ExternalException)
                {
                }
            }
            else if (item is DirectoryInfo)
            {
                json.SetValue("type", "dir");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(item.LinkTarget))
                    json.SetValue("link", item.LinkTarget);
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (ExternalException)
            {
            }

            return json;
        }
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }
}
