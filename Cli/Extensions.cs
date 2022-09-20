using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.IO;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace Trivial.UI;

internal static class LocalWebAppExtensions
{
    internal const string DefaultManifestFileName = "localwebapp.json";
    internal const string DefaultManifestGeneratedFileName = "localwebapp.files.json";
    internal const string VirtualRootDomain = "localwebapp.localhost";

    /// <summary>
    /// Tries to parse a URI.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The URI.</returns>
    public static Uri TryCreateUri(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            return new Uri(url);
        }
        catch (FormatException)
        {
        }
        catch (ArgumentException)
        {
        }

        return null;
    }
}
