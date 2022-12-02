using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Trivial.CommandLine;
using Trivial.IO;
using Trivial.Security;

namespace Trivial.Web;

internal class BuildVerb : BaseCommandVerb
{
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

        console.Write("Build and package...");
        var package = LocalWebAppHost.Package(dir);
        var host = await LocalWebAppHost.LoadAsync(package, true, cancellationToken);
        console.BackspaceToBeginning();
        var manifest = host?.Manifest;
        if (string.IsNullOrWhiteSpace(manifest?.Id))
        {
            console.Write(ConsoleColor.Red, "Error!");
            console.WriteLine(" Failed to build.");
            return;
        }

        if (host.IsVerified)
        {
            console.WriteLine(ConsoleColor.Green, "Success!");
        }
        else
        {
            console.Write(ConsoleColor.Green, "Success");
            console.WriteLine(" without verification.");
        }

        console.WriteLine($"{manifest.DisplayName} ({manifest.Id}) v{manifest.Version}");
        if (!string.IsNullOrWhiteSpace(manifest.PublisherName)) console.WriteLine(manifest.PublisherName);
        if (!string.IsNullOrWhiteSpace(manifest.Website)) console.WriteLine(ConsoleColor.Cyan, manifest.Website);
        if (!string.IsNullOrWhiteSpace(manifest.Description)) console.WriteLine(manifest.Description);
        if (!string.IsNullOrWhiteSpace(manifest.Copyright)) console.WriteLine(manifest.Copyright);
        var zip = package.Signature;
        if (zip == null || !zip.Exists) return;
        console.WriteLine(ConsoleColor.Cyan, zip.FullName);
        var hash = HashUtility.ComputeHashString(SHA256.Create, zip);
        console.WriteLine($"SHA256 \t{hash}");
        hash = HashUtility.ComputeHashString(SHA512.Create, zip);
        console.WriteLine($"SHA512 \t{hash}");
    }
}
