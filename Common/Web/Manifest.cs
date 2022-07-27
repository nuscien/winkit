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
    /// Gets or sets the identifier of the app.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the app.
    /// </summary>
    [JsonPropertyName("title")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the icon of the app.
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

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
    /// Gets or sets the JSON data file list.
    /// </summary>
    [JsonPropertyName("dataRes")]
    public Dictionary<string, string> JsonBindings { get; set; }

    /// <summary>
    /// Gets or sets the text data file list.
    /// </summary>
    [JsonPropertyName("strRes")]
    public Dictionary<string, string> TextBindings { get; set; }

    /// <summary>
    /// Gets or sets the host app binding information.
    /// </summary>
    [JsonPropertyName("host")]
    public List<LocalWebAppHostBindingInfo> HostBinding { get; set; }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// Gets or sets the metadata.
    /// </summary>
    [JsonPropertyName("meta")]
    public JsonObjectNode Metadata { get; set; }
}

/// <summary>
/// The host app binding information of standalone web app.
/// </summary>
public class LocalWebAppHostBindingInfo
{
    /// <summary>
    /// Gets or sets the app identifier in store.
    /// </summary>
    [JsonPropertyName("id")]
    public string HostId { get; set; }

    /// <summary>
    /// Gets or sets the kind of the app framework.
    /// </summary>
    [JsonPropertyName("kind")]
    public string FrameworkKind { get; set; }

    /// <summary>
    /// Gets or sets the minimum version.
    /// </summary>
    [JsonPropertyName("min")]
    public string MinimumVersion { get; set; }

    /// <summary>
    /// Gets or sets the maximum version.
    /// </summary>
    [JsonPropertyName("max")]
    public string MaximumVersion { get; set; }
}

/// <summary>
/// The information of the local web app.
/// </summary>
public class LocalWebAppInfo
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppInfo class.
    /// </summary>
    public LocalWebAppInfo()
    {
        CreationTime = DateTime.Now;
        LastModificationTime = DateTime.Now;
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppInfo class.
    /// </summary>
    /// <param name="manifest">The manifest.</param>
    /// <param name="options">The options.</param>
    /// <param name="details">The details.</param>
    public LocalWebAppInfo(LocalWebAppManifest manifest, LocalWebAppOptions options, JsonObjectNode details = null)
        : this()
    {
        Set(manifest);
        Set(options);
        Details = details;
    }

    /// <summary>
    /// Gets or sets the identifier of the app.
    /// </summary>
    [JsonPropertyName("id")]
    public string ResourcePackageId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the app is disabled.
    /// </summary>
    [JsonPropertyName("disable")]
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets or sets the display name of the app.
    /// </summary>
    [JsonPropertyName("title")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the icon of the app.
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

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
    /// Gets or sets the tags.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// Gets or sets the update service information.
    /// </summary>
    [JsonPropertyName("update")]
    public LocalWebAppPackageUpdateInfo Update { get; set; }

    /// <summary>
    /// Gets or sets the signature algorithm.
    /// </summary>
    [JsonPropertyName("signAlg")]
    public string SignatureAlgorithm { get; set; }

    /// <summary>
    /// Gets or sets the public key of signature for verification.
    /// </summary>
    [JsonPropertyName("signKey")]
    public string SignatureKey { get; set; }

    /// <summary>
    /// Gets or sets the details.
    /// </summary>
    [JsonPropertyName("info")]
    public JsonObjectNode Details { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the app is installed.
    /// </summary>
    [JsonPropertyName("installed")]
    public bool Installed { get; set; }

    /// <summary>
    /// Gets or sets the creation date time.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the last modification date time.
    /// </summary>
    [JsonPropertyName("updated")]
    public DateTime LastModificationTime { get; set; }

    /// <summary>
    /// Gets the update virtual host.
    /// </summary>
    [JsonPropertyName("server")]
    public string CustomizedVirtualHost { get; set; }

    /// <summary>
    /// Gets the update virtual host.
    /// </summary>
    [JsonPropertyName("manifest")]
    public string CustomizedManifestFileName { get; set; }

    /// <summary>
    /// Gets the update virtual host.
    /// </summary>
    [JsonPropertyName("local")]
    public string LocalPath { get; set; }

    /// <summary>
    /// Gets signature provider.
    /// </summary>
    /// <returns>The signature provider.</returns>
    public ISignatureProvider GetSignatureProvider()
        => GetOptions()?.GetSignatureProvider();

    /// <summary>
    /// Sets the manifest.
    /// </summary>
    /// <param name="manifest">The manifest.</param>
    public void Set(LocalWebAppManifest manifest)
    {
        if (manifest == null) return;
        ResourcePackageId = manifest.Id;
        DisplayName = manifest.DisplayName;
        Icon = manifest.Icon;
        Version = manifest.Version;
        Description = manifest.Description;
        PublisherName = manifest.PublisherName;
        Copyright = manifest.Copyright;
        Website = manifest.Website;
        Tags = manifest.Tags;
    }

    /// <summary>
    /// Sets the options.
    /// </summary>
    /// <param name="options">The options.</param>
    public void Set(LocalWebAppOptions options)
    {
        if (options == null) return;
        ResourcePackageId = options.ResourcePackageId;
        Update = options.Update;
        SignatureAlgorithm = options.SignatureAlgorithm;
        SignatureKey = options.SignatureKey;
        CustomizedManifestFileName = options.ManifestFileName;
        CustomizedVirtualHost = options.CustomizedVirtualHost;
    }

    /// <summary>
    /// Sets the signature key.
    /// </summary>
    /// <param name="alg">The signature algorithm.</param>
    /// <param name="key">The public key of signature for verification.</param>
    public void SetKey(string alg, string key)
    {
        if (!string.IsNullOrWhiteSpace(alg)) SignatureAlgorithm = alg;
        if (!string.IsNullOrWhiteSpace(key)) SignatureKey = key;
    }

    /// <summary>
    /// Gets options.
    /// </summary>
    /// <returns>The options.</returns>
    public LocalWebAppOptions GetOptions()
        => new(ResourcePackageId, SignatureAlgorithm, SignatureKey, Update, CustomizedManifestFileName, CustomizedVirtualHost);
}
