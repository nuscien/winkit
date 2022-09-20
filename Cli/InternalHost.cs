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
    /// <summary>
    /// Removes a specific resource package.
    /// </summary>
    /// <param name="appId">The resource package identifier.</param>
    /// <returns>true if the resource package exists and is removed; otherwise, false.</returns>
    private static DirectoryInfo GetPackageFolder(string appId)
        => Directory.CreateDirectory(Path.Combine("AppData", "LocalWebApp", appId));

    /// <summary>
    /// Removes a specific resource package.
    /// </summary>
    /// <param name="appId">The resource package identifier.</param>
    /// <returns>true if the resource package exists and is removed; otherwise, false.</returns>
    private static async Task<DirectoryInfo> TryGetPackageFolderAsync(string appId)
    {
        try
        {
            return GetPackageFolder(appId);
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

        await Task.Run(() => { });
        return null;
    }

    /// <summary>
    /// Removes a specific resource package.
    /// </summary>
    /// <param name="appId">The resource package identifier.</param>
    /// <returns>true if the resource package exists and is removed; otherwise, false.</returns>
    private static async Task TryRemovePackageFolderAsync(string appId)
    {
        try
        {
            var dir = GetPackageFolder(appId);
            dir.Delete(true);
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

        await Task.Run(() => { });
    }
}
