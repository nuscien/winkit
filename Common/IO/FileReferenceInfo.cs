using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Trivial.IO;

/// <summary>
/// The reference information of storage folder.
/// </summary>
public class StorageFolderReferenceInfo : BaseDirectoryReferenceInfo<StorageFolder>, IDirectoryHostReferenceInfo
{
    IFileContainerReferenceInfo parent;

    /// <summary>
    /// Initializes a new instance of the StorageFolderReferenceInfo class.
    /// </summary>
    /// <param name="directory">The directory instance.</param>
    /// <param name="parent">The parent folder.</param>
    public StorageFolderReferenceInfo(StorageFolder directory, StorageFolderReferenceInfo parent = null) : this(directory, parent, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the StorageFolderReferenceInfo class.
    /// </summary>
    /// <param name="directory">The directory instance.</param>
    /// <param name="parent">The parent folder.</param>
    /// <param name="getProperties">true if get properties immediately; otherwise, false.</param>
    internal StorageFolderReferenceInfo(StorageFolder directory, StorageFolderReferenceInfo parent, bool getProperties) : base(directory)
    {
        Exists = directory != null;
        this.parent = parent;
        if (getProperties) _ = GetPropertiesAsync();
    }

    /// <summary>
    /// Gets the date created.
    /// </summary>
    public DateTime Creation { get; private set; }

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <returns>The directory collection.</returns>
    public Task<IReadOnlyList<StorageFolderReferenceInfo>> GetDirectoriesAsync()
        => GetDirectoriesAsync(null);

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="predicate">An optional function to test each element for a condition.</param>
    /// <returns>The directory collection.</returns>
    public async Task<IReadOnlyList<StorageFolderReferenceInfo>> GetDirectoriesAsync(Func<StorageFolder, bool> predicate)
    {
        var dir = Source;
        if (dir == null) return new List<StorageFolderReferenceInfo>();
        IEnumerable<StorageFolder> col = await dir.GetFoldersAsync();
        if (col == null) return new List<StorageFolderReferenceInfo>();
        if (predicate != null) col = col.Where(predicate);
        return col.Select(ele => new StorageFolderReferenceInfo(ele, this)).ToList();
    }

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    public Task<IReadOnlyList<StorageFileReferenceInfo>> GetFilesAsync()
        => GetFilesAsync(null);

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="predicate">An optional function to test each element for a condition.</param>
    /// <returns>The file collection.</returns>
    public async Task<IReadOnlyList<StorageFileReferenceInfo>> GetFilesAsync(Func<StorageFile, bool> predicate)
    {
        var dir = Source;
        if (dir == null) return new List<StorageFileReferenceInfo>();
        IEnumerable<StorageFile> col = await dir.GetFilesAsync();
        if (col == null) return new List<StorageFileReferenceInfo>();
        if (predicate != null) col = col.Where(predicate);
        return col.Select(ele => new StorageFileReferenceInfo(ele, this)).ToList();
    }

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    Task<IReadOnlyList<IFileReferenceInfo>> IDirectoryHostReferenceInfo.GetFilesAsync()
        => GetFilesAsync(Source, this);

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <returns>The directory collection.</returns>
    Task<IReadOnlyList<IDirectoryReferenceInfo>> IDirectoryHostReferenceInfo.GetDirectoriesAsync()
        => GetDirectoriesAsync(Source, this);

    /// <summary>
    /// Gets the parent.
    /// </summary>
    public async Task<StorageFolderReferenceInfo> GetParentAsync()
        => await GetParentInternalAsync() as StorageFolderReferenceInfo;

    /// <summary>
    /// Gets the parent.
    /// </summary>
    Task<IFileContainerReferenceInfo> IDirectoryHostReferenceInfo.GetParentAsync()
        => GetParentInternalAsync();

    /// <summary>
    /// Gets properties.
    /// </summary>
    /// <returns>The properties.</returns>
    internal async Task<BasicProperties> GetPropertiesAsync()
    {
        var info = Source;
        if (info == null) return null;
        try
        {
            Name = info.Name;
            Creation = LastModification = info.DateCreated.DateTime;
            var properties = await info.GetBasicPropertiesAsync();
            LastModification = properties.DateModified.DateTime;
            return properties;
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (IOException)
        {
        }
        catch (NullReferenceException)
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
    /// Converts a number to angle model.
    /// </summary>
    /// <param name="value">The raw value.</param>
    public static explicit operator LocalDirectoryReferenceInfo(StorageFolderReferenceInfo value)
    {
        try
        {
            var path = value?.Source?.Path;
            if (string.IsNullOrWhiteSpace(path)) return null;
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) return null;
            return new LocalDirectoryReferenceInfo(dir);
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    private async Task<IFileContainerReferenceInfo> GetParentInternalAsync()
    {
        if (parent != null) return parent;
        try
        {
            var dir = await Source.GetParentAsync();
            if (dir == null) return null;
            parent = new StorageFolderReferenceInfo(dir);
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
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ExternalException)
        {
        }

        return parent;
    }

    internal async static Task<IReadOnlyList<IDirectoryReferenceInfo>> GetDirectoriesAsync(StorageFolder dir, StorageFolderReferenceInfo parent)
    {
        if (dir == null) return new List<IDirectoryReferenceInfo>();
        if (parent == null) parent = new StorageFolderReferenceInfo(dir);
        var folders = await dir.GetFoldersAsync();
        var list = new List<IDirectoryReferenceInfo>();
        if (folders == null) return list;
        foreach (var folder in folders)
        {
            var info = new StorageFolderReferenceInfo(folder, parent, false);
            await info.GetPropertiesAsync();
            list.Add(info);
        }

        return list;
    }

    internal async static Task<IReadOnlyList<IFileReferenceInfo>> GetFilesAsync(StorageFolder dir, StorageFolderReferenceInfo parent)
    {
        if (dir == null) return new List<IFileReferenceInfo>();
        if (parent == null) parent = new StorageFolderReferenceInfo(dir);
        var files = await dir.GetFilesAsync();
        var list = new List<IFileReferenceInfo>();
        if (files == null) return list;
        foreach (var file in files)
        {
            var info = new StorageFileReferenceInfo(file, parent, false);
            await info.GetPropertiesAsync();
            list.Add(info);
        }

        return list;
    }
}

/// <summary>
/// The reference information of file.
/// </summary>
public class StorageFileReferenceInfo : BaseFileReferenceInfo<StorageFile>
{
    private bool isLoading;

    /// <summary>
    /// Initializes a new instance of the StorageFileReferenceInfo class.
    /// </summary>
    /// <param name="file">The file item.</param>
    /// <param name="parent">The parent folder.</param>
    public StorageFileReferenceInfo(StorageFile file, StorageFolderReferenceInfo parent = null) : this(file, parent, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the StorageFileReferenceInfo class.
    /// </summary>
    /// <param name="file">The file item.</param>
    /// <param name="parent">The parent folder.</param>
    /// <param name="getProperties">true if get properties immediately; otherwise, false.</param>
    internal StorageFileReferenceInfo(StorageFile file, StorageFolderReferenceInfo parent, bool getProperties) : base(parent, file)
    {
        Exists = file != null && file.IsAvailable;
        if (parent == null) _ = UpdateParentAsync();
        if (getProperties) _ = GetPropertiesAsync();
    }

    /// <summary>
    /// Gets the date created.
    /// </summary>
    public DateTime Creation { get; private set; }

    /// <summary>
    /// Gets the file attributes.
    /// </summary>
    public Windows.Storage.FileAttributes Attributes { get; private set; }

    /// <summary>
    /// Retrieves an adjusted thumbnail image for the file, determined by the purpose of the thumbnail, the requested size, and the specified options.
    /// </summary>
    /// <param name="mode">The enum value that describes the purpose of the thumbnail and determines how the thumbnail image is adjusted.</param>
    /// <returns>When this method completes successfully, it returns a thumbnail that represents the thumbnail image; or null, if there is no thumbnail image associated with the file.</returns>
    public async Task<StorageItemThumbnail> GetThumbnailAsync(ThumbnailMode mode)
    {
        var r = Source;
        if (r == null) return null;
        return await Source.GetThumbnailAsync(mode);
    }

    /// <summary>
    /// Retrieves an adjusted thumbnail image for the file, determined by the purpose of the thumbnail, the requested size, and the specified options.
    /// </summary>
    /// <param name="mode">The enum value that describes the purpose of the thumbnail and determines how the thumbnail image is adjusted.</param>
    /// <param name="requestedSize">The requested size, in pixels, of the longest edge of the thumbnail. Windows uses this size as a guide and tries to scale the thumbnail image without reducing the quality of the image.</param>
    /// <param name="options">The enum value that describes the desired behavior to use to retrieve the thumbnail image. The specified behavior might affect the size and/or quality of the image and how quickly the thumbnail image is retrieved.</param>
    /// <returns>When this method completes successfully, it returns a thumbnail that represents the thumbnail image; or null, if there is no thumbnail image associated with the file.</returns>
    public async Task<StorageItemThumbnail> GetThumbnailAsync(ThumbnailMode mode, uint requestedSize, ThumbnailOptions options = ThumbnailOptions.None)
    {
        var r = Source;
        if (r == null) return null;
        return await Source.GetThumbnailAsync(mode, requestedSize, options);
    }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    public new StorageFolderReferenceInfo GetParent()
        => base.GetParent() as StorageFolderReferenceInfo;

    /// <summary>
    /// Gets the parent.
    /// </summary>
    protected override IFileContainerReferenceInfo GetParentInfo()
    {
        var info = base.GetParentInfo();
        if (info == null && !isLoading) _ = UpdateParentAsync();
        return base.GetParentInfo();
    }

    /// <summary>
    /// Gets properties.
    /// </summary>
    /// <returns>The properties.</returns>
    internal async Task<BasicProperties> GetPropertiesAsync()
    {
        var info = Source;
        if (info == null || !info.IsAvailable) return null;
        BasicProperties properties = null;
        try
        {
            isLoading = true;
            Name = info.Name;
            Creation = LastModification = info.DateCreated.DateTime;
            Attributes = info.Attributes;
            properties = await info.GetBasicPropertiesAsync();
            LastModification = properties.DateModified.DateTime;
            try
            {
                Size = (long)properties.Size;
            }
            catch (OverflowException)
            {
            }
            catch (InvalidCastException)
            {
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (IOException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (ExternalException)
        {
        }
        finally
        {
            isLoading = false;
        }

        await UpdateParentAsync();
        return properties;
    }

    /// <summary>
    /// Converts a number to angle model.
    /// </summary>
    /// <param name="value">The raw value.</param>
    public static explicit operator LocalFileReferenceInfo(StorageFileReferenceInfo value)
    {
        try
        {
            var path = value?.Source?.Path;
            if (string.IsNullOrWhiteSpace(path)) return null;
            var file = new FileInfo(path);
            if (!file.Exists) return null;
            return new LocalFileReferenceInfo(file);
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    private async Task UpdateParentAsync()
    {
        var info = Source;
        if (info == null) return;
        try
        {
            isLoading = true;
            var dir = await info.GetParentAsync();
            if (dir != null) SetParent(new StorageFolderReferenceInfo(dir));
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
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ExternalException)
        {
        }
        finally
        {
            isLoading = false;
        }
    }
}
