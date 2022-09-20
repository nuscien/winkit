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

internal class InitVerb : BaseCommandVerb
{
    protected override async Task OnProcessAsync(CancellationToken cancellationToken = default)
    {
        const string FileName = "localwebapp.project.json";
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

        if (dir.GetFiles(FileName).Any() || dir.GetDirectories("localwebapp").FirstOrDefault()?.GetFiles(FileName)?.Any() == true)
        {
            console.Write(ConsoleColor.Green, "Already exist.");
            return;
        }

        var packageConfigFile = dir.GetFiles("package.json").FirstOrDefault();
        if (packageConfigFile == null || !packageConfigFile.Exists)
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.Write(" Miss ");
            console.Write(ConsoleColor.Cyan, "package.json");
            console.Write(" file.");
            return;
        }

        var packageConfig = JsonObjectNode.TryParse(packageConfigFile);
        var packageName = packageConfig?.TryGetStringTrimmedValue("name", true);
        if (packageName == null)
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.Write(" Invalid format in ");
            console.Write(ConsoleColor.Cyan, "package.json");
            console.Write(" file.");
            return;
        }

        var folder = Directory.CreateDirectory(Path.Combine(dir.FullName, "localwebapp"));
        var manifest = new JsonObjectNode();
        var json = new JsonObjectNode
        {
            { "package", manifest }
        };
        UpdateManifest(manifest, "title");
        UpdateManifest(manifest, "publisher");
        UpdateManifest(manifest, "copyright");
        UpdateManifest(manifest, "description");
        UpdateManifest(manifest, "website");
        UpdateManifest(manifest, "icon");
        File.WriteAllText(Path.Combine(folder.FullName, FileName), json.ToString(IndentStyles.Compact));
    }

    private void UpdateManifest(JsonObjectNode json, string key)
    {
        var s = Arguments.GetMergedValue(key)?.Trim();
        if (!string.IsNullOrEmpty(s)) json.SetValue(key, s);
    }
}
