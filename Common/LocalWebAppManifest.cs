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
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace Trivial.Web;

/// <summary>
/// The information of standalone web app.
/// </summary>
public class LocalWebAppManifest
{
    /// <summary>
    /// Gets or sets the name of the app.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the version of the app.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the description of the app.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the publisher name of the app.
    /// </summary>
    [JsonPropertyName("publisher")]
    public string PublisherName { get; set; }

    /// <summary>
    /// Gets or sets the copyright information.
    /// </summary>
    [JsonPropertyName("copyright")]
    public string Copyright { get; set; }

    /// <summary>
    /// Gets or sets the official website URL.
    /// </summary>
    [JsonPropertyName("website")]
    public string Website { get; set; }

    /// <summary>
    /// Gets or sets the relative path of homepage.
    /// </summary>
    [JsonPropertyName("entry")]
    public string HomepagePath { get; set; }

    /// <summary>
    /// The file list of HTML, JavaScript, Type Script, JSON and CSS.
    /// </summary>
    [JsonPropertyName("files")]
    public List<LocalWebAppFileInfo> Files { get; set; }

    /// <summary>
    /// The JSON data file list.
    /// </summary>
    [JsonPropertyName("json")]
    public Dictionary<string, LocalWebAppFileInfo> JsonBindings { get; set; }

    /// <summary>
    /// The text data file list.
    /// </summary>
    [JsonPropertyName("text")]
    public Dictionary<string, LocalWebAppFileInfo> TextBindings { get; set; }

    /// <summary>
    /// The host app binding information.
    /// </summary>
    [JsonPropertyName("host")]
    public List<LocalWebAppHostBindingInfo> HostBinding { get; set; }

    /// <summary>
    /// The update information.
    /// </summary>
    [JsonPropertyName("update")]
    public WebAppPackageUpdateInfo Update { get; set; }

    /// <summary>
    /// The tags.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The root directory should not be null.</exception>
    /// <exception cref="DirectoryNotFoundException">The root directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    public static async Task<LocalWebAppManifest> LoadAsync(LocalWebAppOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        var rootPath = options.RootDirectory;
        if (rootPath == null) throw new InvalidOperationException("The root directory should not be null or empty.");
        var manifestFileName = options.ManifestFileName;
        if (string.IsNullOrWhiteSpace(manifestFileName)) throw new InvalidOperationException("The resource manifest should not be null or empty.");
        if (!rootPath.Exists)
        {
            string errorMessage = null;
            try
            {
                if (!string.IsNullOrEmpty(rootPath.FullName))
                    errorMessage = string.Concat("The root directory is not found. Path: ", rootPath.FullName);
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

            throw new DirectoryNotFoundException(errorMessage ?? "rootPath is not found.");
        }

        var file = rootPath.EnumerateFiles(manifestFileName).FirstOrDefault();
        if (file == null || !file.Exists) throw new FileNotFoundException("The resource manifest is not found.");
        var manifest = await JsonSerializer.DeserializeAsync<LocalWebAppManifest>(file.OpenRead(), null as JsonSerializerOptions, cancellationToken);
        if (manifest == null) throw new FormatException("The format of the package manifest is not correct.");
#if RELEASE
        try
        {
            var files = rootPath.EnumerateFiles("*.html", SearchOption.AllDirectories)
                .Union(rootPath.EnumerateFiles("*.htm", SearchOption.AllDirectories))
                .Union(rootPath.EnumerateFiles("*.js", SearchOption.AllDirectories))
                .Union(rootPath.EnumerateFiles("*.ts", SearchOption.AllDirectories))
                .Union(rootPath.EnumerateFiles("*.css", SearchOption.AllDirectories))
                .Union(rootPath.EnumerateFiles("*.json", SearchOption.AllDirectories))
                .ToList();
            if (manifest.Files != null)
            {
                foreach (var item in manifest.Files)
                {
                    if (!item.Verify(options, out var fileInfo))
                    {
                        if (fileInfo == null || !fileInfo.Exists) continue;
                        throw new InvalidOperationException("Incorrect signature."); // Maybe need throw a new signature failure exception.
                    }

                    if (fileInfo != null) files.Remove(fileInfo);
                }
            }

            if (files.Count > 0)
                throw new InvalidOperationException(files.Count == 1 ? "Miss signature for a file." : $"Miss signature for {files.Count} files."); // Maybe need throw a new signature missed exception.
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
        catch (NotSupportedException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (CryptographicException)
        {
        }
        catch (ExternalException)
        {
        }
#endif
        return manifest;
    }
}

/// <summary>
/// The options of the standalone web app resource package.
/// </summary>
public class LocalWebAppOptions
{
    /// <summary>
    /// Gets or sets the file name of the manifest.
    /// </summary>
    public string ManifestFileName { get; set; } = "edgeplatform.json";

    /// <summary>
    /// Gets or sets the virtual host.
    /// </summary>
    public string VirtualHost { get; set; } = "privateapp.edgeplatform.localhost";

    /// <summary>
    /// Gets or sets the root directory.
    /// </summary>
    public DirectoryInfo RootDirectory { get; set; }

    /// <summary>
    /// Gets or sets the temp directory.
    /// </summary>
    public DirectoryInfo TempDirectory { get; set; }

    /// <summary>
    /// Gets or sets the public key.
    /// </summary>
    public RSAParameters PublicKey { get; set; }

    /// <summary>
    /// Gets or sets the hash algorithm name.
    /// </summary>
    public HashAlgorithmName HashAlgorithmName { get; set; } = HashAlgorithmName.SHA512;

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
    /// Reads the file.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The file stream.</returns>
    public virtual FileStream ReadFile(FileInfo file)
    {
        if (file == null || !file.Exists) return null;
        var temp = TempDirectory;
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
            var path = Path.Combine(TempDirectory.FullName, string.Concat("temp-", file.Name));
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
}
