using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
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
using System.IO.Compression;

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
        string appDomain = null;
        if (!string.IsNullOrEmpty(ResourcePackageId))
        {
            var appDomainArr = ResourcePackageId.Split(new[] { '.', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (appDomainArr.Length > 1) appDomain = string.Concat(appDomainArr[1], appDomainArr[0]);
            else appDomain = appDomainArr.FirstOrDefault();
        }

        if (string.IsNullOrEmpty(appDomain)) appDomain = "privateapp";
        VirtualHost = string.Concat(appDomain, '.', UI.LocalWebAppExtensions.VirtualRootDomain);
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
    /// Gets the signature provider.
    /// </summary>
    public ISignatureProvider SignatureProvider { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the files are verified.
    /// </summary>
    public bool IsVerified { get; private set; }

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
    /// Gets the static data resources.
    /// </summary>
    public JsonObjectNode DataResources { get; } = new();

    /// <summary>
    /// Gets the static data strings.
    /// </summary>
    public IDictionary<string, string> DataStrings { get; } = new Dictionary<string, string>();

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
    /// <param name="test">true if combine only when it tests passed; otherwise, false.</param>
    /// <returns>A online path mapped.</returns>
    public string GetLocalPath(string localRelativePath, bool test = false)
    {
        var host = ResourcePackageDirectory?.FullName;
        if (localRelativePath.StartsWith("..")) localRelativePath = GetResourcePackageParentPath(localRelativePath);
        else if (localRelativePath.StartsWith('~')) localRelativePath = localRelativePath[1..];
        else if (localRelativePath.StartsWith(".asset:")) localRelativePath = localRelativePath[7..];
        else if (localRelativePath.StartsWith(".data:")) return Path.Combine(DataDirectory?.FullName ?? host, localRelativePath[6..]);
        else if (localRelativePath.StartsWith("./")) localRelativePath = localRelativePath[2..];
        else if (localRelativePath.StartsWith('.')) return null;
        else if (localRelativePath.StartsWith('%')) localRelativePath = FileSystemInfoUtility.GetLocalPath(localRelativePath);
        else if (localRelativePath.Contains("://")) return localRelativePath;
        else if (test) return localRelativePath;
        return string.IsNullOrEmpty(host) ? null : Path.Combine(host, localRelativePath);
    }

    /// <summary>
    /// Reads the file.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The file stream.</returns>
    public FileStream TryReadFile(FileInfo file)
    {
        if (file == null || !file.Exists) return null;
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

        var temp = Directory.CreateDirectory(Path.Combine(CacheDirectory.FullName, "read"));
        if (temp == null || !temp.Exists) return null;
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
    /// Reads the file.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The file stream.</returns>
    public string TryReadFileText(FileInfo file)
    {
        if (file == null || !file.Exists) return null;
        try
        {
            return File.ReadAllText(file.FullName);
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

        var temp = Directory.CreateDirectory(Path.Combine(CacheDirectory.FullName, "read"));
        if (temp == null || !temp.Exists) return null;
        try
        {
            var path = Path.Combine(temp.FullName, file.Name);
            file = file.CopyTo(path, true);
            return File.ReadAllText(file.FullName);
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
    /// Reads the file.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The file stream.</returns>
    public async Task<JsonObjectNode> TryReadFileJsonAsync(FileInfo file, CancellationToken cancellationToken = default)
    {
        using var stream = TryReadFile(file);
        if (stream == null) return null;
        try
        {
            return await JsonObjectNode.ParseAsync(stream, cancellationToken);
        }
        catch (JsonException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (FormatException)
        {
        }
        catch (IOException)
        {
        }

        return null;
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(LocalWebAppOptions options, bool skipVerificationException = false, CancellationToken cancellationToken = default)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrEmpty(options.ResourcePackageId)) throw new InvalidOperationException("The resource package identifier should not be null or empty.");
        var appDataFolder = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("LocalWebApp", Windows.Storage.CreationCollisionOption.OpenIfExists);
        var appId = options.ResourcePackageId.Replace('/', '_').Replace('\\', '_').Replace(' ', '_').Replace("@", string.Empty);
        appDataFolder = await appDataFolder.CreateFolderAsync(appId, Windows.Storage.CreationCollisionOption.OpenIfExists);
        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(appDataFolder.Path);
        return await LoadAsync(dir, options, skipVerificationException, null, cancellationToken);
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="rootDir">The root directory for local standalone web app.</param>
    /// <param name="options">The options to parse.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="appDir">The optional resource package directory to load the local standalone web app.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="SecurityException">Signature failed.</exception>
    public static Task<LocalWebAppHost> LoadAsync(DirectoryInfo rootDir, LocalWebAppOptions options, bool skipVerificationException = false, DirectoryInfo appDir = null, CancellationToken cancellationToken = default)
        => LoadAsync(rootDir, options, skipVerificationException ? LocalWebAppVerificationOptions.SkipException : LocalWebAppVerificationOptions.Regular, appDir, cancellationToken);

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="rootDir">The root directory for local standalone web app.</param>
    /// <param name="options">The options to parse.</param>
    /// <param name="verifyOptions">The verification options.</param>
    /// <param name="appDir">The optional resource package directory to load the local standalone web app.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="SecurityException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(DirectoryInfo rootDir, LocalWebAppOptions options, LocalWebAppVerificationOptions verifyOptions = LocalWebAppVerificationOptions.Regular, DirectoryInfo appDir = null, CancellationToken cancellationToken = default)
    {
        if (rootDir == null || !rootDir.Exists) throw new ArgumentNullException(nameof(rootDir));
        if (options == null) throw new ArgumentNullException(nameof(options));

        // Initialize and verify sub-folders.
        var dataDir = Directory.CreateDirectory(Path.Combine(rootDir.FullName, "data"));
        var cacheDir = Directory.CreateDirectory(Path.Combine(rootDir.FullName, "cache"));
        if (appDir == null)
        {
            var settings = await TryGetSettingsAsync(cacheDir, cancellationToken);
            var v = settings?.TryGetStringValue("version")?.Trim();
            if (!string.IsNullOrEmpty(v))
                appDir = rootDir.GetDirectories(string.Concat('v', v)).FirstOrDefault();
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
        if (string.IsNullOrWhiteSpace(manifestFileName)) manifestFileName = UI.LocalWebAppExtensions.DefaultManifestFileName;
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

        // Bind static resources from given files.
        var filesToBind = host?.Manifest?.JsonBindings;
        if (filesToBind != null)
        {
            foreach (var kvp in filesToBind)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value)) continue;
                try
                {
                    var path = host.GetLocalPath(kvp.Value);
                    if (string.IsNullOrEmpty(path)) continue;
                    var fileToBind = FileSystemInfoUtility.TryGetFileInfo(path);
                    if (fileToBind == null) continue;
                    var v = await host.TryReadFileJsonAsync(fileToBind, cancellationToken);
                    if (v == null) continue;
                    host.DataResources.SetValue(kvp.Key, v);
                }
                catch (IOException)
                {
                }
            }
        }

        filesToBind = host?.Manifest?.TextBindings;
        if (filesToBind != null)
        {
            foreach (var kvp in filesToBind)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value)) continue;
                try
                {
                    var path = host.GetLocalPath(kvp.Value);
                    if (string.IsNullOrEmpty(path)) continue;
                    var fileToBind = FileSystemInfoUtility.TryGetFileInfo(path);
                    if (fileToBind == null) continue;
                    var v = host.TryReadFileText(fileToBind);
                    if (v == null) continue;
                    host.DataStrings[kvp.Key] = v;
                }
                catch (IOException)
                {
                }
            }
        }

        // Verify files and return result.
        if (verifyOptions != LocalWebAppVerificationOptions.Disabled)
        {
            var hostBinding = manifest.HostBinding;
            if (hostBinding != null && hostBinding.Count > 0)
            {
                var verifiedHost = false;
                foreach (var bindingInfo in hostBinding)
                {
                    if (bindingInfo?.HostId != options.HostId) continue;
                    if (!string.IsNullOrWhiteSpace(bindingInfo.MinimumVersion))
                        if (VersionComparer.Compare(bindingInfo.MinimumVersion, manifest.Version, false) > 0) continue;
                    if (!string.IsNullOrWhiteSpace(bindingInfo.MaximumVersion))
                        if (VersionComparer.Compare(bindingInfo.MaximumVersion, manifest.Version, false) < 0) continue;
                    verifiedHost = true;
                    break;
                }

                if (!verifiedHost) return null;
            }

            host.IsVerified = await VerifyAsync(host, manifestFileName, verifyOptions == LocalWebAppVerificationOptions.SkipException, cancellationToken);
        }

        // Return result.
        return host;
    }

    /// <summary>
    /// Tests if the files are verified by signature.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>true if verified; otherwise, false.</returns>
    public async Task<bool> VerifyAsync(CancellationToken cancellationToken = default)
    {
        var state = await VerifyAsync(this, Options?.ManifestFileName, true, cancellationToken);
        IsVerified = state;
        return state;
    }

    /// <summary>
    /// Computes the signature for files.
    /// </summary>
    /// <param name="signatureProvider">The signature provider with private key.</param>
    /// <param name="output">true if write file; otherwise, false.</param>
    /// <returns>The file collection.</returns>
    /// <exception cref="ArgumentNullException">The directory or signatureProvider was null.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory was not found.</exception>
    public LocalWebAppFileCollection Sign(ISignatureProvider signatureProvider, bool output = true)
    {
        string fileName = null;
        if (output)
        {
            fileName = Options?.ManifestFileName?.Trim();
            if (string.IsNullOrEmpty(fileName)) fileName = UI.LocalWebAppExtensions.DefaultManifestFileName;
            fileName = GetSubFileName(fileName, "files");
        }

        var col = Sign(ResourcePackageDirectory, signatureProvider, fileName);
        if (col != null) IsVerified = true;
        return col;
    }

    /// <summary>
    /// Computes the signature for files.
    /// </summary>
    /// <param name="signatureProvider">The signature provider with private key.</param>
    /// <param name="hasWritenFile">true if write file succeeded; otherwise, false.</param>
    /// <returns>The file collection.</returns>
    /// <exception cref="ArgumentNullException">The directory or signatureProvider was null.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory was not found.</exception>
    public LocalWebAppFileCollection Sign(ISignatureProvider signatureProvider, out bool hasWritenFile)
    {
        var fileName = Options?.ManifestFileName?.Trim();
        if (string.IsNullOrEmpty(fileName)) fileName = UI.LocalWebAppExtensions.DefaultManifestFileName;
        fileName = GetSubFileName(fileName, "files");
        var col = Sign(ResourcePackageDirectory, signatureProvider, fileName, out hasWritenFile);
        if (col != null) IsVerified = true;
        return col;
    }

    /// <summary>
    /// Creates a package.
    /// </summary>
    /// <param name="outputFileName">The file name of the zip.</param>
    /// <returns>The file output.</returns>
    public FileInfo Package(string outputFileName = null)
    {
        if (!IsVerified) return null;
        if (string.IsNullOrWhiteSpace(outputFileName))
            outputFileName = string.Concat(ResourcePackageId, '.', Manifest?.Version ?? "0", ".zip");
        if (!outputFileName.Contains('\\') && !outputFileName.Contains('/'))
            outputFileName = Path.Combine((ResourcePackageDirectory.Parent ?? ResourcePackageDirectory).FullName, outputFileName);
        File.Delete(outputFileName);
        ZipFile.CreateFromDirectory(ResourcePackageDirectory.FullName, outputFileName);
        return FileSystemInfoUtility.TryGetFileInfo(outputFileName);
    }

    /// <summary>
    /// Computes the signature for files.
    /// </summary>
    /// <param name="dir">The app directory.</param>
    /// <param name="signatureProvider">The signature provider with private key.</param>
    /// <param name="outputFileName">The file name to output.</param>
    /// <returns>The file collection.</returns>
    /// <exception cref="ArgumentNullException">The directory or signatureProvider was null.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory was not found.</exception>
    public static LocalWebAppFileCollection Sign(DirectoryInfo dir, ISignatureProvider signatureProvider, string outputFileName = null)
        => Sign(dir, signatureProvider, outputFileName, out _);

    /// <summary>
    /// Computes the signature for files.
    /// </summary>
    /// <param name="dir">The app directory.</param>
    /// <param name="signatureProvider">The signature provider with private key.</param>
    /// <param name="outputFileName">The file name to output.</param>
    /// <param name="hasWritenFile">true if write file succeeded; otherwise, false.</param>
    /// <returns>The file collection.</returns>
    /// <exception cref="ArgumentNullException">The directory or signatureProvider was null.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory was not found.</exception>
    public static LocalWebAppFileCollection Sign(DirectoryInfo dir, ISignatureProvider signatureProvider, string outputFileName, out bool hasWritenFile)
    {
        if (dir == null) throw new ArgumentNullException(nameof(dir));
        if (signatureProvider == null) throw new ArgumentNullException(nameof(signatureProvider));
        if (!dir.Exists) throw new DirectoryNotFoundException("dir is not found");
        var files = GetSourceFiles(dir).ToList();
        var collection = new LocalWebAppFileCollection
        {
            Files = new()
        };
        var manifestPath = Path.Combine(dir.FullName, UI.LocalWebAppExtensions.DefaultManifestFileName);
        var manifest = FileSystemInfoUtility.TryGetFileInfo(manifestPath);
        files.Insert(0, manifest);
        foreach (var file in files)
        {
            try
            {
                using var stream = file.OpenRead();
                if (stream == null) continue;
                var sign = signatureProvider.Sign(stream);
                if (sign == null) continue;
                var parent = file.Directory;
                var path = file.Name;
                while (parent != null && parent != dir && parent.FullName != dir.FullName)
                {
                    path = Path.Combine(parent.Name, path);
                    if (parent == parent.Parent) break;
                    parent = parent.Parent;
                }

                var item = new LocalWebAppFileInfo
                {
                    Signature = WebFormat.Base64UrlEncode(sign),
                    Path = path
                };
                collection.Files.Add(item);
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
            catch (ExternalException)
            {
            }
        }

        var written = false;
        if (!string.IsNullOrWhiteSpace(outputFileName))
        {
            string s = null;
            try
            {
                if (!outputFileName.Contains('\\') && !outputFileName.Contains('/'))
                    outputFileName = Path.Combine(dir.FullName, outputFileName);
                s = JsonSerializer.Serialize(collection);
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
            catch (ExternalException)
            {
            }

            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    File.WriteAllText(outputFileName, s, Encoding.UTF8);
                    written = true;
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (IOException)
                {
                    try
                    {
                        File.Delete(outputFileName);
                        File.WriteAllText(outputFileName, s, Encoding.UTF8);
                        written = true;
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
                    catch (ExternalException)
                    {
                    }
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
                catch (ExternalException)
                {
                }
            }
        }

        hasWritenFile = written;
        return collection;
    }

    /// <summary>
    /// Computes the signature for files.
    /// </summary>
    /// <param name="dir">The app directory.</param>
    /// <param name="options">The options of the local standalone web app.</param>
    /// <param name="signatureProvider">The signature provider with private key.</param>
    /// <returns>The file collection.</returns>
    /// <exception cref="ArgumentNullException">The directory was null.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory was not found.</exception>
    public static LocalWebAppFileCollection Sign(DirectoryInfo dir, LocalWebAppOptions options, ISignatureProvider signatureProvider)
    {
        var fileName = options?.ManifestFileName;
        if (string.IsNullOrWhiteSpace(fileName)) fileName = UI.LocalWebAppExtensions.DefaultManifestFileName;
        fileName = GetSubFileName(fileName, "files");
        try
        {
            if (signatureProvider == null && options != null)
                signatureProvider = options.GetPublicKey(string.Empty);
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }

        return Sign(dir, signatureProvider, fileName);
    }

    /// <summary>
    /// Computes the signature for files.
    /// </summary>
    /// <param name="dir">The app directory.</param>
    /// <param name="options">The options of the local standalone web app.</param>
    /// <param name="signKey">The key of file signature mapping.</param>
    /// <returns>The file collection.</returns>
    /// <exception cref="ArgumentNullException">The directory was null.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory was not found.</exception>
    public static LocalWebAppFileCollection Sign(DirectoryInfo dir, LocalWebAppOptions options, string signKey = null)
    {
        ISignatureProvider signatureProvider = null;
        try
        {
            signatureProvider = options?.GetPublicKey(signKey ?? string.Empty);
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }

        return Sign(dir, options, signatureProvider);
    }

    /// <summary>
    /// Creates a package.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="signatureProvider">The signature provider with private key.</param>
    /// <param name="outputFileName">The file name of the zip.</param>
    /// <returns>The file output.</returns>
    public static FileInfo Package(DirectoryInfo dir, ISignatureProvider signatureProvider, string outputFileName = null)
    {
        Sign(dir, signatureProvider, UI.LocalWebAppExtensions.DefaultManifestGeneratedFileName);
        if (string.IsNullOrWhiteSpace(outputFileName))
            outputFileName = string.Concat(dir.FullName, ".zip");
        if (!outputFileName.Contains('\\') && !outputFileName.Contains('/'))
            outputFileName = Path.Combine((dir.Parent ?? dir).FullName, outputFileName);
        File.Delete(outputFileName);
        ZipFile.CreateFromDirectory(dir.FullName, outputFileName);
        return FileSystemInfoUtility.TryGetFileInfo(outputFileName);
    }

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="version">The expected version.</param>
    /// <param name="zip">The zip file.</param>
    /// <param name="deleteZip">true if delete the zip file after extracting.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version.</returns>
    public async Task<string> UpdateAsync(string version, FileInfo zip, bool deleteZip, CancellationToken cancellationToken = default)
    {
        if (zip == null) return null;

        // Extract zip.
        string path;
        try
        {
            path = Path.Combine(CacheDirectory.FullName, "TempResourcePackage");
            TryDeleteDirectory(path);
            ZipFile.ExtractToDirectory(zip.FullName, path);
        }
        catch (ArgumentException)
        {
            path = null;
        }
        catch (InvalidOperationException)
        {
            path = null;
        }
        catch (IOException)
        {
            path = null;
        }
        catch (FormatException)
        {
            path = null;
        }
        catch (SecurityException)
        {
            path = null;
        }
        catch (UnauthorizedAccessException)
        {
            path = null;
        }
        catch (NotSupportedException)
        {
            path = null;
        }
        catch (ExternalException)
        {
            path = null;
        }

        try
        {
            if (deleteZip) zip.Delete();
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

        if (path == null) return null;

        // Test package.
        var root = CacheDirectory.Parent;
        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(path);
        if (dir == null || !dir.Exists) return null;
        try
        {
            var host = await LoadAsync(root, Options, false, dir, cancellationToken);
            if (host?.Manifest?.Version != version || host?.ResourcePackageId != ResourcePackageId)
            {
                dir.Delete(true);
                return null;
            }

            path = Path.Combine(root.FullName, string.Concat('v', version));
        }
        catch (ArgumentException)
        {
            path = null;
        }
        catch (InvalidOperationException)
        {
            path = null;
        }
        catch (IOException)
        {
            path = null;
        }
        catch (FormatException)
        {
            path = null;
        }
        catch (SecurityException)
        {
            path = null;
        }
        catch (UnauthorizedAccessException)
        {
            path = null;
        }
        catch (NotSupportedException)
        {
            path = null;
        }
        catch (ExternalException)
        {
            path = null;
        }

        if (path == null) return null;

        // Enable new version.
        TryDeleteDirectory(path);
        try
        {
            CopyTo(dir, path);
        }
        catch (SecurityException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        finally
        {
            try
            {
                TryDeleteDirectory(dir.FullName);
            }
            catch (SecurityException)
            {
            }
        }

        var settings = await TryGetSettingsAsync(CacheDirectory, cancellationToken) ?? new JsonObjectNode();
        var oldVersion = settings.TryGetStringValue("version")?.Trim();
        var installInfo = settings.TryGetObjectValue("install") ?? new();
        var oldVersion2 = installInfo.TryGetStringValue("oldVersion")?.Trim();
        settings.SetValue("version", version);
        installInfo.SetValue("old", oldVersion);
        installInfo.SetValue("done", DateTime.Now);
        settings.SetValue("install", installInfo);
        await TrySaveSettingsAsync(CacheDirectory, settings, cancellationToken);

        // Clean up.
        if (!string.IsNullOrEmpty(oldVersion2))
        {
            path = Path.Combine(root.FullName, string.Concat('v', oldVersion2));
            TryDeleteDirectory(path);
        }

        // Return result.
        return version;
    }

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version.</returns>
    public async Task<string> UpdateAsync(CancellationToken cancellationToken = default)
    {
        // Get update manifest.
        var url = GetUrl(Options.Update?.Url, Options.Update?.VariableParameters);
        if (string.IsNullOrEmpty(url)) return null;
        var http = new JsonHttpClient<JsonObjectNode>();
        var resp = await http.GetAsync(url, cancellationToken);
        var respProp = Options.Update?.ResponseProperty?.Trim();
        resp = resp?.TryGetObjectValue(string.IsNullOrEmpty(respProp) ? "data" : respProp) ?? resp;
        if (resp == null) return null;
        var ver = resp.TryGetStringValue("version")?.Trim();
        if (string.IsNullOrEmpty(ver)) return null;
        if (!string.IsNullOrWhiteSpace(Manifest.Version) && VersionComparer.Compare(ver, Manifest.Version, true) <= 0 && resp.TryGetBooleanValue("force") != true)
            return null;
        
        // Download zip.
        url = GetUrl(resp.TryGetStringValue("url"), resp.TryGetObjectValue("params"));
        var uri = UI.VisualUtility.TryCreateUri(url);
        if (uri == null) return null;
        FileInfo zip = null;
        try
        {
            zip = await HttpClientExtensions.WriteFileAsync(uri, Path.Combine(CacheDirectory.FullName, "TempResourcePackage.zip"), null, cancellationToken);
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
        catch (FormatException)
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
        catch (ExternalException)
        {
        }

        // Continue and return result.
        if (zip == null) return null;
        return await UpdateAsync(ver, zip, true, cancellationToken);
    }

    private string GetResourcePackageParentPath(string localRelativePath)
    {
        var host = ResourcePackageDirectory?.Parent?.FullName;
        return string.IsNullOrEmpty(host) ? null : Path.Combine(host, localRelativePath.TrimStart('.'));
    }

    private static DirectoryInfo CopyTo(DirectoryInfo source, string destPath)
    {
        Directory.CreateDirectory(destPath);
        FileInfo[] files = source.GetFiles();
        foreach (FileInfo fileInfo in files)
        {
            fileInfo.CopyTo(Path.Combine(destPath, fileInfo.Name), overwrite: true);
        }

        DirectoryInfo[] directories = source.GetDirectories();
        foreach (DirectoryInfo directoryInfo in directories)
        {
            source = directoryInfo;
            CopyTo(directoryInfo, Path.Combine(destPath, directoryInfo.Name));
        }

        return new DirectoryInfo(destPath);
    }

    private static string GetSubFileName(string name, string sub)
    {
        var i = name.LastIndexOf('.');
        var ext = name[i..];
        return string.Concat(name[..i], '.', sub, ext);
    }

    private static IEnumerable<FileInfo> GetSourceFiles(DirectoryInfo dir)
        => dir.EnumerateFiles("*.html", SearchOption.AllDirectories)
            .Union(dir.EnumerateFiles("*.htm", SearchOption.AllDirectories))
            .Union(dir.EnumerateFiles("*.js", SearchOption.AllDirectories))
            .Union(dir.EnumerateFiles("*.ts", SearchOption.AllDirectories))
            .Union(dir.EnumerateFiles("*.css", SearchOption.AllDirectories));
            //.Union(dir.EnumerateFiles("*.json", SearchOption.AllDirectories));

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
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
        catch (ExternalException)
        {
        }
    }

    private string GetUrl(string url, Dictionary<string, string> parameters)
    {
        url = url?.Trim();
        if (string.IsNullOrEmpty(url)) return null;
        if (parameters == null) return url;
        var q = new QueryData();
        foreach (var kvp in parameters)
        {
            var k = kvp.Key?.Trim();
            var v = kvp.Value?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(k) || string.IsNullOrEmpty(v)) continue;
            switch (v)
            {
                case "version":
                    q.Add(k, Manifest?.Version);
                    break;
                case "id":
                    q.Add(k, ResourcePackageId);
                    break;
                case "kind":
                    q.Add(k, "WindowsAppSdk");
                    break;
                case "guid":
                    q.Add(k, Guid.NewGuid().ToString());
                    break;
            }
        }

        return q.ToString(url);
    }

    private string GetUrl(string url, JsonObjectNode parameters)
    {
        url = url?.Trim();
        if (string.IsNullOrEmpty(url)) return null;
        if (parameters == null) return url;
        var q = new Dictionary<string, string>();
        foreach (var kvp in parameters)
        {
            if (kvp.Value?.ValueKind != JsonValueKind.String || kvp.Value is not IJsonStringNode s) continue;
            q[kvp.Key] = s.StringValue;
        }

        return GetUrl(url, q);
    }

    private static async Task<bool> VerifyAsync(LocalWebAppHost host, string manifestFileName, bool skipVerificationException, CancellationToken cancellationToken = default)
    {
        try
        {
            var appDir = host.ResourcePackageDirectory;
            manifestFileName = GetSubFileName(manifestFileName, "files");
            var file = appDir.EnumerateFiles(manifestFileName).FirstOrDefault();
            if (file == null || !file.Exists)
            {
                if (skipVerificationException) return false;
                throw new SecurityException("The file signature list is not found.", new FileNotFoundException("The file signature list is not found."));
            }

            using var stream = file.OpenRead();
            var fileCol = await JsonSerializer.DeserializeAsync<LocalWebAppFileCollection>(stream, null as JsonSerializerOptions, cancellationToken);
            if (fileCol.Files == null)
            {
                if (skipVerificationException) return false;
                throw new SecurityException("Miss file signature information.");
            }

            try
            {
                host.SignatureProvider = host.Options?.GetPublicKey(fileCol.SignKey ?? string.Empty);
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }

            var files = GetSourceFiles(appDir).Select(ele => ele.FullName).ToList();
            foreach (var item in fileCol.Files)
            {
                if (!item.Verify(host, out var fileInfo))
                {
                    if (fileInfo == null || !fileInfo.Exists) continue;
                    if (skipVerificationException) return false;
                    throw new SecurityException("Incorrect signature.");
                }

                if (fileInfo?.FullName != null) files.Remove(fileInfo.FullName);
            }

            if (files.Count > 0)
            {
                if (skipVerificationException) return false;
                throw new SecurityException(files.Count == 1 ? "Miss signature for a file." : $"Miss signature for {files.Count} files.");
            }

            return true;
        }
        catch (InvalidOperationException ex)
        {
            if (skipVerificationException) return false;
            throw new SecurityException(string.Concat("Invalid operation during file signature verification. ", ex.Message), ex);
        }
        catch (NotSupportedException ex)
        {
            if (skipVerificationException) return false;
            throw new SecurityException(string.Concat("Not supported during file signature verification.", ex.Message), ex);
        }
        catch (IOException ex)
        {
            if (skipVerificationException) return false;
            throw new SecurityException(string.Concat("IO exception during file signature verification.", ex.Message), ex);
        }
        catch (ExternalException ex)
        {
            if (skipVerificationException) return false;
            throw new SecurityException(string.Concat("External exception during file signature verification.", ex.Message), ex);
        }
    }

    private static async Task<JsonObjectNode> TryGetSettingsAsync(DirectoryInfo cacheDir, CancellationToken cancellationToken = default)
    {
        var settingsFile = cacheDir?.GetFiles("settings.json")?.FirstOrDefault();
        if (settingsFile == null || !settingsFile.Exists) return null;
        try
        {
            using var stream = settingsFile.OpenRead();
            return await JsonObjectNode.ParseAsync(stream, cancellationToken);
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

        return null;
    }

    private static async Task<bool> TrySaveSettingsAsync(DirectoryInfo cacheDir, JsonObjectNode json, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(cacheDir.FullName, "settings.json");
        try
        {
            await File.WriteAllTextAsync(path, json?.ToString(IndentStyles.Compact) ?? "null", Encoding.UTF8, cancellationToken);
            return true;
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

        return false;
    }
}
