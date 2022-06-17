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
    private readonly ISignatureProvider defaultSign;

    /// <summary>
    /// Initializes a new instance of the LocalWebAppOptions class.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier.</param>
    /// <param name="signatureProvider">The signature provider.</param>
    /// <param name="update">The update service information.</param>
    public LocalWebAppOptions(string resourcePackageId, ISignatureProvider signatureProvider, WebAppPackageUpdateInfo update = null)
        : this(resourcePackageId, signatureProvider, update, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppOptions class.
    /// </summary>
    /// <param name="resourcePackageId">The resource package identifier.</param>
    /// <param name="signatureProvider">The signature provider.</param>
    /// <param name="update">The update service information.</param>
    /// <param name="manifestFileName">The file name of the manifest.</param>
    public LocalWebAppOptions(string resourcePackageId, ISignatureProvider signatureProvider, WebAppPackageUpdateInfo update, string manifestFileName)
    {
        ResourcePackageId = resourcePackageId;
        defaultSign = signatureProvider;
        Update = update;
        if (string.IsNullOrWhiteSpace(manifestFileName)) manifestFileName = "edgeplatform.json";
        ManifestFileName = manifestFileName;
    }

    /// <summary>
    /// Gets or sets the file name of the manifest.
    /// </summary>
    public string ManifestFileName { get; }

    /// <summary>
    /// Gets or sets the app resource package identifier.
    /// </summary>
    public string ResourcePackageId { get; }

    /// <summary>
    /// Gets or sets the update service information.
    /// </summary>
    public WebAppPackageUpdateInfo Update { get; }

    /// <summary>
    /// Gets the public key.
    /// </summary>
    /// <param name="signKey">The key of file signature mapping.</param>
    /// <returns>The RSA public key to verify file signature.</returns>
    public virtual ISignatureProvider GetPublicKey(string signKey)
        => defaultSign;
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
