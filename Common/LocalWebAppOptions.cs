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
/// The options of the standalone web app resource package.
/// </summary>
public class LocalWebAppOptions
{
    /// <summary>
    /// Gets or sets the file name of the manifest.
    /// </summary>
    public string ManifestFileName { get; set; } = "edgeplatform.json";

    /// <summary>
    /// Gets or sets the app resource package identifier.
    /// </summary>
    public string ResourcePackageId { get; set; }

    /// <summary>
    /// Gets or sets the public key.
    /// </summary>
    public RSAParameters PublicKey { get; set; }

    /// <summary>
    /// Gets or sets the hash algorithm name.
    /// </summary>
    public HashAlgorithmName HashAlgorithmName { get; set; } = HashAlgorithmName.SHA512;

    /// <summary>
    /// Gets or sets the update information.
    /// </summary>
    public WebAppPackageUpdateInfo Update { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether it is in debug mode to ignore any signature verification and enable Microsoft Edge DevTools.
    /// </summary>
    public bool IsDevEnvironmentEnabled { get; set; }
}

/// <summary>
/// The update information of standalone web app.
/// </summary>
public class WebAppPackageUpdateInfo
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
}
