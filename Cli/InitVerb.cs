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
    /// <inheritdoc />
    protected override async Task OnProcessAsync(CancellationToken cancellationToken = default)
    {
        const string FileName = "localwebapp.project.json";
        var console = GetConsole();
        if (string.IsNullOrEmpty(Arguments.Verb?.Value))
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" Please type a directory path.");
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
            { "$schema", "https://nuscien.github.io/winkit/schema/localwebapp.project.v6.json" },
            { "package", manifest }
        };
        var skipTyping = Arguments.Has("direct", "d");
        UpdateManifest(manifest, "title", skipTyping);
        UpdateManifest(manifest, "publisher", skipTyping);
        UpdateManifest(manifest, "copyright", skipTyping);
        UpdateManifest(manifest, "description", skipTyping);
        UpdateManifest(manifest, "website", skipTyping);
        UpdateManifest(manifest, "icon", skipTyping);
        var outputDir = "dist";
        if (!skipTyping)
        {
            console.Write($"Package directory ({outputDir}): ");
            try
            {
                var inputOutputDir = console.ReadLine();
                if (!string.IsNullOrWhiteSpace(inputOutputDir)) outputDir = inputOutputDir;
            }
            catch (IOException)
            {
            }
        }

        json.SetValue("ref", new JsonObjectNode
        {
            { "path", outputDir }
        });
        console.Write("Initializing…");
        await File.WriteAllTextAsync(Path.Combine(folder.FullName, FileName), json.ToString(IndentStyles.Compact), cancellationToken);
        var rsa = RSA.Create().ExportParameters(true);
        var filePath = Path.Combine(folder.FullName, "localwebapp.private.pem");
        if (!File.Exists(filePath))
        {
            var s = RSAParametersConvert.ToPrivatePEMString(rsa, true);
            await File.WriteAllTextAsync(filePath, s, cancellationToken);
        }

        filePath = Path.Combine(folder.FullName, "localwebapp.pem");
        if (!File.Exists(filePath))
        {
            var s = RSAParametersConvert.ToPublicPEMString(rsa);
            await File.WriteAllTextAsync(filePath, s, cancellationToken);
        }

        filePath = Path.Combine(folder.FullName, ".gitignore");
        if (!File.Exists(filePath))
        {
            var s = $"localwebapp.zip{Environment.NewLine}localwebapp/localwebapp.zip{Environment.NewLine}localwebapp/data{Environment.NewLine}localwebapp/cache{Environment.NewLine}localwebapp/logs{Environment.NewLine}localwebapp/temp";
            await File.WriteAllTextAsync(filePath, s, cancellationToken);
        }

        console.BackspaceToBeginning();
        console.WriteLine(ConsoleColor.Green, "Done!");
    }

    /// <inheritdoc />
    protected override void OnGetHelp()
    {
        var console = GetConsole();
        console.WriteLine("Initializes a new local web app project.");
        console.WriteLine();
        console.WriteLine("Usage:");
        console.Write("  lwac ");
        console.Write(ConsoleColor.Yellow, "init");
        console.Write(' ');
        console.WriteLine("<directory>", ConsoleColor.Green);
        console.WriteLine();
        console.WriteLine("Arguments:");
        WriteArgumentDescription("<directory>", "The directory path of the local web app project to initialize.");
        console.WriteLine();
    }

    private bool UpdateManifest(JsonObjectNode json, string key, bool skipTyping = false)
    {
        var s = Arguments.GetMergedValue(key)?.Trim();
        if (string.IsNullOrEmpty(s) && !skipTyping)
        {
            var console = GetConsole();
            console.Write($"{key.ToSpecificCase(Cases.Capitalize)}: ");
            try
            {
                s = console.ReadLine();
            }
            catch (IOException)
            {
            }
        }

        if (string.IsNullOrEmpty(s)) return false;
        json.SetValue(key, s);
        return true;
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
