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
    private static async Task<DirectoryInfo> TryGetPackageFolderAsync(string appId)
    {
        try
        {
            var appDataFolder = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("LocalWebApp", Windows.Storage.CreationCollisionOption.OpenIfExists);
            appDataFolder = await appDataFolder.CreateFolderAsync(appId, Windows.Storage.CreationCollisionOption.OpenIfExists);
            return FileSystemInfoUtility.TryGetDirectoryInfo(appDataFolder.Path);
        }
        catch (IOException)
        {
        }
        catch (NotSupportedException)
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
        catch (ExternalException)
        {
        }

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
            var appDataFolder = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("LocalWebApp", Windows.Storage.CreationCollisionOption.OpenIfExists);
            appDataFolder = await appDataFolder.GetFolderAsync(appId);
            if (appDataFolder != null) await appDataFolder.DeleteAsync();
        }
        catch (IOException)
        {
        }
    }
}
