using System;
using System.Collections.Generic;
using System.IO.Compression;
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

        console.Write("Build and package...");
        var package = LocalWebAppHost.Package(dir, TryGetCompressionLevel(), null);
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

    /// <inheritdoc />
    protected override void OnGetHelp()
    {
        var console = GetConsole();
        console.WriteLine("Build and package a local web app project.");
        console.WriteLine();
        console.WriteLine("Usage:");
        console.Write("  lwac ");
        console.Write(ConsoleColor.Yellow, "build");
        console.Write(' ');
        console.WriteLine(ConsoleColor.Green, "<directory> [--compress <level>]");
        console.WriteLine();
        console.WriteLine("Arguments:");
        WriteArgumentDescription("<directory>", "The directory path of the local web app project to build.");
        WriteArgumentDescription("--compress <level>", "The compression level to use for the package. Options: auto, fast, no, smallest.");
        console.WriteLine();
    }

    private CompressionLevel? TryGetCompressionLevel()
    {
        var compress = Arguments.GetFirst("compress")?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(compress)) return null;
        if (Enum.TryParse<CompressionLevel>(compress, true, out var c)) return c;
        return compress.ToLowerInvariant() switch
        {
            "auto" => CompressionLevel.Optimal,
            "fast" => CompressionLevel.Fastest,
            "no" or "none" => CompressionLevel.NoCompression,
            "smallest" or "small" => CompressionLevel.SmallestSize,
            _ => null
        };
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
