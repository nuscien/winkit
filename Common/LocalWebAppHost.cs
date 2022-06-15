using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Collection;
using Trivial.Data;
using Trivial.IO;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace Trivial.Web;

/// <summary>
/// The local standalone web app host.
/// </summary>
public class LocalWebAppHost
{
    private LocalWebAppHost(LocalWebAppManifest manifest, LocalWebAppOptions options)
    {
        Manifest = manifest;
        Options = options;
        ResourcePackageId = options?.ResourcePackageId?.Trim();
        if (string.IsNullOrEmpty(ResourcePackageId)) return;
        VirtualHost = string.IsNullOrEmpty(ResourcePackageId) ? "privateapp.edgeplatform.localhost" : string.Concat(
            ResourcePackageId.Split(new[] { '.', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault().Replace("@", string.Empty).Trim().ToLowerInvariant(),
            ".edgeplatform.localhost");
    }

    /// <summary>
    /// Gets the cache directory.
    /// </summary>
    internal DirectoryInfo CacheDirectory { get; private set; }

    /// <summary>
    /// Gets the resource package identifier of the local standalone web app.
    /// </summary>
    public string ResourcePackageId { get; }

    /// <summary>
    /// Gets the manifest.
    /// </summary>
    public LocalWebAppManifest Manifest { get; }

    /// <summary>
    /// Gets the options.
    /// </summary>
    public LocalWebAppOptions Options { get; }

    /// <summary>
    /// Gets or sets the virtual host.
    /// </summary>
    public string VirtualHost { get; private set; }

    /// <summary>
    /// Gets the resource package directory.
    /// </summary>
    public DirectoryInfo ResourcePackageDirectory { get; private set; }

    /// <summary>
    /// Gets the data directory.
    /// </summary>
    public DirectoryInfo DataDirectory { get; private set; }

    /// <summary>
    /// Gets online path of a relative embedded path.
    /// </summary>
    /// <param name="localRelativePath">The relative path of embedded file.</param>
    /// <returns>A online path mapped.</returns>
    public string GetVirtualPath(string localRelativePath)
    {
        var host = VirtualHost?.Trim();
        if (string.IsNullOrEmpty(host)) return null;
        if (localRelativePath.StartsWith('.')) localRelativePath = localRelativePath[1..];
        else if (localRelativePath.StartsWith('~')) localRelativePath = localRelativePath[1..];
        if (localRelativePath.StartsWith('/')) localRelativePath = localRelativePath[1..];
        return string.Concat("https://", host, '/', localRelativePath);
    }

    /// <summary>
    /// Gets online path of a relative embedded path.
    /// </summary>
    /// <param name="localRelativePath">The relative path of embedded file.</param>
    /// <returns>A online path mapped.</returns>
    public string GetLocalPath(string localRelativePath)
    {
        var host = ResourcePackageDirectory?.FullName;
        if (string.IsNullOrEmpty(host)) return null;
        if (localRelativePath.StartsWith('.')) localRelativePath = localRelativePath[1..];
        else if (localRelativePath.StartsWith('~')) localRelativePath = localRelativePath[1..];
        return Path.Combine(host, localRelativePath);
    }

    /// <summary>
    /// Reads the file.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The file stream.</returns>
    public FileStream ReadFile(FileInfo file)
    {
        if (file == null || !file.Exists) return null;
        var temp = Directory.CreateDirectory(Path.Combine(CacheDirectory.FullName, "read"));
        if (temp == null || !temp.Exists) return file.OpenRead();
        try
        {
            return file.OpenRead();
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
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ExternalException)
        {
        }

        try
        {
            var path = Path.Combine(temp.FullName, file.Name);
            file = file.CopyTo(path, true);
            return file.OpenRead();
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
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(LocalWebAppOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrEmpty(options.ResourcePackageId)) throw new InvalidOperationException("The resource package identifier should not be null or empty.");
        var appDataFolder = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("LocalWebApp", Windows.Storage.CreationCollisionOption.OpenIfExists);
        var appId = options.ResourcePackageId.Replace('/', '_').Replace('\\', '_').Replace(' ', '_').Replace("@", string.Empty);
        appDataFolder = await appDataFolder.CreateFolderAsync(appId, Windows.Storage.CreationCollisionOption.OpenIfExists);
        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(appDataFolder.Path);
        return await LoadAsync(dir, options, null, cancellationToken);
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="rootDir">The root directory for local standalone web app.</param>
    /// <param name="options">The options to parse.</param>
    /// <param name="appDir">The optional resource package directory to load the local standalone web app.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="SecurityException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(DirectoryInfo rootDir, LocalWebAppOptions options, DirectoryInfo appDir = null, CancellationToken cancellationToken = default)
    {
        if (rootDir == null || !rootDir.Exists) throw new ArgumentNullException(nameof(rootDir));
        if (options == null) throw new ArgumentNullException(nameof(options));

        // Initialize and verify sub-folders.
        var dataDir = Directory.CreateDirectory(Path.Combine(rootDir.FullName, "data"));
        var cacheDir = Directory.CreateDirectory(Path.Combine(rootDir.FullName, "cache"));
        var settingsFile = cacheDir.GetFiles("settings.json").FirstOrDefault();
        if (appDir == null && settingsFile != null && settingsFile.Exists)
        {
            try
            {
                using var stream = settingsFile.OpenRead();
                var settings = await JsonObjectNode.ParseAsync(stream, cancellationToken);
                var v = settings.TryGetStringValue("v");
                appDir = appDir.GetDirectories(string.Concat('v', v)).FirstOrDefault();
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (JsonException)
            {
            }
            catch (FormatException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (AggregateException)
            {
            }
            catch (ExternalException)
            {
            }
        }

        if (appDir == null || !appDir.Exists)
            appDir = rootDir.EnumerateDirectories("v*.*").OrderBy(ele => ele.CreationTime).LastOrDefault();
        if (appDir == null || !appDir.Exists)
        {
            string errorMessage = null;
            try
            {
                if (!string.IsNullOrEmpty(appDir.FullName))
                    errorMessage = string.Concat("The resource package directory is not found. Path: ", appDir.FullName);
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (ExternalException)
            {
            }

            throw new DirectoryNotFoundException(errorMessage ?? "The resource package is not found.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Load manifest.
        var manifestFileName = options.ManifestFileName;
        if (string.IsNullOrWhiteSpace(manifestFileName)) manifestFileName = "edgeplatform.json";
        var file = appDir.EnumerateFiles(manifestFileName).FirstOrDefault();
        if (file == null || !file.Exists) throw new FileNotFoundException("The resource manifest is not found.");
        var manifest = await JsonSerializer.DeserializeAsync<LocalWebAppManifest>(file.OpenRead(), null as JsonSerializerOptions, cancellationToken);
        if (string.IsNullOrWhiteSpace(manifest?.Id)) throw new FormatException("The format of the package manifest is not correct.");
        if (manifest.Id.Trim().ToLowerInvariant() != options.ResourcePackageId?.Trim()?.ToLowerInvariant())
            throw new InvalidOperationException("The app is not the specific one.");
        var host = new LocalWebAppHost(manifest, options)
        {
            ResourcePackageDirectory = appDir,
            DataDirectory = dataDir,
            CacheDirectory = cacheDir,
        };

        // Debug mode.
        if (options.DebugMode)
        {
            return host;
        }

        // Verify files.
        if (manifest.Files == null) throw new SecurityException("Miss file signatures.");
        var files = appDir.EnumerateFiles("*.html", SearchOption.AllDirectories)
            .Union(appDir.EnumerateFiles("*.htm", SearchOption.AllDirectories))
            .Union(appDir.EnumerateFiles("*.js", SearchOption.AllDirectories))
            .Union(appDir.EnumerateFiles("*.ts", SearchOption.AllDirectories))
            .Union(appDir.EnumerateFiles("*.css", SearchOption.AllDirectories))
            .Union(appDir.EnumerateFiles("*.json", SearchOption.AllDirectories))
            .ToList();
        foreach (var item in manifest.Files)
        {
            if (!item.Verify(host, out var fileInfo))
            {
                if (fileInfo == null || !fileInfo.Exists) continue;
                throw new SecurityException("Incorrect signature.");
            }

            if (fileInfo != null) files.Remove(fileInfo);
        }

        if (files.Count > 0)
            throw new InvalidOperationException(files.Count == 1 ? "Miss signature for a file." : $"Miss signature for {files.Count} files."); // Maybe need throw a new signature missed exception.

        // Return result.
        return host;
    }
}
