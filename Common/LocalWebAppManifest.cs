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
    /// The tags.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }
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
    public string AppId { get; set; }

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
