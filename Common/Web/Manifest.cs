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
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppInfo class.
    /// </summary>
    /// <param name="manifest">The manifest.</param>
    /// <param name="name">The display name.</param>
    /// <param name="signAlg">The signature algorithm.</param>
    /// <param name="signKey">The public signature key for verification.</param>
    public LocalWebAppInfo(LocalWebAppManifest manifest, string name, string signKey = null, string signAlg = null)
    {
        DisplayName = name;
        SignatureKey = signKey;
        SignatureAlgorithm = signAlg;
        if (manifest == null) return;
        ResourcePackageId = manifest.Id;
        Icon = manifest.Icon;
        Version = manifest.Version;
        Description = manifest.Description;
        PublisherName = manifest.PublisherName;
        Copyright = manifest.Copyright;
        Website = manifest.Website;
        Tags = manifest.Tags;
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
    /// Gets signature provider.
    /// </summary>
    /// <returns>The signature provider.</returns>
    public ISignatureProvider GetSignatureProvider()
    {
        if (string.IsNullOrWhiteSpace(SignatureAlgorithm) || string.IsNullOrWhiteSpace(SignatureKey)) return null;
        return SignatureAlgorithm.Trim().ToLowerInvariant() switch
        {
            "rs512" => RSASignatureProvider.CreateRS512(SignatureKey),
            "rs384" => RSASignatureProvider.CreateRS384(SignatureKey),
            "rs256" => RSASignatureProvider.CreateRS256(SignatureKey),
            _ => null,
        };
    }

    /// <summary>
    /// Gets options.
    /// </summary>
    /// <param name="hostId">The identifier of the host app.</param>
    /// <returns>The options.</returns>
    public LocalWebAppOptions GetOptions(string hostId)
        => new(hostId, ResourcePackageId, GetSignatureProvider(), Update);
}
