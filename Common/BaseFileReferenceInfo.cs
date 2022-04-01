using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.IO;

/// <summary>
/// The client for loading file reference information.
/// </summary>
public interface IFileReferenceClient
{
    /// <summary>
    /// Tests if supports the directory reference.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>true if supports; otherwise, false.</returns>
    bool Test(IFileContainerReferenceInfo directory);

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The file collection.</returns>
    Task<IReadOnlyList<IFileReferenceInfo>> GetFilesAsync(IFileContainerReferenceInfo directory);

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The directory collection.</returns>
    Task<IReadOnlyList<IDirectoryReferenceInfo>> GetDirectoriesAsync(IFileContainerReferenceInfo directory);

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="directory">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    Task<IFileContainerReferenceInfo> GetParentAsync(IFileContainerReferenceInfo directory);
}

/// <summary>
/// The client for loading file reference information.
/// </summary>
public abstract class BaseFileReferenceClient<T> : IFileReferenceClient where T : IDirectoryReferenceInfo
{
    /// <summary>
    /// Tests if supports the directory reference.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>true if supports; otherwise, false.</returns>
    public abstract bool Test(T directory);

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The file collection.</returns>
    public abstract Task<IReadOnlyList<IFileReferenceInfo>> GetFilesAsync(T directory);

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The directory collection.</returns>
    public abstract Task<IReadOnlyList<T>> GetDirectoriesAsync(T directory);

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="directory">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    public abstract Task<T> GetParentAsync(T directory);

    /// <summary>
    /// Tests if supports the directory reference.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>true if supports; otherwise, false.</returns>
    bool IFileReferenceClient.Test(IFileContainerReferenceInfo directory)
    {
        if (directory is not T info) return false;
        return Test(info);
    }

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The file collection.</returns>
    async Task<IReadOnlyList<IFileReferenceInfo>> IFileReferenceClient.GetFilesAsync(IFileContainerReferenceInfo directory)
    {
        if (directory is not T info) return new List<IFileReferenceInfo>();
        return await GetFilesAsync(info) ?? new List<IFileReferenceInfo>();
    }

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The directory collection.</returns>
    async Task<IReadOnlyList<IDirectoryReferenceInfo>> IFileReferenceClient.GetDirectoriesAsync(IFileContainerReferenceInfo directory)
    {
        var col = new List<IDirectoryReferenceInfo>();
        if (directory is not T info) return col;
        var list = await GetDirectoriesAsync(info);
        if (list == null) return col;
        foreach (var item in list)
        {
            col.Add(item);
        }

        return col;
    }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="directory">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    async Task<IFileContainerReferenceInfo> IFileReferenceClient.GetParentAsync(IFileContainerReferenceInfo directory)
    {
        if (directory is not T info) return null;
        return await GetParentAsync(info);
    }
}

/// <summary>
/// The file reference client factory.
/// </summary>
public class FileReferenceClientFactory : IFileReferenceClient
{
    private readonly Dictionary<Type, IFileReferenceClient> handlers = new();

    /// <summary>
    /// The singleton.
    /// </summary>
    public static readonly FileReferenceClientFactory Instance = new();

    /// <summary>
    /// Registers the client handler.
    /// </summary>
    /// <typeparam name="T">The type of client.</typeparam>
    /// <param name="client">The client.</param>
    public void Register<T>(BaseFileReferenceClient<T> client) where T : IDirectoryReferenceInfo
        => Register(typeof(T), client);

    /// <summary>
    /// Registers the client handler.
    /// </summary>
    /// <typeparam name="T">The type of client.</typeparam>
    /// <param name="client">The client.</param>
    public void Register<T>(IFileReferenceClient client) where T : IDirectoryReferenceInfo
        => Register(typeof(T), client);

    /// <summary>
    /// Registers the client handler.
    /// </summary>
    /// <param name="type">The type to register.</param>
    /// <param name="client">The client.</param>
    public void Register(Type type, IFileReferenceClient client)
    {
        if (client == null) handlers.Remove(type);
        else handlers[type] = client;
    }

    /// <summary>
    /// Removes the client handler.
    /// </summary>
    /// <typeparam name="T">The type of client.</typeparam>
    public void Remove<T>()
        => handlers.Remove(typeof(T));

    /// <summary>
    /// Removes the client handler.
    /// </summary>
    /// <param name="type">The type to unregister.</param>
    public void Remove(Type type)
        => handlers.Remove(type);

    /// <summary>
    /// Tests if supports the directory reference.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>true if supports; otherwise, false.</returns>
    public bool Test(IFileContainerReferenceInfo directory)
    {
        if (directory == null) return false;
        if (directory is IDirectoryHostReferenceInfo) return true;
        var type = directory.GetType();
        return handlers.ContainsKey(type) || directory.Source is DirectoryInfo;
    }

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The file collection.</returns>
    public async Task<IReadOnlyList<IFileReferenceInfo>> GetFilesAsync(IFileContainerReferenceInfo directory)
    {
        if (directory == null) return new List<IFileReferenceInfo>();
        var type = directory.GetType();
        if (handlers.TryGetValue(type, out var h) && h.Test(directory))
            return await h.GetFilesAsync(directory) ?? new List<IFileReferenceInfo>();
        if (directory is IDirectoryHostReferenceInfo dir) return await dir.GetFilesAsync() ?? new List<IFileReferenceInfo>();
        return await LocalDirectoryReferenceInfo.GetFilesAsync(directory.Source as DirectoryInfo, null);
    }

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The directory collection.</returns>
    public async Task<IReadOnlyList<IDirectoryReferenceInfo>> GetDirectoriesAsync(IFileContainerReferenceInfo directory)
    {
        if (directory == null) return new List<IDirectoryReferenceInfo>();
        var type = directory.GetType();
        if (handlers.TryGetValue(type, out var h) && h.Test(directory))
            return await h.GetDirectoriesAsync(directory) ?? new List<IDirectoryReferenceInfo>();
        if (directory is IDirectoryHostReferenceInfo dir) return await dir.GetDirectoriesAsync() ?? new List<IDirectoryReferenceInfo>();
        return await LocalDirectoryReferenceInfo.GetDirectoriesAsync(directory.Source as DirectoryInfo, null);
    }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="directory">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    public async Task<IFileContainerReferenceInfo> GetParentAsync(IFileContainerReferenceInfo directory)
    {
        if (directory == null) return null;
        var type = directory.GetType();
        if (handlers.TryGetValue(type, out var h) && h.Test(directory))
            return await h.GetParentAsync(directory);
        if (directory is IDirectoryHostReferenceInfo dir) return await dir.GetParentAsync();
        if (directory.Source is not DirectoryInfo info) return null;
        return new LocalDirectoryReferenceInfo(info);
    }
}

/// <summary>
/// The reference information of file.
/// </summary>
public interface IFileSystemReferenceInfo
{
    /// <summary>
    /// Gets the file name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the item exists.
    /// </summary>
    bool Exists { get; }

    /// <summary>
    /// Gets the file size.
    /// </summary>
    DateTime LastModification { get; }

    /// <summary>
    /// Gets the instance source for reference.
    /// </summary>
    object Source { get; }
}

/// <summary>
/// The reference information of file.
/// </summary>
public interface IFileReferenceInfo : IFileSystemReferenceInfo
{
    /// <summary>
    /// Gets or sets the file size.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <returns>The parent reference information instance.</returns>
    IFileContainerReferenceInfo GetParent();
}

/// <summary>
/// The reference information of file container.
/// </summary>
public interface IFileContainerReferenceInfo : IFileSystemReferenceInfo
{
}

/// <summary>
/// The reference information of directory.
/// </summary>
public interface IDirectoryReferenceInfo : IFileContainerReferenceInfo
{
}

/// <summary>
/// The reference information of directory with host.
/// </summary>
public interface IDirectoryHostReferenceInfo : IDirectoryReferenceInfo
{
    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <returns>The directory collection.</returns>
    Task<IReadOnlyList<IDirectoryReferenceInfo>> GetDirectoriesAsync();

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    Task<IReadOnlyList<IFileReferenceInfo>> GetFilesAsync();

    /// <summary>
    /// Gets the parent.
    /// </summary>
    Task<IFileContainerReferenceInfo> GetParentAsync();
}

/// <summary>
/// The reference information of file.
/// </summary>
public class BaseFileSystemReferenceInfo : IFileSystemReferenceInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseFileSystemReferenceInfo class.
    /// </summary>
    protected BaseFileSystemReferenceInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileSystemReferenceInfo class.
    /// </summary>
    /// <param name="source">The instance source for reference.</param>
    protected BaseFileSystemReferenceInfo(object source)
    {
        Source = source;
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileSystemReferenceInfo class.
    /// </summary>
    /// <param name="name">The file name.</param>
    /// <param name="lastModification">The last modification time.</param>
    /// <param name="exists">true if exists; otherwise, false.</param>
    /// <param name="source">The instance source for reference.</param>
    public BaseFileSystemReferenceInfo(string name, DateTime lastModification, object source = null, bool exists = true)
    {
        Name = name;
        LastModification = lastModification;
        Source = source;
        Exists = exists;
    }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the item exists.
    /// </summary>
    public bool Exists { get; protected set; }

    /// <summary>
    /// Gets the file size.
    /// </summary>
    public DateTime LastModification { get; protected set; }

    /// <summary>
    /// Gets the instance source for reference.
    /// </summary>
    public object Source { get; internal set; }
}

/// <summary>
/// The reference information of directory.
/// </summary>
public class BaseDirectoryReferenceInfo : BaseFileSystemReferenceInfo, IDirectoryReferenceInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseDirectoryReferenceInfo class.
    /// </summary>
    protected BaseDirectoryReferenceInfo() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseDirectoryReferenceInfo class.
    /// </summary>
    /// <param name="source">The instance source for reference.</param>
    protected BaseDirectoryReferenceInfo(object source) : base(source)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseDirectoryReferenceInfo class.
    /// </summary>
    /// <param name="name">The file name.</param>
    /// <param name="lastModification">The last modification time.</param>
    /// <param name="source">The instance source for reference.</param>
    /// <param name="exists">true if exists; otherwise, false.</param>
    public BaseDirectoryReferenceInfo(string name, DateTime lastModification, object source = null, bool exists = true)
        : base(name, lastModification, source, exists)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseDirectoryReferenceInfo class.
    /// </summary>
    /// <param name="directory">The directory item.</param>
    public BaseDirectoryReferenceInfo(DirectoryInfo directory) : base()
    {
        Source = directory;
        if (directory == null) return;
        if (!directory.Exists)
        {
            try
            {
                Name = directory.Name;
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
            catch (NullReferenceException)
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

            return;
        }

        Name = directory.Name;
        Exists = true;
        try
        {
            LastModification = directory.LastWriteTime;
            return;
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

        Exists = false;
    }
}

/// <summary>
/// The reference information of file.
/// </summary>
public class BaseFileReferenceInfo : BaseFileSystemReferenceInfo, IFileReferenceInfo
{
    private IFileContainerReferenceInfo parent;

    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    protected BaseFileReferenceInfo() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    /// <param name="parent">The parent directory.</param>
    /// <param name="source">The instance source for reference.</param>
    protected BaseFileReferenceInfo(IFileContainerReferenceInfo parent, object source) : base(source)
    {
        SetParent(parent);
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    /// <param name="name">The file name.</param>
    /// <param name="lastModification">The last modification time.</param>
    /// <param name="size">The file size.</param>
    /// <param name="parent">The parent directory.</param>
    /// <param name="source">The instance source for reference.</param>
    /// <param name="exists">true if exists; otherwise, false.</param>
    public BaseFileReferenceInfo(string name, DateTime lastModification, long size, BaseDirectoryReferenceInfo parent, object source = null, bool exists = true)
        : base(name, lastModification, source, exists)
    {
        Size = size;
        SetParent(parent);
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    /// <param name="file">The file item.</param>
    /// <param name="parent">The parent folder.</param>
    public BaseFileReferenceInfo(FileInfo file, LocalDirectoryReferenceInfo parent = null) : base()
    {
        Source = file;
        if (file == null) return;
        if (!file.Exists)
        {
            try
            {
                Name = file.Name;
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
            catch (NullReferenceException)
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

            Exists = false;
            return;
        }

        Name = file.Name;
        try
        {
            LastModification = file.LastWriteTime;
            Size = file.Length;
        }
        catch (IOException)
        {
            return;
        }
        catch (NotSupportedException)
        {
            return;
        }
        catch (InvalidOperationException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }
        catch (SecurityException)
        {
            return;
        }
        catch (ExternalException)
        {
            return;
        }

        Exists = true;
        if (parent != null)
        {
            SetParent(parent);
            return;
        }

        try
        {
            var dir = file.Directory;
            if (dir == null || !dir.Exists) return;
            SetParent(new LocalDirectoryReferenceInfo(dir));
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
    }

    /// <summary>
    /// Gets the file size.
    /// </summary>
    public long Size { get; protected set; }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    public BaseDirectoryReferenceInfo GetParent()
        => GetParentInfo() as BaseDirectoryReferenceInfo;

    /// <summary>
    /// Sets the parent.
    /// </summary>
    /// <param name="info">The parent.</param>
    protected virtual void SetParent(IFileContainerReferenceInfo info)
        => parent = info;

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <returns>The parent reference information instance.</returns>
    protected virtual IFileContainerReferenceInfo GetParentInfo()
        => parent;

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <returns>The parent reference information instance.</returns>
    IFileContainerReferenceInfo IFileReferenceInfo.GetParent()
        => GetParentInfo();
}

/// <summary>
/// The reference information of file.
/// </summary>
/// <typeparam name="T">The type of instance source.</typeparam>
public class BaseDirectoryReferenceInfo<T> : BaseDirectoryReferenceInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseDirectoryReferenceInfo class.
    /// </summary>
    protected BaseDirectoryReferenceInfo() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseDirectoryReferenceInfo class.
    /// </summary>
    /// <param name="source">The instance source for reference.</param>
    protected BaseDirectoryReferenceInfo(T source) : base(source)
    {
        Source = source;
    }

    /// <summary>
    /// Initializes a new instance of the BaseDirectoryReferenceInfo class.
    /// </summary>
    /// <param name="name">The file name.</param>
    /// <param name="lastModification">The last modification time.</param>
    /// <param name="source">The instance source for reference.</param>
    /// <param name="exists">true if exists; otherwise, false.</param>
    public BaseDirectoryReferenceInfo(string name, DateTime lastModification, T source = default, bool exists = true)
        : base(name, lastModification, source, exists)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    /// <param name="directory">The directory item.</param>
    internal BaseDirectoryReferenceInfo(DirectoryInfo directory) : base(directory)
    {
        if (directory is T s) Source = s;
    }

    /// <summary>
    /// Gets the directory information instance.
    /// </summary>
    public new T Source { get; private set; }
}

/// <summary>
/// The reference information of file.
/// </summary>
/// <typeparam name="T">The type of instance source.</typeparam>
public class BaseFileReferenceInfo<T> : BaseFileReferenceInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    protected BaseFileReferenceInfo() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    /// <param name="parent">The parent directory.</param>
    /// <param name="source">The instance source for reference.</param>
    protected BaseFileReferenceInfo(IFileContainerReferenceInfo parent, T source) : base(parent, source)
    {
        Source = source;
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    /// <param name="name">The file name.</param>
    /// <param name="lastModification">The last modification time.</param>
    /// <param name="size">The file size.</param>
    /// <param name="parent">The parent directory.</param>
    /// <param name="source">The instance source for reference.</param>
    /// <param name="exists">true if exists; otherwise, false.</param>
    public BaseFileReferenceInfo(string name, DateTime lastModification, long size, BaseDirectoryReferenceInfo parent, T source = default, bool exists = true)
        : base(name, lastModification, size, parent, source, exists)
    {
        Source = source;
    }

    /// <summary>
    /// Initializes a new instance of the BaseFileReferenceInfo class.
    /// </summary>
    /// <param name="file">The file item.</param>
    /// <param name="parent">The parent folder.</param>
    internal BaseFileReferenceInfo(FileInfo file, LocalDirectoryReferenceInfo parent = null) : base(file, parent)
    {
        if (file is T s) Source = s;
    }

    /// <summary>
    /// Gets the directory information instance.
    /// </summary>
    public new T Source { get; private set; }
}

/// <summary>
/// The reference information of directory.
/// </summary>
public class LocalDirectoryReferenceInfo : BaseDirectoryReferenceInfo<DirectoryInfo>, IDirectoryHostReferenceInfo
{
    IFileContainerReferenceInfo parent;

    /// <summary>
    /// Initializes a new instance of the LocalDirectoryReferenceInfo class.
    /// </summary>
    /// <param name="directory">The directory instance.</param>
    /// <param name="parent">The parent folder.</param>
    public LocalDirectoryReferenceInfo(DirectoryInfo directory, LocalDirectoryReferenceInfo parent = null) : base(directory)
    {
        this.parent = parent;
        if (directory == null) return;
        try
        {
            Creation = directory.CreationTime;
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
    }

    /// <summary>
    /// Initializes a new instance of the LocalDirectoryReferenceInfo class.
    /// </summary>
    /// <param name="directory">The directory instance.</param>
    /// <param name="parent">The parent folder.</param>
    public LocalDirectoryReferenceInfo(DirectoryInfo directory, LocalPackageFileReferenceInfo parent) : base(directory)
    {
        this.parent = parent;
    }

    /// <summary>
    /// Gets the date created.
    /// </summary>
    public DateTime Creation { get; private set; }

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <returns>The directory collection.</returns>
    public IEnumerable<LocalDirectoryReferenceInfo> GetDirectories()
        => GetDirectories(false, null);

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="showHidden">true if show hidden; otherwise, false.</param>
    /// <param name="predicate">An optional function to test each element for a condition.</param>
    /// <returns>The directory collection.</returns>
    public IEnumerable<LocalDirectoryReferenceInfo> GetDirectories(bool showHidden, Func<DirectoryInfo, bool> predicate = null)
    {
        var dir = Source;
        if (dir == null) return new List<LocalDirectoryReferenceInfo>();
        var col = dir.EnumerateDirectories();
        if (col == null) return new List<LocalDirectoryReferenceInfo>();
        if (!showHidden) col = col.Where(ele => !ele.Attributes.HasFlag(FileAttributes.Hidden));
        if (predicate != null) col = col.Where(predicate);
        return col.Select(ele => new LocalDirectoryReferenceInfo(ele, this));
    }

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>The directory collection.</returns>
    public IEnumerable<LocalDirectoryReferenceInfo> GetDirectories(string searchPattern)
    {
        var dir = Source;
        if (dir == null) return new List<LocalDirectoryReferenceInfo>();
        var col = string.IsNullOrEmpty(searchPattern) ? dir.EnumerateDirectories() : dir.EnumerateDirectories(searchPattern);
        if (col == null) return new List<LocalDirectoryReferenceInfo>();
        return col.Select(ele => new LocalDirectoryReferenceInfo(ele, this));
    }

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    public IEnumerable<LocalFileReferenceInfo> GetFiles()
        => GetFiles(false, null);

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="showHidden">true if show hidden; otherwise, false.</param>
    /// <param name="predicate">An optional function to test each element for a condition.</param>
    /// <returns>The file collection.</returns>
    public IEnumerable<LocalFileReferenceInfo> GetFiles(bool showHidden, Func<FileInfo, bool> predicate = null)
    {
        var dir = Source;
        if (dir == null) return new List<LocalFileReferenceInfo>();
        var col = dir.EnumerateFiles();
        if (col == null) return new List<LocalFileReferenceInfo>();
        if (!showHidden) col = col.Where(ele => !ele.Attributes.HasFlag(FileAttributes.Hidden));
        if (predicate != null) col = col.Where(predicate);
        return col.Select(ele => new LocalFileReferenceInfo(ele, this));
    }

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>The directory collection.</returns>
    public IEnumerable<LocalFileReferenceInfo> GetFiles(string searchPattern)
    {
        var dir = Source;
        if (dir == null) return new List<LocalFileReferenceInfo>();
        var col = string.IsNullOrEmpty(searchPattern) ? dir.EnumerateFiles() : dir.EnumerateFiles(searchPattern);
        if (col == null) return new List<LocalFileReferenceInfo>();
        return col.Select(ele => new LocalFileReferenceInfo(ele, this));
    }

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <returns>The directory collection.</returns>
    public Task<IReadOnlyList<LocalDirectoryReferenceInfo>> GetDirectoriesAsync()
        => GetReadOnlyListAsync(GetDirectories(false, null));

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="showHidden">true if show hidden; otherwise, false.</param>
    /// <param name="predicate">An optional function to test each element for a condition.</param>
    /// <returns>The directory collection.</returns>
    public Task<IReadOnlyList<LocalDirectoryReferenceInfo>> GetDirectoriesAsync(bool showHidden, Func<DirectoryInfo, bool> predicate = null)
        => GetReadOnlyListAsync(GetDirectories(showHidden, predicate));

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>The directory collection.</returns>
    public Task<IReadOnlyList<LocalDirectoryReferenceInfo>> GetDirectoriesAsync(string searchPattern)
        => GetReadOnlyListAsync(GetDirectories(searchPattern));

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    public Task<IReadOnlyList<LocalFileReferenceInfo>> GetFilesAsync()
        => GetReadOnlyListAsync(GetFiles(false, null));

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="showHidden">true if show hidden; otherwise, false.</param>
    /// <param name="predicate">An optional function to test each element for a condition.</param>
    /// <returns>The file collection.</returns>
    public Task<IReadOnlyList<LocalFileReferenceInfo>> GetFilesAsync(bool showHidden, Func<FileInfo, bool> predicate = null)
        => GetReadOnlyListAsync(GetFiles(showHidden, predicate));

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <param name="searchPattern">The search string to match against the names of directories. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>The file collection.</returns>
    public Task<IReadOnlyList<LocalFileReferenceInfo>> GetFilesAsync(string searchPattern)
        => GetReadOnlyListAsync(GetFiles(searchPattern));

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
    /// Refreshes the state of the object.
    /// </summary>
    public void Refresh()
    {
        var source = Source;
        if (source == null) return;
        try
        {
            source.Refresh();
            Name = source.Name;
            Creation = source.CreationTime;
            LastModification = source.LastWriteTime;
            if (GetParent().Source != Source.Parent)
            {
                parent = null;
                GetParent();
            }
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
    }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    public LocalDirectoryReferenceInfo GetParent()
        => GetParentInternal() as LocalDirectoryReferenceInfo;

    /// <summary>
    /// Gets the parent.
    /// </summary>
    public Task<LocalDirectoryReferenceInfo> GetParentAsync()
        => Task.FromResult(GetParentInternal() as LocalDirectoryReferenceInfo);

    /// <summary>
    /// Gets the parent.
    /// </summary>
    Task<IFileContainerReferenceInfo> IDirectoryHostReferenceInfo.GetParentAsync()
        => Task.FromResult(GetParentInternal());

    /// <summary>
    /// Gets the parent.
    /// </summary>
    private IFileContainerReferenceInfo GetParentInternal()
    {
        if (parent != null) return parent;
        try
        {
            var dir = Source.Parent;
            if (dir == null || !dir.Exists) return null;
            parent = new LocalDirectoryReferenceInfo(dir);
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

    private static Task<IReadOnlyList<T>> GetReadOnlyListAsync<T>(IEnumerable<T> col)
        => Task.FromResult<IReadOnlyList<T>>(col.ToList());

    internal static Task<IReadOnlyList<IDirectoryReferenceInfo>> GetDirectoriesAsync(DirectoryInfo dir, LocalDirectoryReferenceInfo parent)
    {
        if (dir == null) return Task.FromResult<IReadOnlyList<IDirectoryReferenceInfo>>(new List<IDirectoryReferenceInfo>());
        if (parent == null) parent = new LocalDirectoryReferenceInfo(dir);
        var col = dir.EnumerateDirectories()?.Where(ele => !ele.Attributes.HasFlag(FileAttributes.Hidden))?.Select(ele => new LocalDirectoryReferenceInfo(ele, parent) as IDirectoryReferenceInfo)?.ToList() ?? new List<IDirectoryReferenceInfo>();
        return Task.FromResult<IReadOnlyList<IDirectoryReferenceInfo>>(col);
    }

    internal static Task<IReadOnlyList<IFileReferenceInfo>> GetFilesAsync(DirectoryInfo dir, LocalDirectoryReferenceInfo parent)
    {
        if (dir == null) return Task.FromResult<IReadOnlyList<IFileReferenceInfo>>(new List<IFileReferenceInfo>());
        if (parent == null) parent = new LocalDirectoryReferenceInfo(dir);
        var col = dir.EnumerateFiles()?.Where(ele => !ele.Attributes.HasFlag(FileAttributes.Hidden))?.Select(ele => new LocalFileReferenceInfo(ele, parent) as IFileReferenceInfo)?.ToList() ?? new List<IFileReferenceInfo>();
        return Task.FromResult<IReadOnlyList<IFileReferenceInfo>>(col);
    }
}

/// <summary>
/// The reference information of file.
/// </summary>
public class LocalFileReferenceInfo : BaseFileReferenceInfo<FileInfo>
{
    /// <summary>
    /// Initializes a new instance of the LocalFileReferenceInfo class.
    /// </summary>
    /// <param name="file">The file item.</param>
    /// <param name="parent">The parent folder.</param>
    public LocalFileReferenceInfo(FileInfo file, LocalDirectoryReferenceInfo parent = null) : base(file, parent)
    {
        if (file == null) return;
        try
        {
            Extension = file.Extension;
            Creation = file.CreationTime;
            Attributes = file.Attributes;
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
    }

    /// <summary>
    /// Gets the extension name.
    /// </summary>
    public string Extension { get; private set; }

    /// <summary>
    /// Gets the date created.
    /// </summary>
    public DateTime Creation { get; private set; }

    /// <summary>
    /// Gets the file attributes.
    /// </summary>
    public FileAttributes Attributes { get; private set; }

    /// <summary>
    /// Refreshes the state of the object.
    /// </summary>
    public void Refresh()
    {
        var source = Source;
        if (source == null) return;
        try
        {
            source.Refresh();
            Name = source.Name;
            Extension = source.Extension;
            Creation = source.CreationTime;
            LastModification = source.LastWriteTime;
            Size = source.Length;
            if (GetParent().Source != Source.Directory)
            {
                SetParent(null);
                GetParentInfo();
            }
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
    }

    /// <summary>
    /// Opens a file in the specified mode and other options.
    /// </summary>
    /// <param name="mode">A constant specifying the mode (for example, Open or Append) in which to open the file.</param>
    /// <param name="access">A constant specifying whether to open the file with Read, Write, or ReadWrite file access.</param>
    /// <param name="share">A constant specifying the type of access other file stream objects have to this file.</param>
    /// <returns>A file opened in the specified mode, access options and shared options.</returns>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="FileNotFoundException">The file is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">The file is read-only.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
    /// <exception cref="IOException">The file is already open.</exception>
    public FileStream Open(FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
    {
        var file = Source;
        if (file == null) throw new FileNotFoundException("The file does not specify.");
        return file.Open(mode, access, share);
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Opens a file in the specified mode and other options.
    /// </summary>
    /// <param name="options">An object that describes optional file stream parameters to use.</param>
    /// <returns>A file opened in the specified mode, access options and shared options.</returns>
    /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="FileNotFoundException">The file is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">The file is read-only.</exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
    /// <exception cref="IOException">The file is already open.</exception>
    public FileStream Open(FileStreamOptions options)
    {
        var file = Source;
        if (file == null) throw new FileNotFoundException("The file does not specify.");
        return options == null ? file.Open(FileMode.OpenOrCreate) : file.Open(options);
    }
#endif

    /// <summary>
    /// Gets the parent.
    /// </summary>
    public new LocalDirectoryReferenceInfo GetParent()
        => base.GetParent() as LocalDirectoryReferenceInfo;

    /// <summary>
    /// Gets the parent.
    /// </summary>
    protected override IFileContainerReferenceInfo GetParentInfo()
    {
        var info = base.GetParentInfo();
        if (info != null) return info;
        try
        {
            var dir = Source.Directory;
            if (dir == null || !dir.Exists) return null;
            SetParent(new LocalDirectoryReferenceInfo(dir));
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

        return base.GetParentInfo();
    }
}

/// <summary>
/// The reference information of package (such as compressed) file.
/// </summary>
public class LocalPackageFileReferenceInfo : LocalFileReferenceInfo, IFileContainerReferenceInfo
{
    /// <summary>
    /// Initializes a new instance of the LocalPackageFileReferenceInfo class.
    /// </summary>
    /// <param name="file">The file item.</param>
    /// <param name="parent">The parent folder.</param>
    public LocalPackageFileReferenceInfo(FileInfo file, LocalDirectoryReferenceInfo parent = null) : base(file, parent)
    {
    }
}
