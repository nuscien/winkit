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
using System.Xml.Linq;
using Trivial.UI;

namespace Trivial.Web;

/// <summary>
/// The options to verify.
/// </summary>
public enum LocalWebAppVerificationOptions : byte
{
    /// <summary>
    /// Verify.
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Do NOT verifiy.
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// Verify but skip exception.
    /// </summary>
    SkipException = 2,
}

/// <summary>
/// The states of window.
/// </summary>
public enum CommonWindowStates : byte
{
    /// <summary>
    /// Restored (normal).
    /// </summary>
    Restored = 0,

    /// <summary>
    /// Maximized.
    /// </summary>
    Maximized = 1,

    /// <summary>
    /// Minimized.
    /// </summary>
    Minimized = 2,

    /// <summary>
    /// Fullscreen.
    /// </summary>
    Fullscreen = 3,

    /// <summary>
    /// Compact overlay (picture-in-picture).
    /// </summary>
    Compact = 4,

    /// <summary>
    /// Pops up the dialog.
    /// </summary>
    Dialog = 5,

    /// <summary>
    /// Flyout.
    /// </summary>
    Flyout = 6,

    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown = 7,
}

/// <summary>
/// The options of the standalone web app resource package.
/// </summary>
public class LocalWebAppOptions
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppOptions class.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier.</param>
    /// <param name="signAlg">The signature algorithm.</param>
    /// <param name="signKey">The public key of signature.</param>
    /// <param name="update">The update service information.</param>
    public LocalWebAppOptions(string resourcePackageId, string signAlg, string signKey, LocalWebAppPackageUpdateInfo update = null)
        : this(resourcePackageId, signAlg, signKey, update, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppOptions class.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier.</param>
    /// <param name="signAlg">The signature algorithm.</param>
    /// <param name="signKey">The public key of signature.</param>
    /// <param name="update">The update service information.</param>
    /// <param name="manifestFileName">The file name of the manifest.</param>
    public LocalWebAppOptions(string resourcePackageId, string signAlg, string signKey, LocalWebAppPackageUpdateInfo update, string manifestFileName)
    {
        ResourcePackageId = resourcePackageId;
        SignatureAlgorithm = signAlg;
        SignatureKey = signKey;
        Update = update;
        if (string.IsNullOrWhiteSpace(manifestFileName)) manifestFileName = UI.LocalWebAppExtensions.DefaultManifestFileName;
        ManifestFileName = manifestFileName;
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppOptions class.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier.</param>
    /// <param name="signAlg">The signature algorithm.</param>
    /// <param name="signKey">The public key of signature.</param>
    /// <param name="update">The update service information.</param>
    /// <param name="manifestFileName">The file name of the manifest.</param>
    /// <param name="virtualHost">The customized virtual host.</param>
    public LocalWebAppOptions(string resourcePackageId, string signAlg, string signKey, LocalWebAppPackageUpdateInfo update, string manifestFileName, string virtualHost)
        : this(resourcePackageId, signAlg, signKey, update, manifestFileName)
    {
        if (!string.IsNullOrWhiteSpace(virtualHost)) CustomizedVirtualHost = virtualHost;
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppOptions class.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier.</param>
    /// <param name="options">The options to copy.</param>
    public LocalWebAppOptions(string resourcePackageId, LocalWebAppOptions options)
        : this(resourcePackageId, options.SignatureAlgorithm, options.SignatureKey, options.Update, options.ManifestFileName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppOptions class.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier.</param>
    /// <param name="options">The options to copy.</param>
    /// <param name="virtualHost">The customized virtual host.</param>
    public LocalWebAppOptions(string resourcePackageId, LocalWebAppOptions options, string virtualHost)
        : this(resourcePackageId, options)
    {
        if (!string.IsNullOrWhiteSpace(virtualHost)) CustomizedVirtualHost = virtualHost;
    }

    /// <summary>
    /// Gets the file name of the manifest.
    /// </summary>
    public string ManifestFileName { get; }

    /// <summary>
    /// Gets the app resource package identifier.
    /// </summary>
    public string ResourcePackageId { get; }

    /// <summary>
    /// Gets the update service information.
    /// </summary>
    public LocalWebAppPackageUpdateInfo Update { get; }

    /// <summary>
    /// Gets the signature algorithm.
    /// </summary>
    public string SignatureAlgorithm { get; }

    /// <summary>
    /// Gets the public key of signature.
    /// </summary>
    internal string SignatureKey { get; }

    /// <summary>
    /// Gets the update virtual host.
    /// </summary>
    internal string CustomizedVirtualHost { get; }

    /// <summary>
    /// Gets the public key.
    /// </summary>
    /// <returns>The RSA public key to verify file signature.</returns>
    public virtual ISignatureProvider GetSignatureProvider()
    {
        if (string.IsNullOrWhiteSpace(SignatureAlgorithm)) return null;
        return SignatureAlgorithm.Trim().ToUpperInvariant() switch
        {
            "RS512" => RSASignatureProvider.CreateRS512(SignatureKey),
            "RS384" => RSASignatureProvider.CreateRS384(SignatureKey),
            "RS256" => RSASignatureProvider.CreateRS256(SignatureKey),
            "HS512" => HashSignatureProvider.CreateHS512(SignatureKey),
            "HS384" => HashSignatureProvider.CreateHS384(SignatureKey),
            "HS256" => HashSignatureProvider.CreateHS256(SignatureKey),
            _ => null,
        };
    }
}

/// <summary>
/// The update information of standalone web app.
/// </summary>
public class LocalWebAppPackageUpdateInfo
{
    /// <summary>
    /// Gets or sets the URL of web app package update service.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the variable parameters.
    /// </summary>
    [JsonPropertyName("params")]
    public Dictionary<string, string> VariableParameters { get; set; }

    /// <summary>
    /// Gets or sets the property name of response body.
    /// </summary>
    [JsonPropertyName("prop")]
    public string ResponseProperty { get; set; }

    /// <summary>
    /// Gets or sets the additional settings.
    /// </summary>
    [JsonPropertyName("settings")]
    public JsonObjectNode Settings { get; set; }
}

/// <summary>
/// The local web app assembly embbed resource information.
/// </summary>
public class LocalWebAppEmbbeddedResourceInfo
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppEmbbeddedResourceInfo class.
    /// </summary>
    public LocalWebAppEmbbeddedResourceInfo()
    {
    }

    /// <summary>
    /// Gets or sets the file name of project.
    /// </summary>
    public string ProjectFileName { get; set; }

    /// <summary>
    /// Gets or sets the file name of the package resource.
    /// </summary>
    public string PackageResourceFileName { get; set; }

    /// <summary>
    /// Gets or sets the file name of the private PEM.
    /// </summary>
    public string PemFileName { get; set; }

    /// <summary>
    /// Gets or sets the file name of the package configuration file.
    /// </summary>
    public string PackageConfigurationFileName { get; set; }

    /// <summary>
    /// Creates by folder.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="assembly">The assembly.</param>
    /// <returns>The local web app embbedded resource information instance.</returns>
    public static LocalWebAppEmbbeddedResourceInfo CreateByFolder(string folder, System.Reflection.Assembly assembly = null)
    {
        assembly ??= System.Reflection.Assembly.GetEntryAssembly();
        return new()
        {
            ProjectFileName = CreateFileNameByFolder(folder, GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, "project"), assembly),
            PackageResourceFileName = CreateFileNameByFolder(folder, GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, null, ".zip"), assembly),
            PemFileName = CreateFileNameByFolder(folder, GetSubFileName(LocalWebAppExtensions.DefaultManifestFileName, null, ".pem"), assembly),
            PackageConfigurationFileName = CreateFileNameByFolder(folder, "package.json", assembly)
        };
    }

    private static string CreateFileNameByFolder(string folder, string name, System.Reflection.Assembly assembly)
    {
        var files = assembly.GetManifestResourceNames();
        var fileName = $"{assembly.GetName().Name}.{folder}.{name}";
        if (files.Contains(fileName)) return fileName;
        name = $"{folder}.{name}";
        if (files.Contains(name)) return name;
        name = $".{name}";
        foreach (var n in files)
        {
            if (n.EndsWith(name)) return n;
        }

        return null;
    }

    private static string GetSubFileName(string name, string sub, string ext = null)
    {
        var i = name.LastIndexOf('.');
        if (string.IsNullOrEmpty(ext)) ext = name[i..];
        return string.IsNullOrWhiteSpace(sub) ? string.Concat(name[..i], ext) : string.Concat(name[..i], '.', sub, ext);
    }
}

/// <summary>
/// The package result of local web app.
/// </summary>
public class LocalWebAppPackageResult
{
    internal LocalWebAppPackageResult(LocalWebAppOptions options, DirectoryInfo root, DirectoryInfo app, FileInfo sign, JsonObjectNode details, JsonObjectNode project)
    {
        Options = options;
        RootDirectory = root;
        AppDirectory = app;
        Signature = sign;
        Details = details;
        ProjectConfiguration = project;
    }

    /// <summary>
    /// Gets the options.
    /// </summary>
    public LocalWebAppOptions Options { get; }

    /// <summary>
    /// Gets the root directory.
    /// </summary>
    public DirectoryInfo RootDirectory { get; }

    /// <summary>
    /// Gets the app directory.
    /// </summary>
    public DirectoryInfo AppDirectory { get; }

    /// <summary>
    /// Gets the signature file.
    /// </summary>
    public FileInfo Signature { get; }

    /// <summary>
    /// Gets the details information.
    /// </summary>
    public JsonObjectNode Details { get; }

    /// <summary>
    /// Gets the project configuration.
    /// </summary>
    public JsonObjectNode ProjectConfiguration { get; }
}
