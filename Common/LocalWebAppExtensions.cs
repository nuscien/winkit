using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static LocalWebAppMessageResponseBody ListFiles(LocalWebAppMessageRequest request, LocalWebAppOptions options)
    {
        var path = request?.Data?.TryGetStringValue("path");
        if (path.StartsWith('.') || path.StartsWith('~')) path = options.GetLocalPath(path);
        var dir = FileSystemInfoUtility.GetDirectoryInfo(path);
        if (dir == null || !dir.Exists) return new()
        {
            Message = "Not found.",
            IsError = true
        };
        var resp = new LocalWebAppMessageResponseBody
        {
            Data = new()
            {
                { "info", ToJson(dir) }
            }
        };
        var dirList = dir.EnumerateDirectories();
        var data = new JsonArrayNode();
        foreach (var item in dirList)
        {
            data.Add(ToJson(item));
        }

        resp.Data.SetValue("dirs", data);
        var fileList = dir.EnumerateFiles();
        data = new JsonArrayNode();
        foreach (var item in fileList)
        {
            data.Add(ToJson(item));
        }

        resp.Data.SetValue("files", data);
        return resp;
    }

    private static JsonObjectNode ToJson(DirectoryInfo item)
        => new()
        {
            { "name", item.Name },
            { "modified", item.LastWriteTime },
            { "created", item.CreationTime },
            { "attr", (int)item.Attributes }
        };

    private static JsonObjectNode ToJson(FileInfo item)
    {
        var json = new JsonObjectNode
            {
                { "name", item.Name },
                { "modified", item.LastWriteTime },
                { "created", item.CreationTime },
                { "attr", (int)item.Attributes },
                { "length", item.Length },
            };
        if (!string.IsNullOrWhiteSpace(item.LinkTarget))
            json.SetValue("link", item.LinkTarget);
        return json;
    }
}
