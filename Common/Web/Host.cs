using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
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
using Trivial.UI;

namespace Trivial.Web;

/// <summary>
/// The local standalone web app host.
/// </summary>
public partial class LocalWebAppHost
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
        VirtualHost = options?.CustomizedVirtualHost ?? LocalWebAppSettings.VirtualHostGenerator?.Invoke(manifest, options);
        if (string.IsNullOrWhiteSpace(VirtualHost)) VirtualHost = string.Concat(appDomain, '.', LocalWebAppExtensions.VirtualRootDomain);
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
    /// Gets or sets a value indicating whether need to disable auto update in standard mode.
    /// </summary>
    public bool IsAutoUpdateDisabled { get; set; }

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
        else if (localRelativePath.StartsWith('~')) localRelativePath = CombinePath(DataDirectory?.FullName ?? host, localRelativePath[1..]);
        else if (localRelativePath.StartsWith(".asset:")) localRelativePath = GetResourcePackagePath(localRelativePath[7..]);
        else if (localRelativePath.StartsWith(".data:")) return CombinePath(DataDirectory?.FullName ?? host, localRelativePath[6..]);
        else if (localRelativePath.StartsWith(".doc:")) return CombinePath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), localRelativePath[6..]);
        else if (localRelativePath.StartsWith("./") || localRelativePath.StartsWith(".\\")) localRelativePath = GetResourcePackagePath(localRelativePath[2..]);
        else if (localRelativePath.StartsWith('.')) return null;
        else if (localRelativePath.StartsWith('%')) localRelativePath = FileSystemInfoUtility.GetLocalPath(localRelativePath);
        else if (localRelativePath.Contains("://")) return localRelativePath;
        else if (test) return localRelativePath;
        return string.IsNullOrEmpty(host) ? null : CombinePath(host, localRelativePath);
    }

    /// <summary>
    /// Gets absolute icon path of resource package.
    /// </summary>
    public string GetResourcePackageIconPath()
    {
        var appDir = ResourcePackageDirectory;
        if (appDir == null || !appDir.Exists) return null;
        var s = Manifest?.Icon;
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (s.Contains("://")) return s;
        try
        {
            var file = GetFileInfoByRelative(appDir, s);
            if (file == null || !file.Exists || string.IsNullOrWhiteSpace(file.FullName)) return null;
            return file.FullName;
        }
        catch (IOException)
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
        catch (NotSupportedException)
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
        => LocalWebAppSettings.HostId = id;

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
        if (dir == null || string.IsNullOrWhiteSpace(LocalWebAppSettings.HostId)) throw new InvalidOperationException("The options is not correct.");
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
        => LoadAsync(assembly, new LocalWebAppEmbbeddedResourceInfo(), forceToLoad, skipVerificationException, cancellationToken);

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="assembly">The assembly which embed the resource package.</param>
    /// <param name="fileNames">The embbedded file names.</param>
    /// <param name="forceToLoad">true if force to load the resource; otherwise, false.</param>
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
    public static async Task<LocalWebAppHost> LoadAsync(System.Reflection.Assembly assembly, LocalWebAppEmbbeddedResourceInfo fileNames, bool forceToLoad = false, bool skipVerificationException = false, CancellationToken cancellationToken = default)
    {
        if (assembly == null) assembly = System.Reflection.Assembly.GetEntryAssembly();
        fileNames ??= new();
        var projectFileName = fileNames.ProjectFileName;
        var packageFileName = fileNames.PackageResourceFileName;
        var pemFileName = fileNames.PemFileName;
        var packageConfigFileName = fileNames.PackageConfigurationFileName?.Trim();
        if (string.IsNullOrWhiteSpace(projectFileName)) projectFileName = GetEmbeddedFileName(GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, "project"), assembly);
        if (string.IsNullOrWhiteSpace(packageFileName)) packageFileName = GetEmbeddedFileName(GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, null, ".zip"), assembly);
        if (string.IsNullOrWhiteSpace(pemFileName)) pemFileName = GetEmbeddedFileName(GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, null, ".pem"), assembly);
        using var stream = string.IsNullOrEmpty(projectFileName) ? null : assembly.GetManifestResourceStream(projectFileName);
        var config = JsonObjectNode.Parse(stream);
        var config2 = config?.TryGetObjectValue("ref");
        string version2 = null;
        if (config.GetValueKind("id") != JsonValueKind.String && (config.TryGetObjectValue("package") ?? config.TryGetObjectValue("manifest"))?.GetValueKind("id") != JsonValueKind.String)
        {
            JsonObjectNode packageConfig = null;
            try
            {
                using var stream2 = assembly.GetManifestResourceStream(packageConfigFileName ?? GetEmbeddedFileName("package.json", assembly));
                if (stream2 == null) throw new InvalidOperationException("Miss package identifier.");
                packageConfig = JsonObjectNode.Parse(stream2);
                if (packageConfig == null) throw new InvalidOperationException("Miss package identifier.");
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("Miss package identifier.", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Miss package identifier.", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Miss package identifier.", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Miss package identifier.", ex);
            }
            catch (NotSupportedException ex)
            {
                throw new InvalidOperationException("Miss package identifier.", ex);
            }
            catch (SecurityException ex)
            {
                throw new InvalidOperationException("Miss package identifier.", ex);
            }
            catch (NotImplementedException ex)
            {
                throw new InvalidOperationException("Miss package identifier.", ex);
            }
            catch (ExternalException ex)
            {
                throw new InvalidOperationException("Miss package identifier.", ex);
            }

            var propStr = packageConfig.TryGetStringTrimmedValue("name");
            if (string.IsNullOrWhiteSpace(propStr)) throw new InvalidOperationException("Miss package identifier.");
            config.SetValue("id", propStr);
            version2 = packageConfig.TryGetStringTrimmedValue("version");
        }

        if (config2 == null) throw new InvalidOperationException("Load project config failed.");
        var key = config2.TryGetStringValue("key");
        if (string.IsNullOrWhiteSpace(key))
        {
            if (string.IsNullOrWhiteSpace(pemFileName)) throw new InvalidOperationException("Miss public key of signature.");
            using var stream2 = assembly.GetManifestResourceStream(pemFileName);
            if (stream2 == null) throw new InvalidOperationException("Miss public key of signature. The specific file does not exist.");
            using var reader = new StreamReader(stream2);
            key = reader.ReadToEnd();
            config2.SetValue("key", key);
        }

        var options = LoadOptions(config, null);
        var version = forceToLoad ? "*" : (config.TryGetObjectValue("package") ?? config.TryGetObjectValue("manifest"))?.TryGetStringTrimmedValue("version", true) ?? version2;
        return await LoadAsync(options, assembly, packageFileName, version, skipVerificationException, cancellationToken);
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
    public static Task<LocalWebAppHost> LoadAsync(LocalWebAppOptions options, System.Reflection.Assembly assembly, string fileName, bool forceToLoad = false, bool skipVerificationException = false, CancellationToken cancellationToken = default)
        => LoadAsync(options, assembly, fileName, forceToLoad ? "*" : null, skipVerificationException, cancellationToken);

    /// <summary>
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <param name="assembly">The assembly which embed the resource package.</param>
    /// <param name="fileName">The zip file name of the embedded resource package.</param>
    /// <param name="version">The version to check update.</param>
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
    private static async Task<LocalWebAppHost> LoadAsync(LocalWebAppOptions options, System.Reflection.Assembly assembly, string fileName, string version, bool skipVerificationException = false, CancellationToken cancellationToken = default)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrEmpty(options.ResourcePackageId)) throw new InvalidOperationException("The resource package identifier should not be null or empty.");
        var appId = FormatResourcePackageId(options.ResourcePackageId);
        var dir = await TryGetPackageFolderAsync(FormatResourcePackageId(appId));
        if (dir == null || !dir.Exists) throw new InvalidOperationException("Initialize app data folder failed.");
        var forceToLoad = version == "*";
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
        if (!string.IsNullOrWhiteSpace(version) && !string.IsNullOrWhiteSpace(host?.Manifest?.Version) && version.Length > 2 && VersionComparer.Compare(host.Manifest.Version, version, false) < 0)
        {
            await LoadCompressedResourceAsync(options, dir, assembly, fileName, cancellationToken);
            host = await LoadAsync(dir, options, skipVerificationException, null, cancellationToken);
        }

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
        return await LoadAsync(info?.GetOptions(), skipVerificationException, cancellationToken);
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
        if (string.IsNullOrWhiteSpace(manifestFileName)) manifestFileName = LocalWebAppExtensions.DefaultManifestFileName;
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
        if (verifyOptions == LocalWebAppVerificationOptions.Regular && !IsForCurrentHost(manifest.HostBinding))
            throw new InvalidOperationException("Does not match the current host app.");

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

    private static bool IsForCurrentHost(List<LocalWebAppHostBindingInfo> hostBinding)
    {
        if (hostBinding == null || hostBinding.Count < 1) return true;
        var version = LocalWebAppSettings.GetAssembly()?.GetName()?.Version?.ToString();
        foreach (var bindingInfo in hostBinding)
        {
            if (bindingInfo?.HostId != LocalWebAppSettings.HostId) continue;
            if (!string.IsNullOrWhiteSpace(bindingInfo.MinimumVersion))
                if (VersionComparer.Compare(bindingInfo.MinimumVersion, version, false) > 0) continue;
            if (!string.IsNullOrWhiteSpace(bindingInfo.MaximumVersion))
                if (VersionComparer.Compare(bindingInfo.MaximumVersion, version, false) < 0) continue;
            return true;
        }

        return false;
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
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        var dir = await GetAppRootDirectoryAsync(options);
        if (dir == null || !dir.Exists) throw new InvalidOperationException("Initialize app data folder failed.");
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
        var host = await LoadAsync(dir, options, false, null, cancellationToken);
        if (host.ResourcePackageId != options.ResourcePackageId) throw new InvalidOperationException("The app is not the expect one.");
        if (host.IsVerified) await RegisterPackageAsync(new LocalWebAppInfo(host));
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
    /// Loads the standalone web app package information.
    /// </summary>
    /// <param name="options">The options to parse.</param>
    /// <param name="zip">The path of the compressed resource package file.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web app host.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadAsync(LocalWebAppOptions options, FileInfo zip, CancellationToken cancellationToken = default)
    {
        var dir = await GetAppRootDirectoryAsync(options);
        if (dir == null || !dir.Exists) throw new InvalidOperationException("Initialize app data folder failed.");
        await UpdateAsync(options, dir, null, zip, false, cancellationToken);
        var host = await LoadAsync(dir, options, false, null, cancellationToken);
        if (host.ResourcePackageId != options.ResourcePackageId) throw new InvalidOperationException("The app is not the expect one.");
        if (host.IsVerified) await RegisterPackageAsync(new LocalWebAppInfo(host));
        return host;
    }

    /// <summary>
    /// Load a dev local web app.
    /// </summary>
    /// <param name="dir">The root directory.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The async task.</returns>
    /// <exception cref="ArgumentNullException">info or uri was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    public static async Task<LocalWebAppHost> LoadDevPackageAsync(DirectoryInfo dir, CancellationToken cancellationToken = default)
    {
        if (dir == null || !dir.Exists) throw new DirectoryNotFoundException("The directory is not found.");
        var package = Package(dir);
        var host = await LoadAsync(package, false, cancellationToken);
        LocalWebAppInfo info = null;
        try
        {
            info = new LocalWebAppInfo(host, package.Details)
            {
                LocalPath = package.RootDirectory.FullName
            };
        }
        catch (IOException)
        {
        }
        catch (NotSupportedException)
        {
        }

        if (!string.IsNullOrWhiteSpace(info?.LocalPath)) await RegisterPackageAsync(info, true);
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
            if (string.IsNullOrEmpty(fileName)) fileName = LocalWebAppExtensions.DefaultManifestFileName;
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
        if (string.IsNullOrEmpty(fileName)) fileName = LocalWebAppExtensions.DefaultManifestFileName;
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
        var manifestPath = Path.Combine(dir.FullName, LocalWebAppExtensions.DefaultManifestFileName);
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
        if (string.IsNullOrWhiteSpace(fileName)) fileName = LocalWebAppExtensions.DefaultManifestFileName;
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
        var keyFile = GetPrivateKeyFile(dir);
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
        Sign(dir, signatureProvider, LocalWebAppExtensions.DefaultManifestGeneratedFileName);
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
    /// <param name="signatureProvider">The signature provider with private key.</param>
    /// <param name="compressionLevel">The compression level.</param>
    /// <param name="outputFileName">The file name of the zip.</param>
    /// <returns>The file output.</returns>
    public static FileInfo Package(DirectoryInfo dir, ISignatureProvider signatureProvider, CompressionLevel compressionLevel, string outputFileName = null)
    {
        Sign(dir, signatureProvider, LocalWebAppExtensions.DefaultManifestGeneratedFileName);
        if (string.IsNullOrWhiteSpace(outputFileName))
            outputFileName = string.Concat(dir.FullName, ".zip");
        if (!outputFileName.Contains('\\') && !outputFileName.Contains('/'))
            outputFileName = Path.Combine((dir.Parent ?? dir).FullName, outputFileName);
        File.Delete(outputFileName);
        ZipFile.CreateFromDirectory(dir.FullName, outputFileName, compressionLevel, false);
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
        => Package(dir, null as CompressionLevel?, outputFileName);

    /// <summary>
    /// Creates a package.
    /// </summary>
    /// <param name="dir">The app path.</param>
    /// <param name="compressionLevel">The compression level.</param>
    /// <param name="outputFileName">The file name of the zip.</param>
    /// <returns>The file output.</returns>
    /// <exception cref="DirectoryNotFoundException">dir was not found.</exception>
    /// <exception cref="DirectoryNotFoundException">The private key was not found.</exception>
    /// <exception cref="InvalidOperationException">The configuration is not valid.</exception>
    /// <exception cref="NotSupportedException">The signature algorithm was not supported.</exception>
    /// <exception cref="IOException">IO exception.</exception>
    /// <exception cref="SecurityException">The security exception during file access.</exception>
    /// <exception cref="UnauthorizedAccessException">One or more files are unauthorized to access.</exception>
    public static LocalWebAppPackageResult Package(DirectoryInfo dir, CompressionLevel? compressionLevel, string outputFileName = null)
    {
        // Load options.
        if (dir == null || !dir.Exists) throw new DirectoryNotFoundException("The root directory is not found.");
        var config = TryLoadBuildConfig(dir, out var configFile);
        var rootDir = dir;
        if (config == null)
        {
            dir = dir.EnumerateDirectories("localwebapp").FirstOrDefault();
            if (dir != null) config = LoadBuildConfig(dir, out configFile);
        }

        if (configFile == null) throw new InvalidOperationException("Miss config file.");
        if (config == null) throw new InvalidOperationException("Parse the config file failed.");
        var keyFile = GetPrivateKeyFile(dir);
        var refConfig = config.TryGetObjectValue("ref") ?? new();

        // Create manifest.
        var appDir = GetDirectoryInfoByRelative(dir, refConfig.TryGetStringValue("path")?.Trim()) ?? dir;
        try
        {
            if (!appDir.Exists) appDir.Create();
        }
        catch (IOException)
        {
        }

        var manifestPath = Path.Combine(appDir.FullName, LocalWebAppExtensions.DefaultManifestFileName);
        var manifestJson = config.TryGetObjectValue("package") ?? config.TryGetObjectValue("manifest") ?? new();
        var packageId = config.TryGetStringValue("id")?.Trim();
        var curVerKind = manifestJson.GetValueKind("version");
        var nodeConfig = JsonObjectNode.TryParse(FileSystemInfoUtility.TryGetFileInfo(rootDir, "package.json"));
        if (!string.IsNullOrEmpty(packageId))
        {
            manifestJson.SetValue("id", packageId);
        }
        else if (manifestJson.GetValueKind("id") != JsonValueKind.String)
        {
            packageId = nodeConfig?.TryGetStringTrimmedValue("name", true) ?? throw new InvalidOperationException("Miss package identifier.");
            manifestJson.SetValue("id", packageId);
            config.SetValue("id", packageId);
        }
        else
        {
            packageId = manifestJson.TryGetStringTrimmedValue("id", true);
            if (packageId == null) throw new InvalidOperationException("Miss package identifier.");
        }

        curVerKind = manifestJson.GetValueKind("version");
        if (curVerKind != JsonValueKind.String && curVerKind != JsonValueKind.Number)
            manifestJson.SetValue("version", nodeConfig?.TryGetStringTrimmedValue("version", true) ?? "0.0.1");
        FillFallbackStringProperty(manifestJson, "description", nodeConfig);
        FillFallbackStringProperty(manifestJson, "website", nodeConfig, "homepage");

        File.WriteAllText(manifestPath, manifestJson.ToString());

        // Setup dev environment.
        if (config.TryGetObjectValue("dev", out var devConfig))
        {
            // Copy source.
            if (devConfig.TryGetArrayValue("copy", out var copyItems))
            {
                foreach (var item in copyItems.OfType<JsonObjectNode>())
                {
                    var src = item?.TryGetStringValue("src")?.Trim();
                    var dist = item?.TryGetStringValue("dist")?.Trim();
                    if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dist) || item.TryGetBooleanValue("disable") == true) continue;
                    try
                    {
                        if (item.TryGetBooleanValue("file") == true)
                        {
                            var sourceFile = GetFileInfoByRelative(dir, src);
                            var distFile = GetFileInfoByRelative(appDir, dist);
                            if (sourceFile == null || !sourceFile.Exists || distFile == null) continue;
                            sourceFile.CopyTo(distFile.FullName, true);
                        }
                        else
                        {
                            var sourceDir = GetDirectoryInfoByRelative(dir, src);
                            var distDir = GetDirectoryInfoByRelative(appDir, dist);
                            if (sourceDir == null || !sourceDir.Exists || distDir == null) continue;
                            sourceDir.TryCopyTo(distDir.FullName);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (SecurityException)
                    {
                    }
                    catch (InvalidOperationException)
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
        }

        // Sign.
        var options = LoadOptions(config, keyFile);
        Sign(appDir, options);

        // Load.
        if (string.IsNullOrWhiteSpace(outputFileName))
            outputFileName = Path.Combine(dir.FullName, GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, null, ".zip"));
        if (!outputFileName.Contains('\\') && !outputFileName.Contains('/'))
            outputFileName = Path.Combine(dir.FullName, outputFileName);
        File.Delete(outputFileName);
        if (compressionLevel.HasValue) ZipFile.CreateFromDirectory(appDir.FullName, outputFileName, compressionLevel.Value, false);
        else ZipFile.CreateFromDirectory(appDir.FullName, outputFileName);
        var zip = FileSystemInfoUtility.TryGetFileInfo(outputFileName);

        // Copy
        var arr = refConfig.TryGetArrayValue("output")?.OfType<JsonObjectNode>();
        if (arr != null)
        {
            foreach (var output in arr)
            {
                if (output == null) continue;
                var outputPath = GetFileInfoByRelative(dir, output.TryGetStringValue("zip"));
                if (!string.IsNullOrWhiteSpace(outputPath?.FullName) && zip.FullName != outputPath.FullName) zip.CopyTo(outputPath.FullName, true);
                outputPath = GetFileInfoByRelative(dir, output.TryGetStringValue("config"));
                if (!string.IsNullOrWhiteSpace(outputPath?.FullName) && configFile.FullName != outputPath.FullName) configFile.CopyTo(outputPath.FullName, true);
            }
        }

        // Generate update meta
        var updateMeta = refConfig.TryGetObjectValue("updateMeta");
        if (updateMeta != null)
        {
            var updateMetaFile = GetFileInfoByRelative(dir, updateMeta.TryGetStringTrimmedValue("path"));
            var updateMetaBackupPath = GetFilePathByRelative(dir, updateMeta.TryGetStringTrimmedValue("backupPath", true));
            if ((updateMetaFile == null || !updateMetaFile.Exists) && !string.IsNullOrEmpty(updateMetaBackupPath) && File.Exists(updateMetaBackupPath))
            {
                var updateMetaPath = GetFilePathByRelative(dir, updateMeta.TryGetStringTrimmedValue("path"));
                if (!string.IsNullOrEmpty(updateMetaPath)) File.Copy(updateMetaBackupPath, updateMetaPath, true);
                updateMetaFile = GetFileInfoByRelative(dir, updateMeta.TryGetStringTrimmedValue("path"));
                updateMetaFile.Refresh();
            }

            if (updateMetaFile != null && updateMetaFile.Exists)
            {
                var umJson = JsonObjectNode.TryParse(updateMetaFile) ?? new();
                var umProp = updateMeta.TryGetStringTrimmedValue("prop", true) ?? "localwebapp";
                var umJson2 = umJson.TryGetObjectValue(umProp);
                if (umJson2 == null)
                {
                    umJson.SetValue(umProp, new JsonObjectNode());
                    umJson2 = umJson.TryGetObjectValue(umProp);
                }

                var appCollection = umJson2.TryGetObjectListValue("apps");
                var um = appCollection?.FirstOrDefault(ele => ele?.TryGetStringTrimmedValue("id") == packageId);
                if (um == null)
                {
                    um = new JsonObjectNode
                {
                    { "id", packageId }
                };
                    appCollection ??= new();
                    appCollection.Add(um);
                    umJson2.SetValue("apps", appCollection);
                }

                var tempStr = manifestJson.TryGetStringValue("title");
                if (!string.IsNullOrWhiteSpace(tempStr)) um.SetValue("title", tempStr);
                tempStr = manifestJson.TryGetStringValue("version");
                um.SetValue("version", tempStr);
                tempStr ??= string.Empty;
                var umUrl = updateMeta.TryGetStringTrimmedValue("urlTemplate", true);
                if (umUrl != null) um.SetValue("url", umUrl
                    .Replace("{ver}", tempStr)
                    .Replace("{ver_}", tempStr.Replace('.', '_').Replace('-', '_'))
                    .Replace("{ver/}", tempStr.Replace('.', '/').Replace('-', '/'))
                    .Replace("{id}", packageId.Replace("@", string.Empty))
                    .Replace("{id_}", packageId.Replace("@", string.Empty).Replace('.', '_').Replace('-', '_'))
                    .Replace("{t}", WebFormat.ParseDate(DateTime.Now).ToString("g"))
                    .Replace("{r}", Guid.NewGuid().ToString("N")));
                var umInfo = updateMeta.TryGetObjectValue("info");
                if (umInfo != null) um.SetValue("info", umInfo);
                umInfo = updateMeta.TryGetObjectValue("sign");
                if (umInfo != null)
                {
                    um.SetValue("sign", umInfo);
                }
                else
                {
                    var umSign = updateMeta.TryGetStringTrimmedValue("sign", true);
                    if (umSign != null) um.SetValue("sign", umSign);
                    //else um.SetValue("sign", new JsonObjectNode
                    //{
                    //    { "alg", options.SignatureAlgorithm },
                    //    { "key", options.SignatureKey }   // Need export public key
                    //});
                }

                var umHashAlg = updateMeta.TryGetStringTrimmedValue("hash")?.ToUpperInvariant()?.Replace("-", string.Empty) ?? string.Empty;
                switch (umHashAlg)
                {
                    case "SHA256":
                        um.SetValue("hash", HashUtility.ComputeHashString(SHA256.Create, zip));
                        break;
                    case "SHA384":
                        um.SetValue("hash", HashUtility.ComputeHashString(SHA384.Create, zip));
                        break;
                    case "SHA512":
                        um.SetValue("hash", HashUtility.ComputeHashString(SHA512.Create, zip));
                        break;
                    default:
                        um.Remove("hash");
                        break;
                }

                var hostInfo = manifestJson.TryGetArrayValue("host");
                if (hostInfo != null) um.SetValue("host", hostInfo);
                var updateMetaMessage = updateMeta.TryGetStringTrimmedValue("message", true);
                if (updateMetaMessage != null) um.SetValue("message", updateMetaMessage);
                try
                {
                    File.WriteAllText(updateMetaFile.FullName, umJson.ToString(IndentStyles.Compact));
                    if (!string.IsNullOrEmpty(updateMetaBackupPath)) updateMetaFile.CopyTo(updateMetaBackupPath, true);
                }
                catch (IOException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (UnauthorizedAccessException)
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
            }
        }

        // Return result.
        var result = new LocalWebAppPackageResult(options, rootDir, appDir, zip, config.TryGetObjectValue("details"), config.TryGetObjectValue("project"));
        LocalWebAppSettings.BuildDevPackage?.Invoke(result);
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
        LocalWebAppSettings.OnUpdate?.Invoke(this);
        return NewVersionAvailable;
    }

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version; or null, if no update.</returns>
    public Task<string> UpdateAsync(CancellationToken cancellationToken = default)
        => UpdateAsync(string.Empty, cancellationToken);

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="uri">The update URI.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version; or null, if no update.</returns>
    public Task<string> UpdateAsync(Uri uri, CancellationToken cancellationToken = default)
        => UpdateAsync(uri.OriginalString, cancellationToken);

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
    /// <param name="dev">true if list dev apps; otherwise, false.</param>
    /// <returns>true if the resource package exists and is removed; otherwise, false.</returns>
    public static async Task<bool> RemovePackageAsync(string resourcePackageId, bool dev = false)
    {
        if (string.IsNullOrWhiteSpace(resourcePackageId)) return false;
        var appId = FormatResourcePackageId(resourcePackageId);
        if (string.IsNullOrWhiteSpace(appId)) return false;
        if (!dev)
        {
            if (appId == "_settings") return false;
            await TryRemovePackageFolderAsync(appId);
        }

        var dir = await GetSettingsDirAsync();
        var settings = await TryGetSettingsAsync(dir) ?? new JsonObjectNode();
        var prop = dev ? "devapps" : "apps";
        var apps = settings.TryGetArrayValue(prop)?.OfType<JsonObjectNode>();
        if (apps == null) return false;
        var json = apps.FirstOrDefault(ele => ele.TryGetStringValue("id") == resourcePackageId);
        if (json == null) return false;
        settings.TryGetArrayValue(prop).Remove(json);
        json = apps.FirstOrDefault(ele => ele.TryGetStringValue("id") == resourcePackageId);
        if (json != null) settings.TryGetArrayValue(prop).Remove(json);
        await TrySaveSettingsAsync(dir, settings);
        return true;
    }

    /// <summary>
    /// Removes the specific resource packages.
    /// </summary>
    /// <param name="resourcePackageIds">The list of the resource package identifier to remove.</param>
    /// <param name="dev">true if list dev apps; otherwise, false.</param>
    /// <returns>The count of the resource package removed..</returns>
    public static async Task<int> RemovePackageAsync(IEnumerable<string> resourcePackageIds, bool dev = false)
    {
        if (resourcePackageIds == null) return 0;
        var i = 0;
        foreach (var id in resourcePackageIds)
        {
            if (await RemovePackageAsync(id, dev)) i++;
        }

        return i;
    }

    /// <summary>
    /// Removes the specific resource packages.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="dev">true if list dev apps; otherwise, false.</param>
    /// <returns>The count of the resource package removed..</returns>
    public static async Task<int> RemovePackageAsync(Func<LocalWebAppInfo, bool> predicate, bool dev = false)
    {
        if (predicate == null) return 0;
        var packages = await ListPackageAsync(dev);
        var ids = packages.Where(predicate).Select(ele => ele.ResourcePackageId).ToList();
        return await RemovePackageAsync(ids);
    }

    /// <summary>
    /// Updates.
    /// </summary>
    /// <param name="config">The configuration file.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The local web hosts updated.</returns>
    public static async Task<List<LocalWebAppHost>> UpdateAsync(JsonObjectNode config, CancellationToken cancellationToken = default)
    {
        var apps = config?.TryGetObjectListValue("apps");
        if (apps == null || apps.Count < 1 || config.TryGetBooleanValue("disable") == true) return new();
        var arr = await ListPackageAsync();
        var removing = new List<string>();
        var list = new List<LocalWebAppHost>();
        foreach (var item in apps)
        {
            try
            {
                var id = item.TryGetStringTrimmedValue("id", true);
                if (id == null) continue;
                if (item.TryGetBooleanValue("disable") == true) continue;
                var idFormatted = id.ToUpperInvariant().Replace("@", string.Empty).Replace("\\", "/");
                var hostBindings = ConvertToBindingInfoList(item.TryGetObjectListValue("host"));
                var app = arr.FirstOrDefault(ele => {
                    var testId = ele?.ResourcePackageId?.Trim();
                    if (string.IsNullOrEmpty(testId)) return false;
                    if (idFormatted != testId?.ToUpperInvariant()?.Replace("@", string.Empty)?.Replace("\\", "/"))
                        return false;
                    return IsForCurrentHost(hostBindings);
                });
                if (item.TryGetBooleanValue("remove") == true)
                {
                    if (app != null) removing.Add(id);
                    continue;
                }

                removing.Remove(id);
                var version = item.TryGetStringTrimmedValue("version", true);
                var uri = LocalWebAppExtensions.TryCreateUri(item.TryGetStringTrimmedValue("url", true));
                if (version == null || uri == null) continue;
                var sign = item.TryGetObjectValue("sign");
                if (sign == null)
                {
                    var signId = item.TryGetStringTrimmedValue("sign", true);
                    if (signId != null) sign = config.TryGetObjectValue("sign")?.TryGetObjectValue(signId);
                }

                if (app == null)
                {
                    if (sign == null) continue;
                    var signAlg = sign.TryGetStringTrimmedValue("alg", true);
                    var signKey = sign.TryGetStringTrimmedValue("key", true);
                    if (signAlg == null || signKey == null) continue;
                    var options = new LocalWebAppOptions(id, signAlg, signKey);
                    var host2 = await LoadAsync(options, uri, cancellationToken);
                    list.Add(host2);
                    continue;
                }

                if (VersionComparer.Compare(version, app.Version, true) <= 0) continue;
                if (sign != null)
                {
                    var signAlg = sign.TryGetStringTrimmedValue("alg", true);
                    var signKey = sign.TryGetStringTrimmedValue("key", true);
                    if (signAlg != null && signKey != null) app.SetKey(signAlg, signKey);
                }

                var host = await LoadAsync(app, uri, cancellationToken);
                list.Add(host);
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
            catch (JsonException)
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
            catch (AggregateException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ApplicationException)
            {
            }
            catch (ExternalException)
            {
            }
        }

        try
        {
            await RemovePackageAsync(removing);
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

        return list;
    }

    /// <summary>
    /// Lists all resource packages registered.
    /// </summary>
    /// <returns>The list of the resource packages.</returns>
    internal static async Task<(List<LocalWebAppInfo>, List<LocalWebAppInfo>)> ListAllPackageAsync()
    {
        var dir = await GetSettingsDirAsync();
        var settings = await TryGetSettingsAsync(dir);
        var apps = settings?.TryGetArrayValue("apps")?.OfType<JsonObjectNode>();
        var list1 = new List<LocalWebAppInfo>();
        if (apps != null)
        {
            foreach (var app in apps)
            {
                try
                {
                    var item = app.Deserialize<LocalWebAppInfo>();
                    if (string.IsNullOrWhiteSpace(item?.ResourcePackageId) || item.IsDisabled) continue;
                    list1.Add(item);
                }
                catch (JsonException)
                {
                }
            }
        }

        apps = settings?.TryGetArrayValue("devapps")?.OfType<JsonObjectNode>();
        var list2 = new List<LocalWebAppInfo>();
        if (apps != null)
        {
            foreach (var app in apps)
            {
                try
                {
                    var item = app.Deserialize<LocalWebAppInfo>();
                    if (string.IsNullOrWhiteSpace(item?.ResourcePackageId) || item.IsDisabled) continue;
                    list2.Add(item);
                }
                catch (JsonException)
                {
                }
            }
        }

        return (list1, list2);
    }

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
        if (relative.StartsWith("./") || relative.StartsWith(".\\"))
            relative = relative[2..];
        while (relative.StartsWith("../") || relative.StartsWith("..\\"))
        {
            root = root.Parent;
            relative = relative[3..];
        }

        return relative == ".." || relative == "." ? null : FileSystemInfoUtility.TryGetFileInfo(root.FullName, relative);
    }

    internal static string GetFilePathByRelative(DirectoryInfo root, string relative)
    {
        if (string.IsNullOrEmpty(relative))
            return null;
        if (relative.EndsWith('/') || relative.EndsWith('\\'))
            relative = relative[..^1];
        if (relative.Length < 1 || relative == "." || relative == "~")
            return null;
        if (relative.StartsWith("./") || relative.StartsWith(".\\"))
            relative = relative[2..];
        while (relative.StartsWith("../") || relative.StartsWith("..\\"))
        {
            root = root.Parent;
            relative = relative[3..];
        }

        if (relative == ".." || relative == ".") return null;
        var path = FileSystemInfoUtility.GetLocalPath(root.FullName);
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(relative)) return null;
        return Path.Combine(path, relative);
    }

    internal static JsonObjectNode TryLoadBuildConfig(DirectoryInfo dir, out FileInfo file)
    {
        file = dir.EnumerateFiles(GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, "project"))?.FirstOrDefault();
        if (file == null || !file.Exists)
        {
            dir = dir.EnumerateDirectories("localwebapp")?.FirstOrDefault();
            if (dir != null && dir.Exists)
                file = dir.EnumerateFiles(GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, "project"))?.FirstOrDefault();
        }

        if (file == null || !file.Exists) return null;
        return JsonObjectNode.TryParse(file);
    }

    internal static JsonObjectNode LoadBuildConfig(DirectoryInfo dir, out FileInfo file)
    {
        var json = TryLoadBuildConfig(dir, out file) ?? throw new FileNotFoundException("The config file does not exist.");
        return json;
    }

    private static FileInfo GetPrivateKeyFile(DirectoryInfo dir)
    {
        var name = GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, "private", ".pem");
        var file = dir.EnumerateFiles(name)?.FirstOrDefault();
        if (file != null && file.Exists) return file;
        dir = dir.EnumerateDirectories("localwebapp")?.FirstOrDefault();
        if (dir != null && dir.Exists) file = dir.EnumerateFiles(name)?.FirstOrDefault();
        if (file != null && file.Exists) return file;
        try
        {
            var path = Path.Combine(dir.FullName, name);
            throw new FileNotFoundException("The private key does not exist.", path);
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

        throw new FileNotFoundException("The private key does not exist.");
    }

    /// <summary>
    /// Registers a resource package.
    /// </summary>
    /// <param name="info">The resource package information.</param>
    /// <param name="dev">true if list dev apps; otherwise, false.</param>
    /// <returns>The async task.</returns>
    private static async Task<bool> RegisterPackageAsync(LocalWebAppInfo info, bool dev = false)
        => (await UpdatePackageAsync(info?.ResourcePackageId, info2 => {
            if (info2 != null && info2.Version == info.Version) info.LastModificationTime = info2.LastModificationTime;
            return info;
        }, dev)) != null;


    /// <summary>
    /// Gets the root directory of the app.
    /// </summary>
    /// <param name="options">The options to load.</param>
    /// <returns>The root directory of the app.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    /// <exception cref="InvalidOperationException">The options was incorrect.</exception>
    /// <exception cref="DirectoryNotFoundException">The related directory was not found.</exception>
    /// <exception cref="FileNotFoundException">The resource manifest was not found.</exception>
    /// <exception cref="JsonException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="FormatException">The format of the resource manifest was incorrect.</exception>
    /// <exception cref="LocalWebAppSignatureException">Signature failed.</exception>
    private static async Task<DirectoryInfo> GetAppRootDirectoryAsync(LocalWebAppOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrEmpty(options.ResourcePackageId)) throw new InvalidOperationException("The resource package identifier should not be null or empty.");
        return await TryGetPackageFolderAsync(FormatResourcePackageId(options.ResourcePackageId));
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
        if (arr == null)
        {
            arr = new JsonArrayNode();
            settings.SetValue(property, arr);
        }

        var apps = arr.OfType<JsonObjectNode>();
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

    private static List<LocalWebAppHostBindingInfo> ConvertToBindingInfoList(List<JsonObjectNode> arr)
    {
        var hostBindings = new List<LocalWebAppHostBindingInfo>();
        if (arr == null) return hostBindings;
        foreach (var binding in arr)
        {
            if (binding == null) continue;
            var b = new LocalWebAppHostBindingInfo
            {
                HostId = binding.TryGetStringTrimmedValue("id", true),
                FrameworkKind = binding.TryGetStringTrimmedValue("kind", true),
                MinimumVersion = binding.TryGetStringTrimmedValue("min", true),
                MaximumVersion = binding.TryGetStringTrimmedValue("max", true),
            };
            if (b.HostId == null) continue;
            hostBindings.Add(b);
        }

        return hostBindings;
    }

    /// <summary>
    /// Gets the parent path of resource package.
    /// </summary>
    /// <param name="localRelativePath">The relative path.</param>
    /// <returns>The path.</returns>
    private string GetResourcePackageParentPath(string localRelativePath)
    {
        var host = ResourcePackageDirectory?.Parent?.FullName;
        return string.IsNullOrEmpty(host) ? null : CombinePath(host, localRelativePath.TrimStart('.'));
    }

    /// <summary>
    /// Gets the parent path of resource package.
    /// </summary>
    /// <param name="localRelativePath">The relative path.</param>
    /// <returns>The path.</returns>
    private string GetResourcePackagePath(string localRelativePath)
    {
        var host = ResourcePackageDirectory?.FullName;
        return string.IsNullOrEmpty(host) ? null : CombinePath(host, localRelativePath.TrimStart('.'));
    }

    /// <summary>
    /// Updates the resource package.
    /// </summary>
    /// <param name="url">The update URL.</param>
    /// <param name="cancellationToken">The optional cancellation token to cancel operation.</param>
    /// <returns>The new version; or null, if no update.</returns>
    private async Task<string> UpdateAsync(string url, CancellationToken cancellationToken = default)
    {
        // Get update manifest.
        if (string.IsNullOrWhiteSpace(url)) url = GetUrl(Options.Update?.Url, Options.Update?.VariableParameters);
        if (string.IsNullOrEmpty(url)) return null;
        var http = new JsonHttpClient<JsonObjectNode>
        {
            SerializeEvenIfFailed = true
        };
        LocalWebAppSettings.UpdateServiceClientHandler?.Invoke(http);
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
                var hostBindings = ConvertToBindingInfoList(respItem.TryGetObjectListValue("host"));
                if (!IsForCurrentHost(hostBindings)) continue;
                resp = respItem;
                break;
            }
        }
        else
        {
            var hostBindings = ConvertToBindingInfoList(resp?.TryGetObjectListValue("host"));
            if (!IsForCurrentHost(hostBindings)) return null;
        }

        if (resp == null) return null;
        var ver = resp.TryGetStringValue("version")?.Trim() ?? resp.TryGetStringValue("latestVersion")?.Trim();
        if (string.IsNullOrEmpty(ver)) return null;
        if (!string.IsNullOrWhiteSpace(Manifest.Version))
        {
            var compare = VersionComparer.Compare(ver, Manifest.Version, true);
            if (compare == 0 || (resp.TryGetBooleanValue("force") != true && compare < 0)) return null;
        }

        // Download zip.
        url = GetUrl(resp.TryGetStringValue("url"), resp.TryGetObjectValue("params"));
        var uri = LocalWebAppExtensions.TryCreateUri(url);
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
            if (string.IsNullOrWhiteSpace(host?.Manifest?.Version) || (version != null && host.Manifest.Version != version) || host.ResourcePackageId != options.ResourcePackageId)
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
                if (dir != null) dir.Refresh();
                if (dir.Exists) TryDeleteDirectory(dir.FullName);
            }
            catch (IOException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (ExternalException)
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

    private static string CombinePath(string parent, string folder)
    {
        if (folder.StartsWith('/') || folder.StartsWith('\\')) folder = folder[1..];
        return Path.Combine(parent, folder);
    }

    private static Task<DirectoryInfo> GetSettingsDirAsync()
        => TryGetPackageFolderAsync("_settings");

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
        var resId = config.TryGetStringValue("id")?.Trim() ?? (config.TryGetObjectValue("package") ?? config.TryGetObjectValue("manifest")).TryGetStringValue("id")?.Trim();
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
        if (IsVerified) await RegisterPackageAsync(new LocalWebAppInfo(this));
        return IsAutoUpdateDisabled ? null : await UpdateAsync();
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
                case "package-version":
                    q.Add(k, Manifest?.Version);
                    break;
                case "package-id":
                    q.Add(k, ResourcePackageId);
                    break;
                case "host-id":
                    q.Add(k, LocalWebAppSettings.HostId);
                    break;
                case "host-additional":
                    q.Add(k, LocalWebAppSettings.HostAdditionalString);
                    break;
                case "host-version":
                    q.Add(k, LocalWebAppSettings.GetAssembly()?.GetName()?.Version?.ToString());
                    break;
                case "fx-kind":
                    q.Add(k, "wasdk");
                    break;
                case "fx-ver":
                    q.Add(k, System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version?.ToString());
                    break;
                case "guid":
                    q.Add(k, Guid.NewGuid().ToString());
                    break;
            }
        }

        return q.ToString(url);
    }

    private static bool FillFallbackStringProperty(JsonObjectNode target, string key, JsonObjectNode fallback, string key2 = null)
    {
        if (target.ContainsKey(key)) return false;
        var prop = fallback.TryGetStringTrimmedValue(key2 ?? key, true);
        if (prop == null) return false;
        target.SetValue(key, prop);
        return true;
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
