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
            if (appDomainArr.Length > 1) appDomain = string.Concat(appDomainArr[1].Replace("@", string.Empty), ".", appDomainArr[0]);
            else appDomain = appDomainArr.FirstOrDefault();
        }

        if (string.IsNullOrEmpty(appDomain)) appDomain = "privateapp";
        VirtualHost = options?.CustomizedVirtualHost ?? LocalWebAppHook.VirtualHostGenerator?.Invoke(manifest, options);
        if (string.IsNullOrWhiteSpace(VirtualHost)) VirtualHost = string.Concat(appDomain, '.', UI.LocalWebAppExtensions.VirtualRootDomain);
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
    /// Gets the available new version to update.
    /// </summary>
    public string NewVersionAvailable { get; private set; }

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
        else if (localRelativePath.StartsWith(".doc:")) return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), localRelativePath[6..]);
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
    /// Sets identifier of the host app.
    /// </summary>
    /// <param name="id">The identifier of the host app.</param>
    public static void SetHostId(string id)
        => LocalWebAppHook.HostId = id;

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="package">The package.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">package was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static Task<LocalWebAppHost> LoadAsync(LocalWebAppPackageResult package, bool skipVerificationException = false, CancellationToken cancellationToken = default)
    {
        if (package == null) throw new ArgumentNullException(nameof(package), "package should not be null.");
        var dir = package.RootDirectory;
        if (dir == null || string.IsNullOrWhiteSpace(LocalWebAppHook.HostId)) throw new InvalidOperationException("The options is not correct.");
        var path = package.ProjectConfiguration?.TryGetObjectValue("dev", "proc")?.TryGetStringValue("path")?.Trim();
        if (!string.IsNullOrEmpty(path)) dir = GetDirectoryInfoByRelative(dir, path);
        if (!dir.Exists) Directory.CreateDirectory(dir.FullName);
        return LoadAsync(dir, package.Options, skipVerificationException, package.AppDirectory, cancellationToken);
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static Task<LocalWebAppHost> LoadAsync(LocalWebAppOptions options, bool skipVerificationException = false, CancellationToken cancellationToken = default)
        => LoadAsync(options, null, null, false, skipVerificationException, cancellationToken);

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="assembly">The assembly which embed the resource package.</param>
    /// <param name="forceToLoad">true if force to load the resource; otherwise, false.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static Task<LocalWebAppHost> LoadAsync(System.Reflection.Assembly assembly, bool forceToLoad = false, bool skipVerificationException = false, CancellationToken cancellationToken = default)
        => LoadAsync(assembly, null, null, forceToLoad, null, skipVerificationException, cancellationToken);

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="assembly">The assembly which embed the resource package.</param>
    /// <param name="projectFileName">The config file name of the resource package project.</param>
    /// <param name="packageFileName">The zip file name of the embedded resource package.</param>
    /// <param name="forceToLoad">true if force to load the resource; otherwise, false.</param>
    /// <param name="pemFileName">The public key file name of signature.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="NotSupportedException">The signature algorithm was not supported.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(System.Reflection.Assembly assembly, string projectFileName, string packageFileName, bool forceToLoad = false, string pemFileName = null, bool skipVerificationException = false, CancellationToken cancellationToken = default)
    {
        if (assembly == null) assembly = System.Reflection.Assembly.GetEntryAssembly();
        if (string.IsNullOrWhiteSpace(projectFileName)) projectFileName = GetEmbeddedFileName(GetSubFileName(UI.LocalWebAppExtensions.DefaultManifestFileName, "project"), assembly);
        if (string.IsNullOrWhiteSpace(packageFileName)) packageFileName = GetEmbeddedFileName(GetSubFileName(UI.LocalWebAppExtensions.DefaultManifestFileName, null, ".zip"), assembly);
        if (string.IsNullOrWhiteSpace(pemFileName)) pemFileName = GetEmbeddedFileName(GetSubFileName(UI.LocalWebAppExtensions.DefaultManifestFileName, null, ".pem"), assembly);
        using var stream = string.IsNullOrEmpty(projectFileName) ? null : assembly.GetManifestResourceStream(projectFileName);
        var config = JsonObjectNode.Parse(stream);
        var config2 = config?.TryGetObjectValue("ref");
        if (config2 == null) throw new InvalidOperationException("Load project config failed.");
        var key = config2.TryGetStringValue("key");
        if (string.IsNullOrWhiteSpace(key))
        {
            if (string.IsNullOrWhiteSpace(pemFileName)) throw new InvalidOperationException("Miss public key of signature.");
            using var stream2 = assembly.GetManifestResourceStream(pemFileName);
            using var reader = new StreamReader(stream2);
            key = reader.ReadToEnd();
            config2.SetValue("key", key);
        }

        var options = LoadOptions(config, null);
        return await LoadAsync(options, assembly, packageFileName, forceToLoad, skipVerificationException, cancellationToken);
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <param name="assembly">The assembly which embed the resource package.</param>
    /// <param name="fileName">The zip file name of the embedded resource package.</param>
    /// <param name="forceToLoad">true if force to load the resource; otherwise, false.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(LocalWebAppOptions options, System.Reflection.Assembly assembly, string fileName, bool forceToLoad = false, bool skipVerificationException = false, CancellationToken cancellationToken = default)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrEmpty(options.ResourcePackageId)) throw new InvalidOperationException("The resource package identifier should not be null or empty.");
        var appDataFolder = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("LocalWebApp", Windows.Storage.CreationCollisionOption.OpenIfExists);
        var appId = FormatResourcePackageId(options.ResourcePackageId);
        appDataFolder = await appDataFolder.CreateFolderAsync(appId, Windows.Storage.CreationCollisionOption.OpenIfExists);
        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(appDataFolder.Path);
        if (assembly != null && !string.IsNullOrWhiteSpace(fileName))
        {
            if (!forceToLoad)
            {
                var folders = dir.EnumerateDirectories("v*.*");
                forceToLoad = true;
                foreach (var folder in folders)
                {
                    try
                    {
                        if (folder.EnumerateFiles("*.json").Any())
                        {
                            forceToLoad = false;
                            break;
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (SecurityException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (ExternalException)
                    {
                    }
                }

                try
                {
                    if (forceToLoad)
                    {
                        var folder = dir.EnumerateDirectories("app").FirstOrDefault();
                        if (folder != null && folder.EnumerateFiles("*.json").Any()) forceToLoad = false;
                    }
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (ExternalException)
                {
                }
            }

            if (forceToLoad) await LoadCompressedResourceAsync(options, dir, assembly, fileName, cancellationToken);
        }

        var host = await LoadAsync(dir, options, skipVerificationException, null, cancellationToken);
        _ = host.UpdateRegisteredAsync();
        return host;
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier registered.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(string resourcePackageId, bool skipVerificationException = false, CancellationToken cancellationToken = default)
    {
        var info = await GetPackageAsync(resourcePackageId);
        return await LoadAsync(info.GetOptions(), skipVerificationException, cancellationToken);
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="rootDir">The root directory for local standalone web app.</param>
    /// <param name="options">The options to parse.</param>
    /// <param name="skipVerificationException">true if don't throw exception on verification failure; otherwise, false.</param>
    /// <param name="appDir">The optional resource package directory to load the local standalone web app.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
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
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
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
            appDir = rootDir.EnumerateDirectories("app").FirstOrDefault();
        if (appDir == null || !appDir.Exists)
        {
            string errorMessage = null;
            try
            {
                if (!string.IsNullOrEmpty(appDir?.FullName))
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
        using var stream = file.OpenRead();
        var manifest = await JsonSerializer.DeserializeAsync<LocalWebAppManifest>(stream, null as JsonSerializerOptions, cancellationToken);
        if (string.IsNullOrWhiteSpace(manifest?.Id)) throw new FormatException("The format of the package manifest is not correct.");
        if (manifest.Id.Trim().ToLowerInvariant() != options.ResourcePackageId?.Trim()?.ToLowerInvariant())
            throw new InvalidOperationException("The app is not the specific one.");
        var host = new LocalWebAppHost(manifest, options)
        {
            ResourcePackageDirectory = appDir,
            DataDirectory = dataDir,
            CacheDirectory = cacheDir,
        };

        // Test the host app binding information.
        if (verifyOptions != LocalWebAppVerificationOptions.Disabled)
        {
            var hostBinding = manifest.HostBinding;
            if (hostBinding != null && hostBinding.Count > 0)
            {
                var verifiedHost = false;
                foreach (var bindingInfo in hostBinding)
                {
                    if (bindingInfo?.HostId != LocalWebAppHook.HostId) continue;
                    if (!string.IsNullOrWhiteSpace(bindingInfo.MinimumVersion))
                        if (VersionComparer.Compare(bindingInfo.MinimumVersion, manifest.Version, false) > 0) continue;
                    if (!string.IsNullOrWhiteSpace(bindingInfo.MaximumVersion))
                        if (VersionComparer.Compare(bindingInfo.MaximumVersion, manifest.Version, false) < 0) continue;
                    verifiedHost = true;
                    break;
                }

                if (!verifiedHost)
                    throw new InvalidOperationException("Does not match the current host app.");
            }
        }

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
            host.IsVerified = await VerifyAsync(host, manifestFileName, verifyOptions == LocalWebAppVerificationOptions.SkipException, cancellationToken);

        // Return result.
        return host;
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <param name="uri">The URI to download the compressed file of the resource package.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(LocalWebAppOptions options, Uri uri, CancellationToken cancellationToken = default)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        if (string.IsNullOrEmpty(options.ResourcePackageId)) throw new InvalidOperationException("The resource package identifier should not be null or empty.");
        var appDataFolder = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("LocalWebApp", Windows.Storage.CreationCollisionOption.OpenIfExists);
        var appId = FormatResourcePackageId(options.ResourcePackageId);
        appDataFolder = await appDataFolder.CreateFolderAsync(appId, Windows.Storage.CreationCollisionOption.OpenIfExists);
        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(appDataFolder.Path);
        var cacheDir = Directory.CreateDirectory(Path.Combine(dir.FullName, "cache"));
        var path = Path.Combine(cacheDir.FullName, "TempResourcePackage.zip");
        TryDeleteDirectory(path);
        try
        {
            await HttpClientExtensions.WriteFileAsync(uri, path, null, cancellationToken);
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

        await UpdateAsync(options, dir, null, FileSystemInfoUtility.TryGetFileInfo(path), true, cancellationToken);
        var host = await LoadAsync(dir, options, true, null, cancellationToken);
        if (host.ResourcePackageId != options.ResourcePackageId) throw new InvalidOperationException("The app is not the expect one.");
        await RegisterPackageAsync(new LocalWebAppInfo(host));
        return host;
    }

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="info">The resource package information.</param>
    /// <param name="uri">The URI to download the compressed file of the resource package.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">info or uri was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(LocalWebAppInfo info, Uri uri, CancellationToken cancellationToken = default)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        if (string.IsNullOrWhiteSpace(info.ResourcePackageId)) throw new ArgumentException("The resource package identifier should not be empty.", nameof(info));
        return await LoadAsync(info?.GetOptions(), uri, cancellationToken);
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
                signatureProvider = options.GetSignatureProvider();
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
    /// <returns>The file collection.</returns>
    /// <exception cref="ArgumentNullException">The directory was null.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory was not found.</exception>
    public static LocalWebAppFileCollection Sign(DirectoryInfo dir, LocalWebAppOptions options)
    {
        ISignatureProvider signatureProvider = null;
        try
        {
            signatureProvider = options?.GetSignatureProvider();
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
    /// Creates the app options.
    /// </summary>
    /// <param name="dir">The app path.</param>
    /// <returns>The file output.</returns>
    /// <exception cref="DirectoryNotFoundException">dir was not found.</exception>
    /// <exception cref="FileNotFoundException">The private key was not found.</exception>
    /// <exception cref="InvalidOperationException">The configuration is not valid.</exception>
    /// <exception cref="NotSupportedException">The signature algorithm was not supported.</exception>
    public static LocalWebAppOptions LoadOptions(DirectoryInfo dir)
    {
        if (dir == null) throw new DirectoryNotFoundException("The root directory is not found.");
        var config = LoadBuildConfig(dir, out _);
        if (config == null) throw new InvalidOperationException("Parse the config file failed.");
        var keyFile = dir.EnumerateFiles(GetSubFileName(UI.LocalWebAppExtensions.DefaultManifestFileName, "private", ".pem"))?.FirstOrDefault();
        if (keyFile == null) throw new FileNotFoundException("The private key does not exist.");
        return LoadOptions(config, keyFile);
    }

    /// <summary>
    /// Creates a package.
    /// </summary>
    /// <param name="dir">The app path.</param>
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
    /// Creates a package.
    /// </summary>
    /// <param name="dir">The app path.</param>
    /// <param name="outputFileName">The file name of the zip.</param>
    /// <returns>The file output.</returns>
    /// <exception cref="DirectoryNotFoundException">dir was not found.</exception>
    /// <exception cref="DirectoryNotFoundException">The private key was not found.</exception>
    /// <exception cref="InvalidOperationException">The configuration is not valid.</exception>
    /// <exception cref="NotSupportedException">The signature algorithm was not supported.</exception>
    /// <exception cref="IOException">IO exception.</exception>
    /// <exception cref="SecurityException">The security exception during file access.</exception>
    /// <exception cref="UnauthorizedAccessException">One or more files are unauthorized to access.</exception>
    public static LocalWebAppPackageResult Package(DirectoryInfo dir, string outputFileName = null)
    {
        // Load options.
        if (dir == null) throw new DirectoryNotFoundException("The root directory is not found.");
        JsonObjectNode config = null;
        FileInfo configFile = null;
        try
        {
            config = LoadBuildConfig(dir, out configFile);
        }
        catch (IOException)
        {
        }

        if (config == null && dir != null)
        {
            dir = dir.EnumerateDirectories("localwebapp").FirstOrDefault();
            if (dir != null) config = LoadBuildConfig(dir, out configFile);
        }

        if (config == null || configFile == null) throw new InvalidOperationException("Parse the config file failed.");
        var keyFile = dir.EnumerateFiles(GetSubFileName(UI.LocalWebAppExtensions.DefaultManifestFileName, "private", ".pem"))?.FirstOrDefault();
        if (keyFile == null) throw new FileNotFoundException("The private key does not exist.");
        var options = LoadOptions(config, keyFile);
        var refConfig = config.TryGetObjectValue("ref");

        // Create manifest.
        var appDir = GetDirectoryInfoByRelative(dir, refConfig.TryGetStringValue("path")?.Trim()) ?? dir;
        var manifestPath = Path.Combine(appDir.FullName, UI.LocalWebAppExtensions.DefaultManifestFileName);
        var manifestJson = config.TryGetObjectValue("package") ?? config.TryGetObjectValue("manifest");
        if (manifestJson != null)
        {
            var packageId = config.TryGetStringValue("id")?.Trim();
            if (!string.IsNullOrEmpty(packageId)) manifestJson.SetValue("id", packageId);
            var manifestStr = manifestJson.ToString();
            File.WriteAllText(manifestPath, manifestStr);
        }

        // Sign.
        Sign(appDir, options);

        // Load.
        if (string.IsNullOrWhiteSpace(outputFileName))
            outputFileName = Path.Combine(dir.FullName, GetSubFileName(UI.LocalWebAppExtensions.DefaultManifestFileName, null, ".zip"));
        if (!outputFileName.Contains('\\') && !outputFileName.Contains('/'))
            outputFileName = Path.Combine(dir.FullName, outputFileName);
        File.Delete(outputFileName);
        ZipFile.CreateFromDirectory(appDir.FullName, outputFileName);
        var zip = FileSystemInfoUtility.TryGetFileInfo(outputFileName);

        // Copy
        var arr = refConfig.TryGetArrayValue("output")?.OfType<JsonObjectNode>();
        if (arr != null)
        {
            foreach (var output in arr)
            {
                if (output == null) continue;
                var outputPath = GetFileInfoByRelative(dir, output.TryGetStringValue("zip"));
                if (!string.IsNullOrWhiteSpace(outputPath?.FullName) && outputPath.Exists && zip.FullName != outputPath.FullName) zip.CopyTo(outputPath.FullName, true);
                outputPath = GetFileInfoByRelative(dir, output.TryGetStringValue("config"));
                if (!string.IsNullOrWhiteSpace(outputPath?.FullName) && outputPath.Exists && configFile.FullName != outputPath.FullName) configFile.CopyTo(outputPath.FullName, true);
            }
        }

        // Return result.
        var result = new LocalWebAppPackageResult(options, dir, appDir, zip, config.TryGetObjectValue("details"), config.TryGetObjectValue("project"));
        LocalWebAppHook.BuildDevPackage?.Invoke(result);
        return result;
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
        NewVersionAvailable = await UpdateAsync(Options, CacheDirectory.Parent, version, zip, deleteZip, cancellationToken);
        LocalWebAppHook.OnUpdate?.Invoke(this);
        return NewVersionAvailable;
    }

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version; or null, if no update.</returns>
    public async Task<string> UpdateAsync(CancellationToken cancellationToken = default)
    {
        // Get update manifest.
        var url = GetUrl(Options.Update?.Url, Options.Update?.VariableParameters);
        if (string.IsNullOrEmpty(url)) return null;
        var http = new JsonHttpClient<JsonObjectNode>
        {
            SerializeEvenIfFailed = true
        };
        LocalWebAppHook.UpdateServiceClientHandler?.Invoke(http);
        var resp = await http.GetAsync(url, cancellationToken);
        var respProp = Options.Update?.ResponseProperty?.Trim();
        resp = resp?.TryGetObjectValue(string.IsNullOrEmpty(respProp) ? "data" : respProp) ?? resp;
        if (resp == null && !string.IsNullOrWhiteSpace(Manifest?.Id))
        {
            var respArr = resp?.TryGetArrayValue(string.IsNullOrEmpty(respProp) ? "data" : respProp)?.OfType<JsonObjectNode>();
            var manifestId = Manifest.Id.Trim().ToUpperInvariant().Replace("@", string.Empty).Replace("\\", "/");
            foreach (var respItem in respArr)
            {
                if (respItem?.TryGetStringValue("id")?.Trim()?.ToUpperInvariant()?.Replace("@", string.Empty)?.Replace("\\", "/") != manifestId) continue;
                resp = respItem;
                break;
            }
        }

        if (resp == null) return null;
        var ver = resp.TryGetStringValue("version")?.Trim() ?? resp.TryGetStringValue("latestVersion")?.Trim();
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

    /// <summary>
    /// Gets the specific resource package information registered.
    /// </summary>
    /// <param name="id">The resource package identifier.</param>
    /// <param name="dev">true if list dev apps; otherwise, false.</param>
    /// <returns>The resource package information instance.</returns>
    public static Task<LocalWebAppInfo> GetPackageAsync(string id, bool dev = false)
        => UpdatePackageAsync(id, null, dev);

    /// <summary>
    /// Lists all resource packages registered.
    /// </summary>
    /// <param name="dev">true if list dev apps; otherwise, false.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>The list of the resource packages.</returns>
    public static async Task<List<LocalWebAppInfo>> ListPackageAsync(bool dev = false, Func<LocalWebAppInfo, bool> predicate = null)
    {
        var dir = await GetSettingsDirAsync();
        var settings = await TryGetSettingsAsync(dir);
        var apps = settings?.TryGetArrayValue(dev ? "devapps" : "apps")?.OfType<JsonObjectNode>();
        var list = new List<LocalWebAppInfo>();
        if (apps == null) return list;
        foreach (var app in apps)
        {
            try
            {
                var item = app.Deserialize<LocalWebAppInfo>();
                if (string.IsNullOrWhiteSpace(item?.ResourcePackageId) || item.IsDisabled) continue;
                list.Add(item);
            }
            catch (JsonException)
            {
            }
        }

        return predicate == null ? list : list.Where(predicate).ToList();
    }

    /// <summary>
    /// Removes a specific resource package.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier to remove.</param>
    /// <returns>true if the resource package exists and is removed; otherwise, false.</returns>
    public static async Task<bool> RemovePackageAsync(string resourcePackageId)
    {
        if (string.IsNullOrWhiteSpace(resourcePackageId)) return false;
        var appDataFolder = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("LocalWebApp", Windows.Storage.CreationCollisionOption.OpenIfExists);
        var appId = FormatResourcePackageId(resourcePackageId);
        try
        {
            appDataFolder = await appDataFolder.GetFolderAsync(appId);
            if (appDataFolder != null) await appDataFolder.DeleteAsync();
        }
        catch (IOException)
        {
        }

        var dir = await GetSettingsDirAsync();
        var settings = await TryGetSettingsAsync(dir) ?? new JsonObjectNode();
        var apps = settings.TryGetArrayValue("apps")?.OfType<JsonObjectNode>();
        if (apps == null) return false;
        var json = apps.FirstOrDefault(ele => ele.TryGetStringValue("id") == resourcePackageId);
        if (json == null) return false;
        settings.TryGetArrayValue("apps").Remove(json);
        return true;
    }

    /// <summary>
    /// Removes the specific resource packages.
    /// </summary>
    /// <param name="resourcePackageIds">The list of the resource package identifier to remove.</param>
    /// <returns>The count of the resource package removed..</returns>
    public static async Task<int> RemovePackageAsync(IEnumerable<string> resourcePackageIds)
    {
        if (resourcePackageIds == null) return 0;
        var i = 0;
        foreach (var id in resourcePackageIds)
        {
            if (await RemovePackageAsync(id)) i++;
        }

        return i;
    }

    /// <summary>
    /// Removes the specific resource packages.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>The count of the resource package removed..</returns>
    public static async Task<int> RemovePackageAsync(Func<LocalWebAppInfo, bool> predicate)
    {
        if (predicate == null) return 0;
        var packages = await ListPackageAsync();
        var ids = packages.Where(predicate).Select(ele => ele.ResourcePackageId);
        return await RemovePackageAsync(ids);
    }

    /// <summary>
    /// Registers a resource package.
    /// </summary>
    /// <param name="info">The resource package information.</param>
    /// <param name="dev">true if list dev apps; otherwise, false.</param>
    /// <returns>The async task.</returns>
    internal static async Task<bool> RegisterPackageAsync(LocalWebAppInfo info, bool dev = false)
        => (await UpdatePackageAsync(info?.ResourcePackageId, info2 => {
            if (info2 != null && info2.Version == info.Version) info.LastModificationTime = info2.LastModificationTime;
            return info;
        }, dev)) != null;

    internal static DirectoryInfo GetDirectoryInfoByRelative(DirectoryInfo root, string relative)
    {
        if (string.IsNullOrEmpty(relative))
            return root;
        if (relative.EndsWith('/') || relative.EndsWith('\\'))
            relative = relative[..^1];
        if (relative.Length < 1 || relative == "." || relative == "~")
            return root;
        if (relative.StartsWith("./") || relative.StartsWith(".\\"))
            relative = relative[2..];
        while (relative.StartsWith("../") || relative.StartsWith("..\\"))
        {
            root = root.Parent;
            relative = relative[3..];
        }

        if (relative == "..")
            return root.Parent;
        return relative == "." ? root : FileSystemInfoUtility.TryGetDirectoryInfo(root.FullName, relative);
    }

    internal static FileInfo GetFileInfoByRelative(DirectoryInfo root, string relative)
    {
        if (string.IsNullOrEmpty(relative))
            return null;
        if (relative.EndsWith('/') || relative.EndsWith('\\'))
            relative = relative[..^1];
        if (relative.Length < 1 || relative == "." || relative == "~")
            return null;
        while (relative.StartsWith("../") || relative.StartsWith("..\\"))
        {
            root = root.Parent;
            relative = relative[3..];
        }

        return relative == ".." || relative == "." ? null : FileSystemInfoUtility.TryGetFileInfo(root.FullName, relative);
    }

    /// <summary>
    /// Gets the specific resource package information registered.
    /// </summary>
    /// <param name="id">The resource package identifier.</param>
    /// <param name="update">A callback to update.</param>
    /// <param name="dev">true if list dev apps; otherwise, false.</param>
    /// <returns>The resource package information instance.</returns>
    private static async Task<LocalWebAppInfo> UpdatePackageAsync(string id, Func<LocalWebAppInfo, LocalWebAppInfo> update, bool dev = false)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var dir = await GetSettingsDirAsync();
        var settings = await TryGetSettingsAsync(dir) ?? new JsonObjectNode();
        var property = dev ? "devapps" : "apps";
        var arr = settings.TryGetArrayValue(property);
        var apps = arr?.OfType<JsonObjectNode>();
        if (apps == null)
        {
            apps = new List<JsonObjectNode>();
            settings.SetValue(property, apps);
        }

        var json = apps.FirstOrDefault(ele => ele.TryGetStringValue("id") == id);
        LocalWebAppInfo info = null;
        var creation = DateTime.Now;
        try
        {
            if (json != null)
            {
                info = json.Deserialize<LocalWebAppInfo>();
                creation = info.CreationTime;
            }
        }
        catch (JsonException)
        {
        }

        if (update != null)
        {
            info = update(info);
            if (info != null)
            {
                if (json != null) arr.Remove(json);
                info.CreationTime = creation;
                var json2 = JsonObjectNode.ConvertFrom(info);
                settings.TryGetArrayValue(property).Add(json2);
                if (dev && arr.Count > 100)
                {
                    arr.Remove(0);
                    if (arr.Count > 100) arr.Remove(0);
                    if (arr.Count > 100) arr.Remove(0);
                }

                await TrySaveSettingsAsync(dir, settings);
            }
        }

        return info;
    }

    private static string FormatResourcePackageId(string resourcePackageId)
        => resourcePackageId.Replace('/', '_').Replace('\\', '_').Replace(' ', '_').Replace("@", string.Empty);

    private static string GetEmbeddedFileName(string name, System.Reflection.Assembly assembly)
    {
        var files = assembly.GetManifestResourceNames();
        var fileName = $"{assembly.GetName().Name}.{name}";
        if (files.Contains(fileName)) return fileName;
        if (files.Contains(name)) return name;
        name = $".{name}";
        foreach (var n in files)
        {
            if (n.EndsWith(name)) return n;
        }

        return null;
    }

    /// <summary>
    /// Gets the parent path of resource package.
    /// </summary>
    /// <param name="localRelativePath">The relative path.</param>
    /// <returns>The path.</returns>
    private string GetResourcePackageParentPath(string localRelativePath)
    {
        var host = ResourcePackageDirectory?.Parent?.FullName;
        return string.IsNullOrEmpty(host) ? null : Path.Combine(host, localRelativePath.TrimStart('.'));
    }

    private static async Task<DirectoryInfo> GetSettingsDirAsync()
    {
        try
        {
            var appDataFolder = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("LocalWebApp", Windows.Storage.CreationCollisionOption.OpenIfExists);
            appDataFolder = await appDataFolder.CreateFolderAsync("_settings", Windows.Storage.CreationCollisionOption.OpenIfExists);
            return FileSystemInfoUtility.TryGetDirectoryInfo(appDataFolder.Path);
        }
        catch (IOException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="options">The app options.</param>
    /// <param name="rootDir">The root path.</param>
    /// <param name="version">The expected version.</param>
    /// <param name="zip">The zip file.</param>
    /// <param name="deleteZip">true if delete the zip file after extracting.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version.</returns>
    private static async Task<string> UpdateAsync(LocalWebAppOptions options, DirectoryInfo rootDir, string version, FileInfo zip, bool deleteZip, CancellationToken cancellationToken = default)
    {
        if (zip == null || options == null) return null;
        var cacheDir = Directory.CreateDirectory(Path.Combine(rootDir.FullName, "cache"));

        // Extract zip.
        string path;
        try
        {
            path = Path.Combine(cacheDir.FullName, "TempResourcePackage");
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
        var dir = FileSystemInfoUtility.TryGetDirectoryInfo(path);
        if (dir == null || !dir.Exists) return null;
        try
        {
            var host = await LoadAsync(rootDir, options, false, dir, cancellationToken);
            if (host?.Manifest?.Version == null || (version != null && host.Manifest.Version != version) || host.ResourcePackageId != options.ResourcePackageId)
            {
                dir.Delete(true);
                return null;
            }

            version = host?.Manifest?.Version;
            path = Path.Combine(rootDir.FullName, string.Concat('v', version));
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
        catch (JsonException)
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
            FileSystemInfoUtility.CopyTo(dir, path);
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

        var settings = await TryGetSettingsAsync(cacheDir, cancellationToken) ?? new JsonObjectNode();
        var oldVersion = settings.TryGetStringValue("version")?.Trim();
        var installInfo = settings.TryGetObjectValue("install") ?? new();
        var oldVersion2 = installInfo.TryGetStringValue("oldVersion")?.Trim();
        settings.SetValue("version", version);
        installInfo.SetValue("old", oldVersion);
        installInfo.SetValue("done", DateTime.Now);
        settings.SetValue("install", installInfo);
        await TrySaveSettingsAsync(cacheDir, settings, cancellationToken);

        // Clean up.
        if (!string.IsNullOrEmpty(oldVersion2))
        {
            path = Path.Combine(rootDir.FullName, string.Concat('v', oldVersion2));
            TryDeleteDirectory(path);
        }

        // Return result.
        return version;
    }

    private static async Task<string> LoadCompressedResourceAsync(LocalWebAppOptions options, DirectoryInfo rootDir, System.Reflection.Assembly assembly, string fileName, CancellationToken cancellationToken = default)
    {
        var cacheDir = Directory.CreateDirectory(Path.Combine(rootDir.FullName, "cache"));
        var path = Path.Combine(cacheDir.FullName, "TempResourcePackage.zip");
        TryDeleteDirectory(path);
        try
        {
            using var fileStream = File.Create(path);
            using var stream = assembly.GetManifestResourceStream(fileName);
            await stream.CopyToAsync(fileStream, cancellationToken);
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

        return await UpdateAsync(options, rootDir, null, FileSystemInfoUtility.TryGetFileInfo(path), true, cancellationToken);
    }

    private static JsonObjectNode LoadBuildConfig(DirectoryInfo dir, out FileInfo file)
    {
        file = dir.EnumerateFiles(GetSubFileName(UI.LocalWebAppExtensions.DefaultManifestFileName, "project"))?.FirstOrDefault();
        if (file == null || !file.Exists) throw new FileNotFoundException("The config file does not exist.");
        return JsonObjectNode.TryParse(file);
    }

    /// <summary>
    /// Creates the app options.
    /// </summary>
    /// <param name="config">The build config.</param>
    /// <param name="keyFile">The private key.</param>
    /// <returns>The file output.</returns>
    /// <exception cref="InvalidOperationException">Parse private key failed.</exception>
    /// <exception cref="NotSupportedException">The signature algorithm was not supported.</exception>
    private static LocalWebAppOptions LoadOptions(JsonObjectNode config, FileInfo keyFile)
    {
        // Create options.
        var resId = config.TryGetStringValue("id")?.Trim() ?? config.TryGetObjectValue("package").TryGetStringValue("id")?.Trim();
        config = config.TryGetObjectValue("ref");
        var sign = config.TryGetStringValue("sign")?.Trim()?.ToUpperInvariant();
        if (string.IsNullOrEmpty(sign)) sign = "RS512";
        var keyStr = config.TryGetStringValue("key");
        try
        {
            if (string.IsNullOrWhiteSpace(keyStr) && keyFile != null && keyFile.Exists)
                keyStr = File.ReadAllText(keyFile.FullName);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("Parse private key failed because of IO exception.", ex);
        }
        catch (SecurityException ex)
        {
            throw new InvalidOperationException("Parse private key failed because of security exception.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Parse private key failed because of unauthorized access exception.", ex);
        }
        catch (NotSupportedException ex)
        {
            throw new InvalidOperationException("Parse private key failed because of not supported exception.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException("Parse private key failed because of invalid operation exception.", ex);
        }
        catch (ExternalException ex)
        {
            throw new InvalidOperationException("Parse private key failed because of external exception.", ex);
        }

        if (!config.TryDeserializeValue<LocalWebAppPackageUpdateInfo>("update", null, out var update)) update = null;
        return new(resId, sign, keyStr, update);
    }

    private async Task<string> UpdateRegisteredAsync()
    {
        await RegisterPackageAsync(new LocalWebAppInfo(this));
        return await UpdateAsync();
    }

    private static string GetSubFileName(string name, string sub, string ext = null)
    {
        var i = name.LastIndexOf('.');
        if (string.IsNullOrEmpty(ext)) ext = name[i..];
        return string.IsNullOrWhiteSpace(sub) ? string.Concat(name[..i], ext) : string.Concat(name[..i], '.', sub, ext);
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
                case "host":
                    q.Add(k, LocalWebAppHook.HostId);
                    break;
                case "additional":
                    q.Add(k, LocalWebAppHook.HostAdditionalString);
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
                throw new LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes.MissSignatureFile, "The signature file is not found.", new FileNotFoundException("The file signature list is not found."));
            }

            using var stream = file.OpenRead();
            var fileCol = await JsonSerializer.DeserializeAsync<LocalWebAppFileCollection>(stream, null as JsonSerializerOptions, cancellationToken);
            if (fileCol.Files == null)
            {
                if (skipVerificationException) return false;
                throw new LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes.MissSignatureInfo, "Miss file signature information.");
            }

            try
            {
                host.SignatureProvider = host.Options?.GetSignatureProvider();
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
                    throw new LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes.Incorrect, "Incorrect signature.");
                }

                if (fileInfo?.FullName != null) files.Remove(fileInfo.FullName);
            }

            if (files.Count > 0)
            {
                if (skipVerificationException) return false;
                throw new LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes.Partial, files.Count == 1 ? "Miss signature for a file." : $"Miss signature for {files.Count} files.");
            }

            return true;
        }
        catch (InvalidOperationException ex)
        {
            if (skipVerificationException) return false;
            throw new LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes.InvalidOperation, string.Concat("Invalid operation during file signature verification. ", ex.Message), ex);
        }
        catch (NotSupportedException ex)
        {
            if (skipVerificationException) return false;
            throw new LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes.NotSupported, string.Concat("Not supported during file signature verification.", ex.Message), ex);
        }
        catch (IOException ex)
        {
            if (skipVerificationException) return false;
            throw new LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes.IO, string.Concat("IO exception during file signature verification.", ex.Message), ex);
        }
        catch (ExternalException ex)
        {
            if (skipVerificationException) return false;
            throw new LocalWebAppSignatureException(LocalWebAppSignatureErrorTypes.External, string.Concat("External exception during file signature verification.", ex.Message), ex);
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
            await File.WriteAllTextAsync(path, json?.ToString(IndentStyles.Minified) ?? "null", Encoding.UTF8, cancellationToken);
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
