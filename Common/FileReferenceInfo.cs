﻿using System;
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
    IEnumerable<IFileReferenceInfo> GetFiles(IFileContainerReferenceInfo directory);

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The directory collection.</returns>
    IEnumerable<IDirectoryReferenceInfo> GetDirectories(IFileContainerReferenceInfo directory);

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="directory">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    IFileContainerReferenceInfo GetParent(IFileContainerReferenceInfo directory);
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
    /// Lists all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    IEnumerable<IFileReferenceInfo> GetFiles();

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <returns>The directory collection.</returns>
    IEnumerable<IDirectoryReferenceInfo> GetDirectories();

    /// <summary>
    /// Gets the parent.
    /// </summary>
    IFileContainerReferenceInfo GetParent();
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
    public abstract IEnumerable<IFileReferenceInfo> GetFiles(T directory);

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The directory collection.</returns>
    public abstract IEnumerable<T> GetDirectories(T directory);

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="file">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    public abstract T GetParent(IFileReferenceInfo file);

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="directory">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    public abstract T GetParent(T directory);

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
    IEnumerable<IFileReferenceInfo> IFileReferenceClient.GetFiles(IFileContainerReferenceInfo directory)
    {
        if (directory is not T info) return new List<IFileReferenceInfo>();
        return GetFiles(info) ?? new List<IFileReferenceInfo>();
    }

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The directory collection.</returns>
    IEnumerable<IDirectoryReferenceInfo> IFileReferenceClient.GetDirectories(IFileContainerReferenceInfo directory)
    {
        if (directory is not T info) yield break;
        var list = GetDirectories(info);
        if (list == null) yield break;
        foreach (var item in list)
        {
            yield return item;
        }
    }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="directory">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    IFileContainerReferenceInfo IFileReferenceClient.GetParent(IFileContainerReferenceInfo directory)
    {
        if (directory is not T info) return null;
        return GetParent(info);
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
    public IEnumerable<IFileReferenceInfo> GetFiles(IFileContainerReferenceInfo directory)
    {
        if (directory == null) return new List<IFileReferenceInfo>();
        var type = directory.GetType();
        if (handlers.TryGetValue(type, out var h) && h.Test(directory))
            return h.GetFiles(directory) ?? new List<IFileReferenceInfo>();
        if (directory is IDirectoryHostReferenceInfo dir) return dir.GetFiles() ?? new List<IFileReferenceInfo>();
        if (directory.Source is not DirectoryInfo info) return new List<IFileReferenceInfo>();
        return info.EnumerateFiles().Select(ele => new LocalFileReferenceInfo(ele, new LocalDirectoryReferenceInfo(info)));
    }

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <param name="directory">The directory to load sub-directories or files.</param>
    /// <returns>The directory collection.</returns>
    public IEnumerable<IDirectoryReferenceInfo> GetDirectories(IFileContainerReferenceInfo directory)
    {
        if (directory == null) return new List<IDirectoryReferenceInfo>();
        var type = directory.GetType();
        if (handlers.TryGetValue(type, out var h) && h.Test(directory))
            return h.GetDirectories(directory) ?? new List<IDirectoryReferenceInfo>();
        if (directory is IDirectoryHostReferenceInfo dir) return dir.GetDirectories() ?? new List<IDirectoryReferenceInfo>();
        if (directory.Source is not DirectoryInfo info) return new List<IDirectoryReferenceInfo>();
        return info.EnumerateDirectories().Select(ele => new LocalDirectoryReferenceInfo(ele, new LocalDirectoryReferenceInfo(info)));
    }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <param name="directory">The directory to get parent.</param>
    /// <return>The parent information; or null, if no parent.</return>
    public IFileContainerReferenceInfo GetParent(IFileContainerReferenceInfo directory)
    {
        if (directory == null) return null;
        var type = directory.GetType();
        if (handlers.TryGetValue(type, out var h) && h.Test(directory))
            return h.GetParent(directory);
        if (directory is IDirectoryHostReferenceInfo dir) return dir.GetParent();
        if (directory.Source is not DirectoryInfo info) return null;
        return new LocalDirectoryReferenceInfo(info);
    }
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
/// The reference information of file.
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
        if (directory == null)
        {
            Exists = false;
            return;
        }

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

            Exists = false;
            return;
        }

        Name = directory.Name;
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
        if (file == null)
        {
            Exists = false;
            return;
        }

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
            Exists = false;
            return;
        }
        catch (NotSupportedException)
        {
            Exists = false;
            return;
        }
        catch (InvalidOperationException)
        {
            Exists = false;
            return;
        }
        catch (UnauthorizedAccessException)
        {
            Exists = false;
            return;
        }
        catch (SecurityException)
        {
            Exists = false;
            return;
        }
        catch (ExternalException)
        {
            Exists = false;
            return;
        }

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
/// The reference information of file.
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
    /// Lists all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    public IEnumerable<LocalFileReferenceInfo> GetFiles()
        => Source?.EnumerateFiles()?.Select(ele => new LocalFileReferenceInfo(ele, this)) ?? new List<LocalFileReferenceInfo>();

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <returns>The directory collection.</returns>
    public IEnumerable<LocalDirectoryReferenceInfo> GetDirectories()
        => Source?.EnumerateDirectories()?.Select(ele => new LocalDirectoryReferenceInfo(ele, this)) ?? new List<LocalDirectoryReferenceInfo>();

    /// <summary>
    /// Lists all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    IEnumerable<IFileReferenceInfo> IDirectoryHostReferenceInfo.GetFiles()
        => GetFiles();

    /// <summary>
    /// Lists all sub-directories.
    /// </summary>
    /// <returns>The directory collection.</returns>
    IEnumerable<IDirectoryReferenceInfo> IDirectoryHostReferenceInfo.GetDirectories()
        => GetDirectories();

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
    IFileContainerReferenceInfo IDirectoryHostReferenceInfo.GetParent()
        => GetParentInternal();

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
}

/// <summary>
/// The reference information of file.
/// </summary>
public class LocalFileReferenceInfo: BaseFileReferenceInfo<FileInfo>
{
    /// <summary>
    /// Initializes a new instance of the LocalFileReferenceInfo class.
    /// </summary>
    /// <param name="file">The file item.</param>
    /// <param name="parent">The parent folder.</param>
    public LocalFileReferenceInfo(FileInfo file, LocalDirectoryReferenceInfo parent = null) : base(file, parent)
    {
    }

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
