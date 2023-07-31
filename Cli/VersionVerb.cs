using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" Please type the directory path.");
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

        console.WriteLine(id);
        var newVersion = Arguments.GetFirst("set")?.TryGet(0)?.Trim();
        if (!string.IsNullOrEmpty(newVersion))
        {
            switch (newVersion.ToLowerInvariant())
            {
                case "++":
                case "increase":
                case "increasement":
                    newVersion = SetVersion(version ?? "0.0.0", null);
                    break;
                case "file":
                    newVersion = Arguments.GetMergedValue("set")[5..];
                    newVersion = SetVersion(id, version, dir, newVersion.Length > 5 ? newVersion : "./localwebapp/localwebapp.cache.json", config?.TryGetObjectValue("ref")?.TryGetInt32Value("minBuildNumber") ?? 0, out _);
                    break;
                case "cache":
                    newVersion = Arguments.GetMergedValue("set")[6..];
                    newVersion = SetVersion(id, version, dir, newVersion.Length > 6 ? newVersion : "./localwebapp/localwebapp.cache.json", config?.TryGetObjectValue("ref")?.TryGetInt32Value("minBuildNumber") ?? 0, out _);
                    break;
                default:
                    if (!newVersion.Contains('.'))
                    {
                        console.Write(ConsoleColor.Red, "Error!");
                        console.WriteLine(" The new version is not in semver format.");
                        return;
                    }

                    break;
            }

            if (string.IsNullOrEmpty(newVersion))
            {
                console.Write(ConsoleColor.Red, "Error!");
                console.WriteLine(" The new version is invalid.");
                return;
            }

            console.WriteLine(string.Concat(version ?? "?", " -> ", newVersion));
            Update(id, newVersion, nodePackage, nodePackageFile, config, configFile);
        }
        else
        {
            console.WriteLine(version ?? "0.0.1");
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

    private string SetVersion(string id, string version, DirectoryInfo root, string path, int minBuildNumber, out int build)
    {
        var file = LocalWebAppHost.GetFileInfoByRelative(root, path);
        var json = file.Length > 0 ? JsonObjectNode.TryParse(file) : new();
        if (json == null)
        {
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
        if (json.TryGetInt32Value("number", out var b))
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
        build = b;
        config.WriteTo(file.FullName, IndentStyles.Compact);
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
                config.WriteTo(configFile.FullName, IndentStyles.Compact);
            }

            nodePackage.WriteTo(nodePackageFile.FullName, IndentStyles.Compact);
            return;
        }

        if (packageConfig == null) return;
        packageConfig.SetValue("version", version);
        config.WriteTo(configFile.FullName, IndentStyles.Compact);
    }

    private static JsonObjectNode GetManifest(JsonObjectNode config)
        => config?.TryGetObjectValue("package") ?? config?.TryGetObjectValue("manifest");
}
