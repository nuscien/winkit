using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Trivial.Data;
using Trivial.Text;
using System.Security.Cryptography;

namespace Trivial.Web;

/// <summary>
/// The source type of standalone web app file item.
/// </summary>
public enum LocalWebAppFileSourceType
{
    /// <summary>
    /// Not set.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// Embedded resource.
    /// </summary>
    Embedded = 1,

    /// <summary>
    /// Online resource.
    /// </summary>
    Online = 2,

    /// <summary>
    /// Localhost
    /// </summary>
    Localhost = 3,

    /// <summary>
    /// Not supported.
    /// </summary>
    NotSupported = 4
}

/// <summary>
/// The file item information of standalone web app.
/// </summary>
public class LocalWebAppFileInfo
{
    private string originalPath;

    /// <summary>
    /// Initializes a new instance of the WebAppPackageFileInfo class.
    /// </summary>
    public LocalWebAppFileInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the WebAppPackageFileInfo class.
    /// </summary>
    /// <param name="path">The relative path.</param>
    public LocalWebAppFileInfo(string path)
    {
        Path = path;
    }

    /// <summary>
    /// Gets or sets the relative path of the file item.
    /// </summary>
    [JsonPropertyName("src")]
    public string Path
    {
        get
        {
            return originalPath;
        }

        set
        {
            originalPath = value;
            Init();
        }
    }

    /// <summary>
    /// Gets the path formatted.
    /// </summary>
    [JsonIgnore]
    public string FormattedPath { get; private set; }

    /// <summary>
    /// Gets the source type.
    /// </summary>
    [JsonIgnore]
    public LocalWebAppFileSourceType SourceType { get; private set; } = LocalWebAppFileSourceType.Empty;

    /// <summary>
    /// Gets or sets the Base64Url encoded signature of the file item.
    /// </summary>
    [JsonPropertyName("sign")]
    public string Signature { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Verifies by signature.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    /// <returns>true if pass; otherwise, false.</returns>
    public bool Verify(LocalWebAppHost host)
        => Verify(host, out _);

    /// <summary>
    /// Verifies by signature.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    /// <param name="file">The file output.</param>
    /// <returns>true if pass; otherwise, false.</returns>
    public bool Verify(LocalWebAppHost host, out FileInfo file)
    {
        if (host == null)
        {
            file = null;
            return false;
        }

        var path = FormattedPath;
        var virtualHost = host.VirtualHost;
        if (!string.IsNullOrWhiteSpace(virtualHost))
        {
            switch (SourceType)
            {
                case LocalWebAppFileSourceType.Localhost:
                    var testStr = string.Concat("//:", virtualHost, "/");
                    var testIndex = path.IndexOf(testStr);
                    if (testIndex >= 0) path = path[(testIndex + testStr.Length)..];
                    break;
                case LocalWebAppFileSourceType.Online:
                    file = null;
                    return true;
                case LocalWebAppFileSourceType.Embedded:
                    break;
                default:
                    file = null;
                    return false;
            }
        }

        if (string.IsNullOrEmpty(path) || path.Contains("://"))
        {
            file = null;
            return false;
        }

        var rootPath = host.ResourcePackageDirectory;
        try
        {
            if (rootPath == null) rootPath = new DirectoryInfo(Environment.CurrentDirectory);
            if (!rootPath.Exists)
            {
                file = null;
                return false;
            }

            file = new FileInfo(System.IO.Path.Combine(rootPath.FullName, path));
            return Verify(file, Signature, host);
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

        file = null;
        return false;
    }

    /// <summary>
    /// Tries to get online path.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    /// <returns></returns>
    public string TryGetOnlinePath(LocalWebAppHost host = null)
    {
        return SourceType switch
        {
            LocalWebAppFileSourceType.Online or LocalWebAppFileSourceType.Localhost => FormattedPath,
            LocalWebAppFileSourceType.Embedded => host?.GetVirtualPath(FormattedPath),
            _ => null,
        };
    }

    private void Init()
    {
        var path = originalPath;
        var s = path;
        if (string.IsNullOrEmpty(s))
        {
            SourceType = LocalWebAppFileSourceType.NotSupported;
            FormattedPath = s;
            return;
        }

        s = s.Trim();
        if (s.StartsWith("//"))
        {
            var i = s.IndexOf('/', 2);
            var j = s.IndexOf('.', 2);
            if (i > 6 && j > 3 && i - j > 2)
            {
                var localhost = s.IndexOf("/localhost/");
                if (localhost < 0) localhost = s.IndexOf(".localhost/");
                if (localhost > 0 && localhost < i)
                {
                    SourceType = LocalWebAppFileSourceType.Localhost;
                    FormattedPath = string.Concat("http", s);
                    return;
                }

                SourceType = LocalWebAppFileSourceType.Online;
                FormattedPath = string.Concat("https", s);
                return;
            }

            SourceType = LocalWebAppFileSourceType.NotSupported;
            FormattedPath = s;
            return;
        }

        if (s.Contains("://"))
        {
            if (s.StartsWith("http:/") || s.StartsWith("https:/"))
            {
                var localhost = s.IndexOf("localhost/");
                if (localhost < 0) localhost = s.IndexOf(".localhost/");
                FormattedPath = s;
                SourceType = localhost > 0 && localhost < s.IndexOf('/', 10)
                    ? LocalWebAppFileSourceType.Localhost
                    : LocalWebAppFileSourceType.Online;
                return;
            }

            SourceType = LocalWebAppFileSourceType.NotSupported;
            FormattedPath = s;
            return;
        }

        if (s.StartsWith("./")) s = s[2..];
        else if (s.StartsWith("~/")) s = s[2..];
        else if (s.StartsWith(".\\")) s = s[2..];
        else if (s.StartsWith("~\\")) s = s[2..];
        if (s.StartsWith('\\')) s = s[1..];
        if (s.StartsWith('/')) s = s[1..];
        if (path != originalPath) return;
        SourceType = LocalWebAppFileSourceType.Embedded;
        FormattedPath = s.TrimStart();
    }

    /// <summary>
    /// Verifies by signature.
    /// </summary>
    /// <param name="file">The file to test.</param>
    /// <param name="signature">The signature.</param>
    /// <param name="host">The standalone web app host.</param>
    /// <returns>true if pass; otherwise, false.</returns>
    internal static bool Verify(FileInfo file, string signature, LocalWebAppHost host)
    {
        if (file == null || !file.Exists) return false;
        try
        {
            if (string.IsNullOrWhiteSpace(signature)) return file.Length < 1;
            var signatureProvider = host.SignatureProvider;
            if (signatureProvider == null) return false;
            var sign = WebFormat.Base64UrlDecode(signature);
            using var stream = host.TryReadFile(file);
            if (stream == null) return false;
            return signatureProvider.Verify(stream, sign);
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

        return false;
    }
}

/// <summary>
/// The file collection of the local standalone web app.
/// </summary>
public class LocalWebAppFileCollection
{
    /// <summary>
    /// The file list of HTML, JavaScript, Type Script, JSON and CSS.
    /// </summary>
    [JsonPropertyName("files")]
    public List<LocalWebAppFileInfo> Files { get; set; }
}

/// <summary>
/// The JSON converter of the LocalWebAppFileInfoJsonConverter class.
/// </summary>
internal class LocalWebAppFileInfoJsonConverter : JsonConverter<LocalWebAppFileInfo>
{
    /// <inheritdoc />
    public override LocalWebAppFileInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null || reader.TokenType == JsonTokenType.False)
            return null;
        if (reader.TokenType == JsonTokenType.String)
            return new(reader.GetString());
        if (reader.TokenType != JsonTokenType.StartObject)
            return null;
        reader.Read();
        var json = JsonObjectNode.ParseValue(ref reader);
        if (json == null) return null;
        var src = json.TryGetStringValue("src") ?? json.TryGetStringValue("path");
        if (string.IsNullOrWhiteSpace(src)) return null;
        return new(src);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, LocalWebAppFileInfo value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        WriteStringProperty(writer, "src", value.Path);
        WriteStringProperty(writer, "sign", value.Signature);
        WriteStringProperty(writer, "description", value.Description);
        writer.WriteEndObject();
    }

    private static bool WriteStringProperty(Utf8JsonWriter writer, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return false;
        writer.WritePropertyName(key);
        writer.WriteStringValue(value);
        return true;
    }
}