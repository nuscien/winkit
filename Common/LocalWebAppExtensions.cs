﻿using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public static JsonObjectNode GetEnvironmentInformation(LocalWebAppManifest manifest, bool isDebug = false)
    {
        var eas = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
        var hostInfo = new JsonObjectNode
        {
            { "version", Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() },
            { "appName", manifest?.Id },
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
                { "args", Environment.GetCommandLineArgs().Skip(1) },
                { "systemAccount", string.Concat(Environment.UserDomainName ?? Environment.MachineName, '\\', Environment.UserName) },
            } },
            { "mkt", new JsonObjectNode {
                { "value", System.Globalization.CultureInfo.CurrentUICulture?.ToString() ?? System.Globalization.CultureInfo.CurrentCulture?.ToString() },
                { "name", System.Globalization.CultureInfo.CurrentUICulture?.DisplayName ?? System.Globalization.CultureInfo.CurrentCulture?.DisplayName },
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

    public static async Task<JsonObjectNode> OnWebMessageReceivedAsync(LocalWebAppHost host, JsonObjectNode json, Uri uri, Dictionary<string, LocalWebAppMessageProcessAsync> handlers)
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
        LocalWebAppResponseMessage resp = null;
        try
        {
            if (string.IsNullOrWhiteSpace(host?.Manifest?.Id))
                resp = new LocalWebAppResponseMessage("The app runs failed.");
            else if (string.IsNullOrEmpty(req.Command))
                resp = new LocalWebAppResponseMessage(string.Concat("Command name should not be null or empty."));
            else if (string.IsNullOrEmpty(req.MessageHandlerId))
                resp = await OnLocalWebAppMessageRequestAsync(req, host);
            else if (handlers.TryGetValue(req.MessageHandlerId, out var h) && h != null)
                resp = await h(req);
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
        catch (InvalidCastException ex)
        {
            resp = HandleException(ex);
        }
        catch (NullReferenceException ex)
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
            { "data", resp?.Data },
            { "info", resp?.AdditionalInfo },
            { "message", resp?.Message },
            { "error", resp?.IsError ?? true },
            { "context", req.Context },
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
        var path = request?.Data?.TryGetStringValue("path");
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        if (host != null) path = host.GetLocalPath(path, true);
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

    public static async Task<LocalWebAppResponseMessage> GetFileAsync(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        var path = request?.Data?.TryGetStringValue("path");
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        if (host != null) path = host.GetLocalPath(path, true);
        var file = FileSystemInfoUtility.TryGetFileInfo(path);
        if (file == null && Directory.Exists(path))
        {
            var dir = FileSystemInfoUtility.TryGetDirectoryInfo(path);
            if (dir == null || !dir.Exists) return new("Not found.");
            var resp2 = new LocalWebAppResponseMessage(new JsonObjectNode()
                {
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
        try
        {
            if (readMethod == "text" || readMethod == "json" || readBoolean == true || file.Length < 1_000_000)
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
        var path = request?.Data?.TryGetStringValue("path");
        if (string.IsNullOrWhiteSpace(path)) return new("The path should not be null or empty.");
        if (!request.IsFullTrusted) return new("No permission. The domain is not trusted.");
        if (host != null) path = host.GetLocalPath(path, true);
        var info = new JsonObjectNode
        {
            { "path", path }
        };
        if (!File.Exists(path)) return new("Not found.")
        {
            AdditionalInfo = info
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
            if (file == null || !file.Exists) return new("Not found.");
            s = alg switch
            {
                "SHA1" => TryHashFile(file, SHA1.Create()),
                "SHA256" => TryHashFile(file, SHA256.Create()),
                "SHA384" => TryHashFile(file, SHA384.Create()),
                "SHA512" => TryHashFile(file, SHA512.Create()),
                "KECCAK224" => TryHashFile(file, HashUtility.Create(new HashAlgorithmName("KECCAK224"))),
                "KECCAK256" => TryHashFile(file, HashUtility.Create(new HashAlgorithmName("KECCAK256"))),
                "KECCAK384" => TryHashFile(file, HashUtility.Create(new HashAlgorithmName("KECCAK384"))),
                "KECCAK512" => TryHashFile(file, HashUtility.Create(new HashAlgorithmName("KECCAK256"))),
                "MD5" => TryHashFile(file, MD5.Create()),
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
                "KECCAK224" => HashUtility.ComputeHashString(HashUtility.Create(new HashAlgorithmName("KECCAK224")), s),
                "KECCAK256" => HashUtility.ComputeHashString(HashUtility.Create(new HashAlgorithmName("KECCAK256")), s),
                "KECCAK384" => HashUtility.ComputeHashString(HashUtility.Create(new HashAlgorithmName("KECCAK384")), s),
                "KECCAK512" => HashUtility.ComputeHashString(HashUtility.Create(new HashAlgorithmName("KECCAK512")), s),
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
        if (!string.IsNullOrWhiteSpace(test)) json.SetValue("verify", test == s);
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
        s = (request.Data.TryGetBooleanValue("decrypt") ?? false) ? alg switch
        {
            "AES" => SymmetricUtility.Encrypt(Aes.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            "3DES" => SymmetricUtility.Encrypt(TripleDES.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            _ => null
        } : alg switch
        {
            "AES" => SymmetricUtility.DecryptText(Aes.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            "3DES" => SymmetricUtility.DecryptText(TripleDES.Create, s, request.Data.TryGetStringValue("key"), request.Data.TryGetStringValue("iv")),
            _ => null
        };
        if (s == null) return new("The algorithm is not supported.");
        return new(new JsonObjectNode()
        {
            { "value", s },
            { "algorithm", alg },
            { "encode", "hex" }
        });
    }

    public static async Task<LocalWebAppResponseMessage> OpenFileAsync(LocalWebAppRequestMessage request, LocalWebAppHost host = null)
    {
        var path = request?.Data?.TryGetStringValue("path")?.Trim();
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
            if (host != null) path = host.GetLocalPath(path, true);
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

    private static async Task<LocalWebAppResponseMessage> OnLocalWebAppMessageRequestAsync(LocalWebAppRequestMessage request, LocalWebAppHost host)
    {
        if (string.IsNullOrEmpty(request?.Command)) return null;
        switch (request.Command.ToLowerInvariant())
        {
            case "list-file":
                return ListFiles(request, host);
            case "get-file":
                return await GetFileAsync(request, host);
            case "write-file":
                return await WriteFileAsync(request, host);
            case "hash":
                return Hash(request, host);
            case "symmetric":
                return Symmetric(request);
            case "open":
                return await OpenFileAsync(request, host);
        }

        return new LocalWebAppResponseMessage(string.Concat("Not supported for this handler. ", request.MessageHandlerId));
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
            byte[] retVal;
            using (var stream = file.OpenRead())
            {
                retVal = hash.ComputeHash(stream);
            }

            var sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
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

        return null;
    }
}
