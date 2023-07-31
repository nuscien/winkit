using System;
using System.Collections.Generic;
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

        await RunAsync();
        var config = LocalWebAppHost.LoadBuildConfig(dir, out var configFile);
        if (config == null)
        {
            var dir2 = dir.EnumerateDirectories("localwebapp").FirstOrDefault();
            if (dir2 != null) config = LocalWebAppHost.LoadBuildConfig(dir2, out configFile);
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
        var version = packageConfig?.TryGetStringTrimmedValue("version", true) ?? config.TryGetStringTrimmedValue("version", true);
        if (id == null)
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" The identifier of the local web app is null.");
            return;
        }

        console.WriteLine(id);
        var newVersion = Arguments.GetFirst("set")?.Value?.Trim();
        if (!string.IsNullOrEmpty(newVersion))
        {
            version ??= "0.0.0";
            switch (newVersion.ToLowerInvariant())
            {
                case "increase":
                    newVersion = Increase(version);
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

            console.WriteLine(string.Concat(version, " -> ", newVersion));
            Update(id, newVersion, nodePackage, nodePackageFile, config, configFile);
        }
        else
        {
            console.WriteLine(version ?? "0.0.1");
        }
    }

    private string Increase(string version)
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

        b++;
        build = string.Concat(b, rest);
        split[2] = build;
        return string.Join('.', split);
    }

    private static void Update(string id, string version, JsonObjectNode nodePackage, FileInfo nodePackageFile, JsonObjectNode config, FileInfo configFile)
    {
        var packageConfig = GetManifest(config);
        if (nodePackage != null && nodePackage.TryGetStringTrimmedValue("name") == id)
        {
            nodePackage.SetValue("version", version);
            if (config != null && config.ContainsKey("version"))
            {
                config.Remove("version");
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
