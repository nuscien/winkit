using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.IO;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace Trivial.UI;

internal static class LocalWebAppExtensions
{
    internal const string DefaultManifestFileName = "localwebapp.json";
    internal const string DefaultManifestGeneratedFileName = "localwebapp.files.json";
    internal const string VirtualRootDomain = "localwebapp.localhost";

    /// <summary>
    /// Tries to parse a URI.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The URI.</returns>
    public static Uri TryCreateUri(string url)
        => VisualUtility.TryCreateUri(url);

    public static JsonObjectNode GetEnvironmentInformation(LocalWebAppManifest manifest)
    {
        var eas = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
        var entryAssembly = LocalWebAppSettings.GetAssembly()?.GetName();
        var hostInfo = new JsonObjectNode
        {
            { "hostApp", new JsonObjectNode
            {
                { "id", LocalWebAppSettings.HostId },
                { "version", entryAssembly?.Version?.ToString() },
                { "name", entryAssembly?.Name },
                { "value", entryAssembly?.FullName },
                { "additional", LocalWebAppSettings.HostAdditionalString },
                { "info", LocalWebAppSettings.HostAdditionalInfo }
            } },
            { "package", new JsonObjectNode
            {
                { "id", manifest?.Id },
                { "icon", manifest?.Icon },
                { "description", manifest?.Description },
                { "url", manifest?.Website },
                { "copyright", manifest?.Copyright },
                { "publisher", manifest?.PublisherName },
                { "version", manifest?.Version },
                { "tags", manifest?.Tags ?? new() },
                { "meta", manifest?.Metadata ?? new() }
            } },
            { "runtime", new JsonObjectNode
            {
                { "kind", "wasdk" },
                { "version", Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() },
                { "netfx", RuntimeInformation.FrameworkDescription },
                { "id", RuntimeInformation.RuntimeIdentifier },
                { "webview2", CoreWebView2Environment.GetAvailableBrowserVersionString() },
                { "arch", RuntimeInformation.ProcessArchitecture.ToString() }
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
                //{ "value", Environment.CommandLine },
                { "args", Environment.GetCommandLineArgs().Skip(1) },
                { "systemAccount", string.Concat(Environment.UserDomainName ?? Environment.MachineName, '\\', Environment.UserName) },
            } },
            { "mkt", new JsonObjectNode {
                { "value", System.Globalization.CultureInfo.CurrentUICulture?.ToString() ?? System.Globalization.CultureInfo.CurrentCulture?.ToString() },
                { "name", System.Globalization.CultureInfo.CurrentUICulture?.DisplayName ?? System.Globalization.CultureInfo.CurrentCulture?.DisplayName },
                { "rtl", System.Globalization.CultureInfo.CurrentUICulture?.TextInfo?.IsRightToLeft },
                { "timeZone", TimeZoneInfo.Local?.Id },
                { "timeZoneDisplayName", TimeZoneInfo.Local?.DisplayName ?? TimeZoneInfo.Local?.StandardName },
                { "baseOffset", TimeZoneInfo.Local?.BaseUtcOffset.ToString() },
            } },
            { "device", new JsonObjectNode
            {
                { "form", Windows.System.Profile.AnalyticsInfo.DeviceForm }, // https://docs.microsoft.com/zh-cn/windows-hardware/customize/desktop/unattend/microsoft-windows-deployment-deviceform
                { "family", Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily },
                { "manufacturer", eas.SystemManufacturer },
                { "productName", eas.SystemProductName },
                { "productSku", eas.SystemSku }
            } }
        };
        return hostInfo;
    }

    public static async Task<JsonObjectNode> OnWebMessageReceivedAsync(LocalWebAppHost host, JsonObjectNode json, Uri uri, Dictionary<string, ILocalWebAppCommandHandler> handlers, IBasicWindowStateController window, ILocalWebAppBrowserMessageHandler browserHandler)
    {
        if (json == null) return null;
        var req = new LocalWebAppRequestMessage
        {
            Uri = uri,
            IsFullTrusted = !string.IsNullOrWhiteSpace(host?.VirtualHost) && uri?.Host == host.VirtualHost,
            TraceId = json.TryGetStringValue("trace")?.Trim(),
            Command = json.TryGetStringValue("cmd")?.Trim(),
            MessageHandlerId = json.TryGetStringValue("handler")?.Trim(),
            Data = json.TryGetObjectValue("data"),
            AdditionalInfo = json.TryGetObjectValue("info"),
            Context = json.TryGetObjectValue("context")
        };
        LocalWebAppResponseMessage resp;
        try
        {
            if (string.IsNullOrWhiteSpace(host?.Manifest?.Id))
                resp = new LocalWebAppResponseMessage("The app runs failed.");
            else if (string.IsNullOrEmpty(req.Command))
                resp = new LocalWebAppResponseMessage(string.Concat("Command name should not be null or empty."));
            else if (string.IsNullOrEmpty(req.MessageHandlerId))
                resp = await OnLocalWebAppMessageRequestAsync(req, host, handlers, window, browserHandler);
            else if (handlers.TryGetValue(req.MessageHandlerId, out var h) && h != null)
                resp = await h.Process(req, new LocalWebAppCommandHandlerContext(host, window, browserHandler));
            else
                resp = new LocalWebAppResponseMessage(string.Concat("Not supported for this handler. ", req.MessageHandlerId));
        }
        catch (ArgumentException ex)
        {
            resp = HandleException(ex);
        }
        catch (InvalidOperationException ex)
        {
            resp = HandleException(ex);
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            resp = HandleException(ex);
            if (ex.StatusCode.HasValue) resp.AdditionalInfo.SetValue("status", (int)ex.StatusCode.Value);
        }
        catch (JsonException ex)
        {
            resp = HandleException(ex);
        }
        catch (FailedHttpException ex)
        {
            resp = HandleException(ex);
            resp.AdditionalInfo.SetValue("reasonPhrase", ex.ReasonPhrase);
            if (ex.StatusCode.HasValue) resp.AdditionalInfo.SetValue("status", (int)ex.StatusCode.Value);
        }
        catch (FailedChangeException ex)
        {
            resp = HandleException(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            resp = HandleException(ex);
        }
        catch (SecurityException ex)
        {
            resp = HandleException(ex);
        }
        catch (NotSupportedException ex)
        {
            resp = HandleException(ex);
        }
        catch (ApplicationException ex)
        {
            resp = HandleException(ex);
        }
        catch (IOException ex)
        {
            resp = HandleException(ex);
        }
        catch (FormatException ex)
        {
            resp = HandleException(ex);
        }
        catch (CryptographicException ex)
        {
            resp = HandleException(ex);
        }
        catch (InvalidCastException ex)
        {
            resp = HandleException(ex);
        }
        catch (NullReferenceException ex)
        {
            resp = HandleException(ex);
        }
        catch (ArithmeticException ex)
        {
            resp = HandleException(ex);
        }
        catch (NotImplementedException ex)
        {
            resp = HandleException(ex);
        }
        catch (ExternalException ex)
        {
            resp = HandleException(ex);
        }
        catch (AggregateException ex)
        {
            resp = HandleException(ex);
            if (ex.InnerExceptions != null)
            {
                var arr = new JsonArrayNode();
                resp.AdditionalInfo.SetValue("errors", arr);
                foreach (var innerEx in ex.InnerExceptions)
                {
                    if (innerEx == null) continue;
                    var innerExJson = new JsonObjectNode
                    {
                        { "type", innerEx.GetType().Name },
                        { "message", innerEx.Message }
                    };
                    if (!string.IsNullOrWhiteSpace(innerEx.HelpLink))
                        resp.AdditionalInfo.SetValue("url", innerEx.HelpLink);
                    if (innerEx.InnerException != null)
                    {
                        innerExJson.SetValue("innerType", innerEx.InnerException.GetType().Name);
                        innerExJson.SetValue("inner", innerEx.InnerException.Message);
                    }

                    arr.Add(innerExJson);
                }
            }
        }

        return new JsonObjectNode
        {
            { "trace", req.TraceId },
            { "cmd", req.Command },
            { "handler", req.MessageHandlerId },
            { "data", resp?.Data ?? new() },
            { "info", resp?.AdditionalInfo ?? new() },
            { "message", resp?.Message },
            { "error", resp?.IsError ?? true },
            { "context", req.Context },
            { "limited", !req.IsFullTrusted },
            { "timeline", new JsonObjectNode
            {
                { "request", json.TryGetDateTimeValue("date") },
                { "processing", req.ProcessingTime },
                { "processed", DateTime.Now }
            } }
        };
    }

    public static LocalWebAppResponseMessage ListFiles(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        var path = GetPath(request?.Data, "path", host);
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
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

    public static LocalWebAppResponseMessage ListDrives(LocalWebAppRequestMessage request)
    {
        var drives = DriveInfo.GetDrives();
        var fixedOnly = request?.Data?.TryGetBooleanValue("fixed") ?? false;
        if (drives == null) return new(new JsonObjectNode
        {
            { "drives", new JsonArrayNode() }
        }, new JsonObjectNode
        {
            { "fixed", fixedOnly }
        });
        var arr = drives.Select(drive =>
        {
            if (drive == null) return null;
            if (fixedOnly && drive.DriveType != DriveType.Fixed) return null;
            var dir = ToJson(drive.RootDirectory);
            var json = new JsonObjectNode
            {
                { "name", drive.Name },
                { "ready", drive.IsReady },
                { "length", drive.TotalSize },
                { "freespace", new JsonObjectNode
                {
                    { "total", drive.TotalFreeSpace },
                    { "available", drive.AvailableFreeSpace }
                } },
                { "label", drive.VolumeLabel },
                { "format", drive.DriveFormat },
                { "type", drive.DriveType.ToString() },
                { "dir", dir }
            };
            return json;
        }).Where(ele => ele != null);
        return new(new JsonObjectNode
        {
            { "drives", arr }
        }, new JsonObjectNode
        {
            { "fixed", fixedOnly }
        });
    }

    public static async Task<LocalWebAppResponseMessage> GetFileAsync(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        var path = GetPath(request?.Data, "path", host);
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        if (path.StartsWith("https://") || path.StartsWith("http//"))
        {
            var readMethod2 = request.Data.TryGetStringValue("read")?.Trim()?.ToLowerInvariant();
            var readBoolean2 = request.Data.TryGetBooleanValue("read");
            if (string.IsNullOrEmpty(readMethod2) || readMethod2 == "none" || readBoolean2 == false) return new("The path is not valid.");
            using var http = LocalWebAppSettings.CreateHttpClient();
            var postData = request.Data.TryGetObjectValue("post");
            var httpResponse = postData == null ? await http.GetAsync(path) : await http.PostAsync(path, HttpClientExtensions.CreateJsonContent(postData));
            using var httpContent = httpResponse?.Content;
            var httpInfo = new JsonObjectNode()
            {
                { "type", "url" },
                { "path", path }
            };
            var httpResp = new LocalWebAppResponseMessage(new JsonObjectNode()
            {
                { "info", httpInfo }
            }, new()
            {
                { "path", path }
            });
            if (httpContent == null) return httpResp;
            var httpLen = httpContent.Headers?.ContentLength;
            if (httpLen.HasValue) httpInfo.SetValue("length", httpLen.Value);
            httpResp.Data.SetValue("source", path.StartsWith("http//") ? "http" : "https");
            httpResp.Data.SetValue("status", (int)httpResponse.StatusCode);
            if (readMethod2 == "base64")
            {
                var httpBytes = await httpContent.ReadAsByteArrayAsync();
                var str = Convert.ToBase64String(httpBytes);
                httpResp.Data.SetValue("value", str);
                httpResp.Data.SetValue("valueType", "base64");
            }
            else if (readMethod2 == "text")
            {
                var str = await httpContent.ReadAsStringAsync();
                httpResp.Data.SetValue("value", str);
                httpResp.Data.SetValue("valueType", "text");
            }
            else if (readMethod2 == "json")
            {
                var str = await http.GetStringAsync(path);
                var valueJson = JsonObjectNode.TryParse(str);
                if (valueJson != null)
                {
                    httpResp.Data.SetValue("value", valueJson);
                    httpResp.Data.SetValue("valueType", "json");
                }
                else if (str.StartsWith('['))
                {
                    var valueJsonArray = JsonArrayNode.TryParse(str);
                    if (valueJsonArray != null)
                    {
                        httpResp.Data.SetValue("value", valueJsonArray);
                        httpResp.Data.SetValue("valueType", "jsonArray");
                    }
                }
            }
            else if (readBoolean2 == true)
            {
                var str = await http.GetStringAsync(path);
                if (str.StartsWith("{"))
                {
                    var valueJson = JsonObjectNode.TryParse(str);
                    if (valueJson != null)
                    {
                        httpResp.Data.SetValue("value", valueJson);
                        httpResp.Data.SetValue("valueType", "json");
                    }
                }
                else if (str.StartsWith("["))
                {
                    var valueJsonArray = JsonArrayNode.TryParse(str);
                    if (valueJsonArray != null)
                    {
                        httpResp.Data.SetValue("value", valueJsonArray);
                        httpResp.Data.SetValue("valueType", "jsonArray");
                    }
                }
                
                if (!httpResp.Data.ContainsKey("valueType"))
                {
                    httpResp.Data.SetValue("value", str);
                    httpResp.Data.SetValue("valueType", "text");
                }
            }

            return httpResp;
        }

        var file = FileSystemInfoUtility.TryGetFileInfo(path);
        if (file == null && Directory.Exists(path))
        {
            var dir = FileSystemInfoUtility.TryGetDirectoryInfo(path);
            if (dir == null || !dir.Exists) return new("Not found.");
            var resp2 = new LocalWebAppResponseMessage(new JsonObjectNode()
                {
                    { "source", "dir" },
                    { "info", ToJson(dir) },
                    { "valueType", "dir" }
                }, new()
                {
                    { "path", path }
                });
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
        var resp = new LocalWebAppResponseMessage(new JsonObjectNode()
            {
                { "source", "file" },
                { "info", ToJson(file) }
            }, new()
            {
                { "path", path }
            });
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
        var readBoolean = request.Data.TryGetBooleanValue("read");
        if (readMethod == "none" || readBoolean == false) return resp;
        var maxLen = request.Data.TryGetInt64Value("maxLength");
        try
        {
            if (maxLen.HasValue && file.Length > maxLen.Value) return resp;
        }
        catch (IOException)
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

        try
        {
            if (readMethod == "base64")
            {
                var str = Convert.ToBase64String(File.ReadAllBytes(path)); ;
                resp.Data.SetValue("value", str);
                resp.Data.SetValue("valueType", "base64");
            }
            else if (readMethod == "text" || readMethod == "json" || readBoolean == true || file.Length < 1_000_000)
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

    public static async Task<LocalWebAppResponseMessage> WriteFileAsync(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        var path = GetPath(request?.Data, "path", host);
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        var info = new JsonObjectNode
        {
            { "path", path }
        };
        var json = request.Data.TryGetObjectValue("value");
        if (json != null)
        {
            await File.WriteAllTextAsync(path, json.ToString(IndentStyles.Compact));
            info.SetValue("inputType", "json");
            return new(new JsonObjectNode(), info);
        }

        var array = request.Data.TryGetArrayValue("value");
        if (array != null)
        {
            await File.WriteAllTextAsync(path, array.ToString(IndentStyles.Compact));
            info.SetValue("inputType", "json-array");
            return new(new JsonObjectNode(), info);
        }

        var s = request.Data.TryGetStringValue("value");
        info.SetValue("inputType", "text");
        await File.WriteAllTextAsync(path, s);
        return new(new JsonObjectNode(), info);
    }

    public static LocalWebAppResponseMessage CreateDirectory(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        var path = GetPath(request?.Data, "path", host) ?? GetPath(request?.Data, "source", host);
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        var info = new JsonObjectNode
        {
            { "path", path }
        };
        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(path);
        if (dir?.Exists == true) return new(new JsonObjectNode
        {
            { "existed", true },
            { "info", ToJson(dir) }
        }, info);
        dir = Directory.CreateDirectory(path);
        return new(new JsonObjectNode
        {
            { "existed", false },
            { "info", ToJson(dir) }
        }, info);
    }

    public static async Task<LocalWebAppResponseMessage> MoveFileAsync(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        var path = GetPath(request?.Data, "path", host) ?? GetPath(request?.Data, "source", host);
        if (string.IsNullOrWhiteSpace(path)) return new("The source path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        var dest = GetPath(request?.Data, "dest", host) ?? GetPath(request?.Data, "destination", host);
        var isToCopy = request.Data.TryGetBooleanValue("copy") ?? false;
        var isDir = request.Data.TryGetBooleanValue("dir") ?? false;
        if (path.StartsWith("https://") || path.StartsWith("http://"))
        {
            if (isDir) return new("Expect a directory but the path is not.");
            if (string.IsNullOrWhiteSpace(dest)) return new("The destination path should not be null or empty.");
            if (request.Data.TryGetBooleanValue("override") != true && File.Exists(dest)) return new("The destination file already exist.");
            using var http = LocalWebAppSettings.CreateHttpClient();
            using var httpContent = (await http.GetAsync(path))?.Content;
            await httpContent.WriteFileAsync(dest);
            return new(new JsonObjectNode(), new JsonObjectNode
            {
                { "action", "download" },
                { "sourceType", "url" },
                { "source", path },
                { "dest", dest }
            });
        }

        if (string.IsNullOrWhiteSpace(path)) return new("The source path is not valid.");
        if (dest == null)
        {
            if (isToCopy) return new(new JsonObjectNode(), new JsonObjectNode
            {
                { "action", "none" },
                { "sourceType", isDir ? "dir" : "file" },
                { "source", path },
                { "dest", dest }
            });
            if (isDir)
                Directory.Delete(path, true);
            else
                File.Delete(path);
            return new(new JsonObjectNode(), new JsonObjectNode
            {
                { "action", "delete" },
                { "sourceType", isDir ? "dir" : "file" },
                { "source", path },
                { "dest", dest }
            });
        }

        if (string.IsNullOrWhiteSpace(dest)) return new("The destination path should not be null or empty.");
        if (path == dest) return new(new JsonObjectNode(), new JsonObjectNode
        {
            { "action", "none" },
            { "sourceType", isDir ? "dir" : "file" },
            { "source", path },
            { "dest", dest }
        });
        var info = new JsonObjectNode
        {
            { "action", isToCopy ? "copy" : "move" },
            { "sourceType", isDir ? "dir" : "file" },
            { "source", path },
            { "dest", dest }
        };
        if (isDir)
        {
            if (!Directory.Exists(path)) return new("The source directory does not exist.")
            {
                AdditionalInfo = info
            };
            if (isToCopy)
                Directory.Move(path, dest);
            else
                FileSystemInfoUtility.CopyTo(FileSystemInfoUtility.TryGetDirectoryInfo(path), dest);
        }
        else
        {
            if (!File.Exists(path)) return new("The source file does not exist.")
            {
                AdditionalInfo = info
            };
            if (isToCopy)
                File.Move(path, dest, request.Data.TryGetBooleanValue("override") ?? false);
            else
                File.Copy(path, dest, request.Data.TryGetBooleanValue("override") ?? false);
        }

        return new(new JsonObjectNode(), info);
    }

    public static LocalWebAppResponseMessage CompressFiles(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        var path = GetPath(request?.Data, "path", host) ?? GetPath(request?.Data, "source", host);
        var dest = GetPath(request?.Data, "dest", host) ?? GetPath(request?.Data, "destination", host);
        if (string.IsNullOrWhiteSpace(path)) return new("The source path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        if (string.IsNullOrWhiteSpace(dest)) return new("The destination path should not be null or empty.");
        if (!Directory.Exists(path)) return new("The source compressed directory does not exist.");
        if (request.Data.TryGetBooleanValue("override") == true && File.Exists(path)) File.Delete(path);
        var includeBaseFolder = request.Data.TryGetBooleanValue("folder") ?? false;
        ZipFile.CreateFromDirectory(path, dest, CompressionLevel.Optimal, includeBaseFolder);
        return new(new JsonObjectNode(), new JsonObjectNode
        {
            { "algorithm", "zip" },
            { "source", path },
            { "dest", dest },
            { "folder", includeBaseFolder }
        });
    }

    public static LocalWebAppResponseMessage ExtractFiles(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        var path = GetPath(request?.Data, "path", host) ?? GetPath(request?.Data, "source", host);
        var dest = GetPath(request?.Data, "dest", host) ?? GetPath(request?.Data, "destination", host);
        if (string.IsNullOrWhiteSpace(path)) return new("The source path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        if (string.IsNullOrWhiteSpace(dest)) return new("The destination path should not be null or empty.");
        if (!File.Exists(path)) return new("The source compressed file does not exist.");
        Directory.CreateDirectory(dest);
        ZipFile.ExtractToDirectory(path, dest, request.Data.TryGetBooleanValue("override") ?? false);
        return new(new JsonObjectNode(), new JsonObjectNode
        {
            { "algorithm", "zip" },
            { "source", path },
            { "dest", dest }
        });
    }

    public static LocalWebAppResponseMessage Hash(LocalWebAppRequestMessage request, LocalWebAppHost host = null)
    {
        var s = request?.Data?.TryGetStringValue("value");
        var alg = request.Data.TryGetStringValue("alg")?.Trim()?.ToUpperInvariant()?.Replace("-", string.Empty);
        if (s == null) return new(new JsonObjectNode()
        {
            { "value", s }
        });
        var input = s;
        var type = request.Data.TryGetStringValue("type")?.Trim()?.ToLowerInvariant();
        if (type == "file")
        {
            if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
            if (host != null) s = host.GetLocalPath(s, true);
            var file = FileSystemInfoUtility.TryGetFileInfo(s);
            if (file == null || !file.Exists) return new("Not found the file.");
            s = alg switch
            {
                "SHA1" => TryHashFile(file, SHA1.Create()),
                "SHA256" => TryHashFile(file, SHA256.Create()),
                "SHA384" => TryHashFile(file, SHA384.Create()),
                "SHA512" => TryHashFile(file, SHA512.Create()),
#if NET6_0
                "KECCAK224" => TryHashFile(file, HashUtility.Create(new HashAlgorithmName("KECCAK224"))),
#endif
                "KECCAK256" => TryHashFile(file, HashUtility.ComputeSHA3256String),
                "KECCAK384" => TryHashFile(file, HashUtility.ComputeSHA3384String),
                "KECCAK512" => TryHashFile(file, HashUtility.ComputeSHA3512String),
                "MD5" => TryHashFile(file, MD5.Create()),
                _ => null
            };
        }
        else if (type == "base64")
        {
            var bytes = Convert.FromBase64String(s);
            s = alg switch
            {
                "SHA1" => HashUtility.ComputeHashString(SHA1.Create, bytes),
                "SHA256" => HashUtility.ComputeHashString(SHA256.Create, bytes),
                "SHA384" => HashUtility.ComputeHashString(SHA384.Create, bytes),
                "SHA512" => HashUtility.ComputeHashString(SHA512.Create, bytes),
#if NET6_0
                "KECCAK224" => HashUtility.ComputeHashString(HashUtility.Create(new HashAlgorithmName("KECCAK224")), bytes),
#endif
                "KECCAK256" => HashUtility.ComputeSHA3256String(bytes),
                "KECCAK384" => HashUtility.ComputeSHA3384String(bytes),
                "KECCAK512" => HashUtility.ComputeSHA3512String(bytes),
                "MD5" => HashUtility.ComputeHashString(MD5.Create, bytes),
                _ => null
            };
        }
        else
        {
            type = null;
            s = alg switch
            {
                "SHA1" => HashUtility.ComputeHashString(SHA1.Create, s),
                "SHA256" => HashUtility.ComputeHashString(SHA256.Create, s),
                "SHA384" => HashUtility.ComputeHashString(SHA384.Create, s),
                "SHA512" => HashUtility.ComputeHashString(SHA512.Create, s),
#if NET6_0
                "KECCAK224" => HashUtility.ComputeHashString(HashUtility.Create(new HashAlgorithmName("KECCAK224")), s),
#endif
                "KECCAK256" => HashUtility.ComputeSHA3256String(s),
                "KECCAK384" => HashUtility.ComputeSHA3384String(s),
                "KECCAK512" => HashUtility.ComputeSHA3512String(s),
                "MD5" => HashUtility.ComputeHashString(MD5.Create, s),
                _ => null
            };
        }

        if (s == null) return new("The algorithm is not supported.");
        var json = new JsonObjectNode()
        {
            { "value", s },
            { "algorithm", alg },
            { "encode", "hex" }
        };
        var test = request.Data.TryGetStringValue("test");
        if (!string.IsNullOrWhiteSpace(test)) json.SetValue("verify", test.Equals(s, StringComparison.OrdinalIgnoreCase));
        var info = new JsonObjectNode
        {
            { "input", input },
            { "inputType", string.IsNullOrEmpty(type) ? "text" : type }
        };
        return new(json, info);
    }

    public static LocalWebAppResponseMessage Symmetric(LocalWebAppRequestMessage request)
    {
        var s = request?.Data?.TryGetStringValue("value");
        if (s == null) return new(new JsonObjectNode()
        {
            { "value", s }
        });
        var alg = request.Data.TryGetStringValue("alg")?.Trim()?.ToUpperInvariant();
        if (string.IsNullOrEmpty(alg)) return new("The algorithm should not be null or empty.");
        var isDecrypt = request.Data.TryGetBooleanValue("decrypt") ?? false;
        s = (isDecrypt) ? alg switch
        {
            "AES" => SymmetricUtility.DecryptText(Aes.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            "3DES" => SymmetricUtility.DecryptText(TripleDES.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            "RC2" => SymmetricUtility.DecryptText(RC2.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            "DES" => SymmetricUtility.DecryptText(DES.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            _ => null
        } : alg switch
        {
            "AES" => SymmetricUtility.Encrypt(Aes.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            "3DES" => SymmetricUtility.Encrypt(TripleDES.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            "RC2" => SymmetricUtility.Encrypt(RC2.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            "DES" => SymmetricUtility.Encrypt(DES.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            _ => null
        };

        if (s == null) return new("The algorithm is not supported.");
        return new(new JsonObjectNode()
        {
            { "value", s },
            { "algorithm", alg },
            { "encode", "base64" }
        }, new JsonObjectNode
        {
            { "way", isDecrypt ? "decrypt" : "encrypt" }
        });
    }

    public static LocalWebAppResponseMessage Signature(LocalWebAppRequestMessage request, LocalWebAppHost host = null, bool verify = false)
    {
        var s = request?.Data?.TryGetStringValue("value");
        if (s == null) return new("Miss input value.");
        var alg = request.Data.TryGetStringValue("alg")?.Trim()?.ToUpperInvariant();
        if (string.IsNullOrEmpty(alg)) return new("The algorithm should not be null or empty.");
        var secret = request.Data.TryGetStringValue("key");
        if (string.IsNullOrWhiteSpace(secret)) return new("Miss key.");
        ISignatureProvider sign = alg switch
        {
            "RS256" => RSASignatureProvider.CreateRS256(secret),
            "RS384" => RSASignatureProvider.CreateRS384(secret),
            "RS512" => RSASignatureProvider.CreateRS512(secret),
            "HS256" => HashSignatureProvider.CreateHS256(secret),
            "HS384" => HashSignatureProvider.CreateHS384(secret),
            "HS512" => HashSignatureProvider.CreateHS512(secret),
            _ => null
        };
        if (sign == null) return new("The algorithm is not supported.");
        var test = verify ? request.Data.TryGetStringValue("test") : null;
        var type = request.Data.TryGetStringValue("type")?.Trim()?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(test))
        {
            if (verify) return new("Miss test signature to verify.");
            if (type == "file")
            {
                if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
                if (host != null) s = host.GetLocalPath(s, true);
                var file = FileSystemInfoUtility.TryGetFileInfo(s);
                if (file == null || !file.Exists) return new("Not found.");
                using var stream = file.OpenRead();
                test = WebFormat.Base64UrlEncode(sign.Sign(stream));
            }
            else if (type == "base64")
            {
                test = WebFormat.Base64UrlEncode(sign.Sign(Convert.FromBase64String(s)));
            }
            else
            {
                test = WebFormat.Base64UrlEncode(sign.Sign(s));
            }

            return new(new JsonObjectNode
            {
                { "sign", test },
                { "algorithm", alg },
                { "encode", "base64url" }
            }, new JsonObjectNode
            {
                { "inputType", type }
            });
        }
        else
        {
            var bytes = WebFormat.Base64UrlDecode(test);
            var result = false;
            if (type == "file")
            {
                if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
                if (host != null) s = host.GetLocalPath(s, true);
                var file = FileSystemInfoUtility.TryGetFileInfo(s);
                if (file == null || !file.Exists) return new("Not found.");
                using var stream = file.OpenRead();
                result = sign.Verify(stream, bytes);
            }
            else
            {
                var data = type == "base64" ? Convert.FromBase64String(s) : Encoding.UTF8.GetBytes(s);
                result = sign.Verify(data, bytes);
            }

            return new(new JsonObjectNode
            {
                { "verify", result },
                { "algorithm", alg }
            }, new JsonObjectNode
            {
                { "inputType", type }
            });
        }
    }

    public static LocalWebAppResponseMessage CreateRsa(LocalWebAppRequestMessage request, LocalWebAppHost host = null, bool verify = false)
    {
        var pem = request?.Data?.TryGetStringValue("pem");
        using var rsa = CreateRsa(pem);
        if (rsa == null) return new("The input PEM is not valid.");
        RSAParameters secret;
        try
        {
            secret = rsa.ExportParameters(true);
        }
        catch (CryptographicException)
        {
            secret = rsa.ExportParameters(false);
        }

        return new(new JsonObjectNode
        {
            { "publicPem", secret.ToPublicPEMString() },
            { "privatePem", secret.ToPrivatePEMString(true) },
            { "publicParams", secret.ToXElement(false).ToString() },
            { "privateParams", secret.ToXElement(true).ToString() },
            { "modulus", Convert.ToBase64String(secret.Modulus) },
            { "exponent", Convert.ToBase64String(secret.Exponent) },
            { "includePrivate", secret.D == null || secret.D.Length == 0 }
        }, new JsonObjectNode
        {
            { "import", pem != null }
        });
    }

    public static LocalWebAppResponseMessage RsaEncrypt(LocalWebAppRequestMessage request, LocalWebAppHost host = null, bool verify = false)
    {
        var s = request?.Data?.TryGetStringValue("value");
        var pem = request?.Data?.TryGetStringValue("pem");
        if (string.IsNullOrWhiteSpace(pem)) return new("The pem should not be null or empty.");
        var parameters = RSAParametersConvert.Parse(pem);
        if (parameters == null) return new("The pem is not valid.");
        var rsa = RSA.Create(parameters.Value);
        var padding = (request.Data.TryGetStringTrimmedValue("padding", true)?.ToLowerInvariant() ?? string.Empty) switch
        {
            "sha1" or "oaepsha1" => RSAEncryptionPadding.OaepSHA1,
            "sha256" or "oaepsha256" => RSAEncryptionPadding.OaepSHA256,
            "sha384" or "oaepsha384" => RSAEncryptionPadding.OaepSHA384,
            "sha512" or "oaepsha512" => RSAEncryptionPadding.OaepSHA512,
            _ => RSAEncryptionPadding.Pkcs1
        };
        var type = request.Data.TryGetStringTrimmedValue("type", true);
        if (request.Data.TryGetBooleanValue("decrypt") ?? false)
        {
            if (type == "base64")
            {
                var bytes = rsa.Decrypt(Convert.FromBase64String(s), padding);
                s = Convert.ToBase64String(bytes);
            }
            else
            {
                type = null;
                s = rsa.DecryptText(s, padding);
            }

            return new(new JsonObjectNode()
            {
                { "value", s },
                { "encode", "base64" }
            }, new JsonObjectNode
            {
                { "way", "decrypt" },
                { "padding", padding.ToString() },
                { "inputType", type }
            });
        }
        else
        {
            var bytes = type == "base64"
                ? rsa.Encrypt(Convert.FromBase64String(s), padding)
                : rsa.Encrypt(s, padding);
            s = Convert.ToBase64String(bytes);
            return new(new JsonObjectNode()
            {
                { "value", s },
                { "encode", "base64" }
            }, new JsonObjectNode
            {
                { "way", "encrypt" },
                { "padding", padding.ToString() },
                { "inputType", type }
            });
        }
    }

    public static async Task<LocalWebAppResponseMessage> OpenFileAsync(LocalWebAppRequestMessage request, LocalWebAppHost host = null)
    {
        var path = GetPath(request?.Data, "path", host);
        if (string.IsNullOrEmpty(path)) return new("The path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        var type = request.Data.TryGetStringValue("type")?.Trim()?.ToLowerInvariant();
        var resp = new LocalWebAppResponseMessage(new JsonObjectNode(), new JsonObjectNode
        {
            { "path", path },
        });
        var args = request.Data.TryGetStringValue("args")?.Trim();
        if (string.IsNullOrEmpty(args)) args = null;
        if (string.IsNullOrEmpty(type))
        {
            if (path.Contains("://")) type = "url";
            else if (args != null) type = "exe";
            else type = "file";
        }

        try
        {
            switch (type)
            {
                case "file":
                    {
                        var fileStorage = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
                        if (fileStorage == null) return new("Not found.");
                        resp.AdditionalInfo.SetValue("type", "file");
                        resp.IsError = !await Windows.System.Launcher.LaunchFileAsync(fileStorage);
                        return resp;
                    }
                case "dir":
                case "folder":
                    {
                        resp.AdditionalInfo.SetValue("type", "dir");
                        resp.IsError = !await Windows.System.Launcher.LaunchFolderPathAsync(path);
                        return resp;
                    }
                case "http":
                case "https":
                case "web":
                case "url":
                case "uri":
                    {
                        resp.AdditionalInfo.SetValue("type", "url");
                        resp.IsError = !await Windows.System.Launcher.LaunchUriAsync(VisualUtility.TryCreateUri(path));
                        return resp;
                    }
                case "exe":
                    {
                        resp.AdditionalInfo.SetValue("type", "exe");
                        if (args != null) resp.AdditionalInfo.SetValue("args", args);
                        using var proc = args == null ? Process.Start(path) : Process.Start(path, args);
                        resp.IsError = proc == null;
                        return resp;
                    }
                default:
                    resp.Message = "Unsupported type.";
                    break;
            }
        }
        catch (IOException ex)
        {
            resp.Message = ex.Message;
        }
        catch (NotSupportedException ex)
        {
            resp.Message = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            resp.Message = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            resp.Message = ex.Message;
        }
        catch (SecurityException ex)
        {
            resp.Message = ex.Message;
        }
        catch (ExternalException ex)
        {
            resp.Message = ex.Message;
        }

        resp.IsError = true;
        return resp;
    }

    public static async Task<LocalWebAppResponseMessage> CheckUpdateAsync(LocalWebAppRequestMessage request, LocalWebAppHost host = null)
    {
        var info = new JsonObjectNode();
        if (request?.Data != null)
        {
            var needCheck = request.Data.TryGetBooleanValue("check") == true;
            var url = request.Data.TryGetStringValue("check");
            if (url != null && url.ToLowerInvariant().StartsWith("https://"))
            {
                var uri = TryCreateUri(url);
                await host.UpdateAsync(uri);
                info.SetValue("check", true);
                info.SetValue("url", url);
            }
            else if (needCheck)
            {
                await host.UpdateAsync();
                info.SetValue("check", true);
            }
        }

        return new(new JsonObjectNode()
        {
            { "version", host.NewVersionAvailable ?? host.Manifest?.Version },
            { "has", !string.IsNullOrWhiteSpace(host.NewVersionAvailable) }
        }, info);
    }

    public static LocalWebAppResponseMessage WindowState(LocalWebAppRequestMessage request, IBasicWindowStateController window, ILocalWebAppBrowserMessageHandler browserHandler)
    {
        var info = new JsonObjectNode();
        var physical = request?.Data?.TryGetBooleanValue("physical") ?? false;
        if (request?.Data != null)
        {
            var w = request.Data.TryGetInt32Value("width") ?? request.Data.TryGetInt32Value("w");
            var h = request.Data.TryGetInt32Value("height") ?? request.Data.TryGetInt32Value("h");
            var x = request.Data.TryGetInt32Value("left") ?? request.Data.TryGetInt32Value("x");
            var y = request.Data.TryGetInt32Value("top") ?? request.Data.TryGetInt32Value("y");
            if (w.HasValue)
            {
                window.Size(w.Value, h ?? window.Size(physical).Y, physical);
                info.SetValue("resize", true);
            }
            else if (h.HasValue)
            {
                window.Size(w ?? window.Size(physical).X, h.Value, physical);
                info.SetValue("resize", true);
            }

            if (x.HasValue)
            {
                window.Position(x.Value, y ?? window.Position(physical).Y, physical);
                info.SetValue("move", true);
            }
            else if (y.HasValue)
            {
                window.Position(x ?? window.Position(physical).X, y.Value, physical);
                info.SetValue("move", true);
            }

            var state = request.Data.TryGetStringValue("state")?.Trim()?.ToLowerInvariant()?.Replace(" ", "") ?? string.Empty;
            info.SetValue("state", state);
            switch (state)
            {
                case "maximize":
                    window.Maximize();
                    break;
                case "minimize":
                    window.Minimize();
                    break;
                case "restore":
                    window.Restore();
                    break;
                case "fullscreen":
                case "enterfullscreen":
                    window.SetFullScreen(true);
                    break;
                case "exitfullscreen":
                    window.SetFullScreen(false);
                    break;
                default:
                    info.Remove("state");
                    break;
            }

            if (request.Data.TryGetBooleanValue("focus") == true)
            {
                browserHandler.Focus();
                info.SetValue("focus", true);
            }
        }

        var size = window.Size(physical);
        var position = window.Position(physical);
        return new(new JsonObjectNode()
        {
            { "width", size.X },
            { "height", size.Y },
            { "top", position.Y },
            { "left", position.X },
            { "state", window.WindowState().ToString() },
            { "title", window.Title },
            { "physical", physical }
        }, info);
    }

    public static LocalWebAppResponseMessage GetMessageHandlers(Dictionary<string, ILocalWebAppCommandHandler> handlers)
    {
        var arr = new JsonArrayNode();
        foreach (var handler in handlers)
        {
            if (handler.Value == null || string.IsNullOrWhiteSpace(handler.Key)) continue;
            arr.Add(new JsonObjectNode
            {
                { "id", handler.Key },
                { "description", handler.Value.Description },
                { "version", handler.Value.Version }
            });
        }

        return new(new JsonObjectNode()
        {
            { "collection", arr }
        });
    }

    public static string GetPath(IJsonValueNode node, LocalWebAppHost host)
    {
        if (node is IJsonValueNode<string> s)
        {
            if (host != null) return host.GetLocalPath(s.Value, true);
            return s.Value.StartsWith("%") ? FileSystemInfoUtility.GetLocalPath(s.Value) : s.Value;
        }

        if (node is not JsonObjectNode json) return null;
        var str = json.TryGetStringValue("path");
        var kind = json.TryGetStringTrimmedValue("parent", true)?.ToLowerInvariant();
        if (kind == null || kind == "absolute")
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            return str.StartsWith("%") ? FileSystemInfoUtility.GetLocalPath(str) : str;
        }

        return kind switch
        {
            "asset" => host?.GetLocalPath(string.Concat(".asset:\\", str), true),
            "parent" => host?.GetLocalPath(string.Concat("..\\", str), true),
            "data" or "appdata" => host?.GetLocalPath(string.Concat(".data:\\", str), true),
            "doc" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), str),
            _ => null,
        };
    }

    public static string GetPath(JsonObjectNode node, string key, LocalWebAppHost host)
        => GetPath(node?.TryGetValue(key), host);

    private static RSA CreateRsa(string pem)
    {
        if (string.IsNullOrWhiteSpace(pem)) return RSA.Create();
        var parameters = RSAParametersConvert.Parse(pem);
        return parameters.HasValue ? RSA.Create(parameters.Value) : null;
    }

    private static async Task<LocalWebAppResponseMessage> OnLocalWebAppMessageRequestAsync(LocalWebAppRequestMessage request, LocalWebAppHost host, Dictionary<string, ILocalWebAppCommandHandler> handlers, IBasicWindowStateController window, ILocalWebAppBrowserMessageHandler browserHandler)
    {
        if (string.IsNullOrEmpty(request?.Command)) return null;
        switch (request.Command.ToLowerInvariant())
        {
            case "list-file":
                return ListFiles(request, host);
            case "list-drives":
                return ListDrives(request);
            case "get-file":
                return await GetFileAsync(request, host);
            case "write-file":
                return await WriteFileAsync(request, host);
            case "move-file":
                return await MoveFileAsync(request, host);
            case "make-dir":
                return CreateDirectory(request, host);
            case "zip-file":
                return CompressFiles(request, host);
            case "unzip-file":
                return ExtractFiles(request, host);
            case "hash":
                return Hash(request, host);
            case "symmetric":
                return Symmetric(request);
            case "rsa-create":
                return CreateRsa(request, host);
            case "rsa":
                return RsaEncrypt(request, host);
            case "sign":
                return Signature(request, host, false);
            case "verify":
                return Signature(request, host, true);
            case "open":
                return await OpenFileAsync(request, host);
            case "download-list":
                {
                    var maxCount = request.Data?.TryGetInt32Value("max");
                    var open = request.Data?.TryGetBooleanValue("open");
                    return new(browserHandler?.DownloadListInfo(open, maxCount ?? 512)?.ToJson(), new()
                    {
                        { "open", open },
                        { "max", maxCount }
                    });
                }
            case "theme":
                {
                    var theme = browserHandler?.GetTheme()?.ToJson();
                    return theme != null ? new(theme) : new("Failed to load theme information.");
                }
            case "check-update":
                return await CheckUpdateAsync(request, host);
            case "window":
                if (window != null) return WindowState(request, window, browserHandler);
                break;
            case "handlers":
                return GetMessageHandlers(handlers);
        }

        return new LocalWebAppResponseMessage(string.Concat("Not supported for this command. ", request.Command));
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

            try
            {
                json.SetValue("path", item.FullName);
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

    private static LocalWebAppResponseMessage HandleException(Exception ex)
    {
        var resp = new LocalWebAppResponseMessage(ex.Message)
        {
            AdditionalInfo = new()
            {
                { "type", ex.GetType().Name },
                { "unhandled", true },
            }
        };
        if (!string.IsNullOrWhiteSpace(ex.HelpLink))
            resp.AdditionalInfo.SetValue("url", ex.HelpLink);
        if (ex.InnerException == null) return resp;
        resp.AdditionalInfo.SetValue("innerType", ex.InnerException.GetType().Name);
        resp.AdditionalInfo.SetValue("inner", ex.InnerException.Message);
        return resp;
    }

    /// <summary>
    /// Gets the hash value of a file.
    /// </summary>
    /// <param name="file">The file to hash.</param>
    /// <param name="hash">The hash algorithm.</param>
    /// <returns>The hash value.</returns>
    private static string TryHashFile(FileInfo file, HashAlgorithm hash)
    {
        try
        {
            return HashUtility.ComputeHashString(hash, file);
        }
        catch (ArgumentException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (IOException)
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

    /// <summary>
    /// Gets the hash value of a file.
    /// </summary>
    /// <param name="file">The file to hash.</param>
    /// <param name="hash">The hash algorithm.</param>
    /// <returns>The hash value.</returns>
    private static string TryHashFile(FileInfo file, Func<FileInfo, string> hash)
    {
        try
        {
            return hash(file);
        }
        catch (ArgumentException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (IOException)
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
