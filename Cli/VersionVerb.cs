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
using System.Threading.Tasks;
using Trivial.CommandLine;
using Trivial.IO;
using Trivial.Security;
using Trivial.Text;

namespace Trivial.Web;

internal class VersionVerb : BaseCommandVerb
{
    /// <inheritdoc />
    protected override async Task OnProcessAsync(CancellationToken cancellationToken = default)
    {
        var console = GetConsole();
        var now = DateTime.Now;
        if (string.IsNullOrEmpty(Arguments.Verb?.Value))
        {
            console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version?.ToString());
            return;
        }

        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(Arguments.Verb?.Value);
        if (dir == null || !dir.Exists)
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" Please type a valid directory path.");
            return;
        }

        await RunAsync(null, cancellationToken);
        var dir2 = dir;
        var config = LocalWebAppHost.TryLoadBuildConfig(ref dir2, out var configFile);
        var packageConfig = GetManifest(config);
        var nodePackageFile = FileSystemInfoUtility.TryGetFileInfo(dir, "package.json");
        var nodePackage = JsonObjectNode.TryParse(nodePackageFile);
        if (config == null)
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" The directory does not include a local web app source.");
            return;
        }

        var id = config?.TryGetStringTrimmedValue("id", true) ?? packageConfig?.TryGetStringTrimmedValue("id", true) ?? nodePackage?.TryGetStringTrimmedValue("name", true);
        var version = packageConfig?.TryGetStringTrimmedValue("version", true) ?? nodePackage?.TryGetStringTrimmedValue("version", true);
        if (id == null)
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" The identifier of the local web app is null.");
            return;
        }

        var title = packageConfig?.TryGetStringTrimmedValue("title", true);
        if (title != null && title != id) console.WriteLine($"{title} ({id})");
        else console.WriteLine(ConsoleColor.Magenta, id);
        var newVersion = Arguments.GetFirst("set")?.TryGet(0)?.Trim();
        var minVer = config?.TryGetObjectValue("ref")?.TryGetInt32Value("minBuildNumber") ?? 0;
        if (string.IsNullOrEmpty(newVersion))
        {
            if (Arguments.Has("set") && Arguments.Has("file"))
            {
                newVersion = SetVersion(id, version, dir, null, minVer, out _);
                console.WriteLine(string.Concat(version ?? "?", " -> ", newVersion));
                Update(id, newVersion, nodePackage, nodePackageFile, config, configFile);
            }
            else
            {
                console.WriteLine(version ?? "0.0.1");
            }
        }
        else
        {
            switch (newVersion.ToLowerInvariant())
            {
                case "++":
                case "increase":
                case "increasement":
                    newVersion = SetVersion(version ?? "0.0.0", null);
                    SetVersion(id, version, dir, newVersion, minVer, out _);
                    break;
                case "sync":
                    newVersion = SetVersion(id, version, dir, "*", minVer, out _);
                    break;
                case "file":
                    newVersion = SetVersion(id, version, dir, null, minVer, out _);
                    break;
                default:
                    if (!newVersion.Contains('.'))
                    {
                        console.Write(ConsoleColor.Red, "Error!");
                        console.WriteLine(" The new version is not in semver format.");
                        return;
                    }

                    SetVersion(id, version, dir, newVersion, minVer, out _);
                    break;
            }

            if (string.IsNullOrEmpty(newVersion))
            {
                console.Write(ConsoleColor.Red, "Error!");
                console.WriteLine(" The new version is invalid.");
                if (version != null) console.WriteLine(version);
                return;
            }

            console.WriteLine(string.Concat(version ?? "?", " -> ", newVersion));
            Update(id, newVersion, nodePackage, nodePackageFile, config, configFile);
        }

        var replaceVer = config?.TryGetObjectValue("ref")?.TryGetObjectListValue("replaceVersion");
        if (replaceVer == null)
        {
            var replaceVerSinge = config?.TryGetObjectValue("ref")?.TryGetObjectValue("replaceVersion");
            if (replaceVerSinge != null) replaceVer = new List<JsonObjectNode> { replaceVerSinge };
        }

        if (replaceVer == null) return;
        var replaceCount = 0;
        foreach (var replaceVerSingle in replaceVer)
        {
            replaceCount += ReplaceStrings(replaceVerSingle, dir2, id, title, newVersion ?? version ?? "0.0.1", now).Count;
        }

        if (replaceCount == 1) console.WriteLine("Updated 1 text-based file by replacing version and others info.");
        else if (replaceCount > 1) console.WriteLine(string.Concat("Updated ", replaceCount, " text-based files by replacing version and others info."));
    }

    /// <inheritdoc />
    protected override void OnGetHelp()
    {
        var console = GetConsole();
        console.WriteLine("Update the version of the local web app.");
        console.WriteLine();
        console.WriteLine("Usage:");
        console.Write("  lwac ");
        console.Write(ConsoleColor.Yellow, "version");
        console.Write(' ');
        console.WriteLine(ConsoleColor.Green, "<directory> --set <version>");
        console.WriteLine();
        console.WriteLine("Arguments:");
        WriteArgumentDescription("<directory>", "The directory path of the local web app project to update version.");
        WriteArgumentDescription("--set <version>", "The new version to set; or increase, to add revision automatically.");
        console.WriteLine();
    }

    private string SetVersion(string version, Func<int, int> update)
    {
        var console = GetConsole();
        var split = version.Split('.');
        if (split.Length < 3)
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" The version is not in semver format.");
            return null;
        }

        var build = split[2];
        var dash = build.IndexOf('-');
        var dash2 = build.IndexOf('+');
        if (dash >= 0)
        {
            if (dash2 >= 0) dash = Math.Min(dash, dash2);
        }
        else if (dash2 >= 0)
        {
            dash = dash2;
        }

        var rest = string.Empty;
        if (dash >= 0)
        {
            rest = build[dash..];
            build = build[..dash];
        }

        var suffix = Arguments.GetFirst("suffix")?.TryGet(0);
        if (Arguments.Has("suffix"))
        {
            if (string.IsNullOrEmpty(suffix)) rest = string.Empty;
            else rest = suffix.StartsWith('+') || suffix.StartsWith('-') || suffix.StartsWith('_') || suffix.StartsWith('.') ? suffix : string.Concat('-', suffix);
        }

        if (!int.TryParse(build, out var b))
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" The version is not in semver format.");
            return null;
        }

        if (update != null)
            b = update(b);
        else
            b++;
        build = string.Concat(b, rest);
        split[2] = build;
        return string.Join('.', split);
    }

    private string SetVersion(string id, string version, DirectoryInfo root, string newVersion, int minBuildNumber, out int build)
    {
        var console = GetConsole();
        var path = Arguments.GetFirst("file")?.Value;
        if (string.IsNullOrEmpty(path))
        {
            build = -1;
            return null;
        }

        var file = LocalWebAppHost.GetFileInfoByRelative(root, path);
        var json = file.Length > 0 ? JsonObjectNode.TryParse(file) : new();
        if (json == null)
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" The cache file is invalid.");
            build = -1;
            return null;
        }

        if (minBuildNumber < 0) minBuildNumber = 0;
        if (json.Count == 0) json.SetValue("localwebapps", new JsonArrayNode());
        var config = json;
        var arr = json.TryGetArrayValue("localwebapps");
        if (arr != null)
        {
            json = arr.OfType<JsonObjectNode>().FirstOrDefault(ele => ele.TryGetStringTrimmedValue("id") == id);
            if (json == null)
            {
                json = new()
                {
                    { "id", id }
                };
                arr.Add(json);
            }
        }

        var buildJson = json.TryGetObjectValue("build");
        if (buildJson == null)
        {
            buildJson = new();
            json.SetValue("build", buildJson);
        }

        json = buildJson;
        int b = 0;
        var oldVer = version;
        if (newVersion == "*")
        {
            if (json.TryGetInt32Value("number", out b))
            {
                if (b < minBuildNumber) b = minBuildNumber;
                version = SetVersion(version, oldVersion => b);
            }
            else if (string.IsNullOrEmpty(version))
            {
                version = "1.0.0";
            }
        }
        else if (!string.IsNullOrEmpty(newVersion))
        {
            version = newVersion;
            SetVersion(version, oldVersion =>
            {
                b = oldVersion;
                return b;
            });
        }
        else if (json.TryGetInt32Value("number", out b))
        {
            b++;
            if (b < minBuildNumber) b = minBuildNumber;
            version = SetVersion(version, oldVersion => b);
        }
        else
        {
            b = -1;
            version = SetVersion(version, oldVersion =>
            {
                b = oldVersion + 1;
                if (b < minBuildNumber) b = minBuildNumber;
                return b;
            });
        }

        json.SetValue("number", b);
        json.SetValue("previousVerson", json.TryGetStringTrimmedValue("version"));
        json.SetValue("oldVersion", oldVer);
        json.SetValue("version", version);
        json.SetValue("update", DateTime.Now);
        build = b;
        WriteFile(config, file);
        return version;
    }

    private static void Update(string id, string version, JsonObjectNode nodePackage, FileInfo nodePackageFile, JsonObjectNode config, FileInfo configFile)
    {
        var packageConfig = GetManifest(config);
        if (nodePackage != null && nodePackage.TryGetStringTrimmedValue("name") == id)
        {
            nodePackage.SetValue("version", version);
            if (packageConfig != null && packageConfig.ContainsKey("version"))
            {
                packageConfig.Remove("version");
                WriteFile(config, configFile);
            }

            WriteFile(nodePackage, nodePackageFile);
            return;
        }

        if (packageConfig == null) return;
        packageConfig.SetValue("version", version);
        WriteFile(config, configFile);
    }

    private IList<FileInfo> ReplaceStrings(JsonObjectNode json, DirectoryInfo root, string id, string title, string version, DateTime now)
        => ReplaceStringsLazy(json, root, id, title, version, now).ToList();

    private IEnumerable<FileInfo> ReplaceStringsLazy(JsonObjectNode json, DirectoryInfo root, string id, string title, string version, DateTime now)
    {
        var files = json?.TryGetStringListValue("files");
        if (files == null || files.Count < 1) yield break;
        var placeholders = json.TryGetObjectValue("placeholders");
        var encodingStr = json.TryGetStringTrimmedValue("encoding") ?? string.Empty;
        var encoding = encodingStr.Trim().ToLowerInvariant().Replace("-", string.Empty) switch
        {
            "utf8" or "" => Encoding.UTF8,
            "unicode" or "utf16" => Encoding.Unicode,
            "ascii" => Encoding.ASCII,
            "utf32" => Encoding.UTF32,
            _ => Encoding.GetEncoding(encodingStr) ?? Encoding.UTF8
        };
        if (placeholders == null || json.TryGetBooleanValue("disable") == true) yield break;
        var idOriginal = placeholders.TryGetStringTrimmedValue("id", true);
        var titleOriginal = placeholders.TryGetStringTrimmedValue("title", true);
        var versionOriginal = placeholders.TryGetStringTrimmedValue("version", true);
        var timeOriginal = placeholders.TryGetStringTrimmedValue("tick", true);
        var tickStr = WebFormat.ParseDate(now).ToString("g");
        foreach (var filePath in files)
        {
            var file = LocalWebAppHost.GetFileInfoByRelative(root, filePath);
            if (file == null || !file.Exists) continue;
            var s = File.ReadAllText(file.FullName, encoding);
            var old = s;
            if (s == null) continue;
            if (idOriginal != null) s = s.Replace(idOriginal, id);
            if (titleOriginal != null) s = s.Replace(titleOriginal, title);
            if (versionOriginal != null) s = s.Replace(versionOriginal, version);
            if (timeOriginal != null) s = s.Replace(timeOriginal, tickStr);
            if (old == s) continue;
            File.WriteAllText(file.FullName, s, encoding);
            yield return file;
        }
    }

    private static JsonObjectNode GetManifest(JsonObjectNode config)
        => config?.TryGetObjectValue("package") ?? config?.TryGetObjectValue("manifest");

    private static bool WriteFile(JsonObjectNode json, FileInfo file)
    {
        try
        {
            File.WriteAllText(file.FullName, json.ToString(IndentStyles.Compact) ?? "null");
            file.Refresh();
            return true;
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (IOException)
        {
        }
        catch (JsonException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ApplicationException)
        {
        }
        catch (ExternalException)
        {
        }

        return false;
    }

    private void WriteArgumentDescription(string key, string description)
    {
        var console = GetConsole();
        console.Write("  ");
        console.Write(ConsoleColor.Green, key);
        console.Write('\t');
        console.WriteLine(description);
    }
}
