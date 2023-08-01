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
        var config = LocalWebAppHost.TryLoadBuildConfig(dir, out var configFile);
        if (config == null)
        {
            var dir2 = dir.EnumerateDirectories("localwebapp").FirstOrDefault();
            if (dir2 != null) config = LocalWebAppHost.TryLoadBuildConfig(dir2, out configFile);
        }

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
        var version = packageConfig?.TryGetStringTrimmedValue("version", true) ?? nodePackage.TryGetStringTrimmedValue("version", true);
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
        if (!string.IsNullOrEmpty(newVersion))
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
}
