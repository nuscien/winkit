﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.IO;
using Trivial.Net;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;
using Windows.Storage;

namespace Trivial.Net;

/// <summary>
/// The source types of web API result.
/// </summary>
public enum WebApiResultSourceTypes : byte
{
    /// <summary>
    /// Unknown source type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The result is from web API.
    /// </summary>
    Online = 1,

    /// <summary>
    /// The result is from local cache.
    /// </summary>
    Cache = 2,

    /// <summary>
    /// The result is from web API but only contains the first part.
    /// </summary>
    Partial = 3,

    /// <summary>
    /// The result is merged by multiple source.
    /// </summary>
    Merged = 4,

    /// <summary>
    /// The mock data.
    /// </summary>
    Mock = 5,

    /// <summary>
    /// The source type is not valid.
    /// </summary>
    Invalid = 7,
}

/// <summary>
/// The result information for web API client.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public class WebApiClientResultInfo<T>
{
    private T cache;
    private T online;
    private Task<T> promise;

    /// <summary>
    /// Gets the cache data.
    /// </summary>
    /// <exception cref="InvalidOperationException">No cache.</exception>
    public T Cache => HasCache || IgnoreExceptionWhenNull ? cache : throw new InvalidOperationException("There is no cache.");

    /// <summary>
    /// Gets a value indicating whether it already has cache result.
    /// </summary>
    public bool HasCache { get; private set; }

    /// <summary>
    /// Gets the result data.
    /// </summary>
    /// <exception cref="InvalidOperationException">No result.</exception>
    public T GetResult()
        => WebAccessingState == TaskStates.Done ? online : (HasCache || IgnoreExceptionWhenNull ? cache : throw new InvalidOperationException("There is no result."));

    /// <summary>
    /// Tries to get result.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>true if get succeeded; otherwise, false.</returns>
    public bool TryGetResult(out T result)
    {
        if (WebAccessingState == TaskStates.Done)
        {
            result = online;
            return true;
        }

        if (HasCache)
        {
            result = cache;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets the web result data.
    /// </summary>
    /// <exception cref="InvalidOperationException">No web result.</exception>
    public T GetWebResult()
        => WebAccessingState == TaskStates.Done || IgnoreExceptionWhenNull ? online : throw new InvalidOperationException("There is no web result.");

    /// <summary>
    /// Tries to get web result.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>true if get succeeded; otherwise, false.</returns>
    public bool TryGetWebResult(out T result)
    {
        if (WebAccessingState == TaskStates.Done)
        {
            result = online;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets state of web accessing.
    /// </summary>
    public TaskStates WebAccessingState { get; private set; } = TaskStates.Pending;

    /// <summary>
    /// Gets a value indicating whether it is loading web result.
    /// </summary>
    public bool IsWebResultEnabled { get; private set; }

    /// <summary>
    /// Sets to do not throw exception when result does not set.
    /// </summary>
    public bool IgnoreExceptionWhenNull { get; set; }

    /// <summary>
    /// Sets cache result.
    /// </summary>
    /// <param name="data">The cache result data.</param>
    internal void SetCacheResult(T data)
    {
        cache = data;
        HasCache = true;
    }

    /// <summary>
    /// Sets web result.
    /// </summary>
    /// <param name="data">The web result data.</param>
    internal void SetWebResult(T data)
    {
        WebAccessingState = TaskStates.Working;
        IsWebResultEnabled = true;
        online = data;
        WebAccessingState = TaskStates.Done;
    }

    /// <summary>
    /// Sets web result.
    /// </summary>
    /// <param name="data">The web result data.</param>
    /// <param name="validate">A handler to validate result.</param>
    internal async Task<bool> SetWebResult(Task<T> data, Func<T, bool> validate = null)
    {
        if (data == null)
        {
            WebAccessingState = TaskStates.Pending;
            return false;
        }

        IsWebResultEnabled = true;
        try
        {
            promise = data;
            WebAccessingState = TaskStates.Working;
            var r = await data;
            if (validate?.Invoke(r) == false)
            {
                WebAccessingState = TaskStates.Faulted;
                promise = null;
                return false;
            }

            online = r;
            WebAccessingState = TaskStates.Done;
            promise = null;
            return true;
        }
        catch (OperationCanceledException)
        {
            WebAccessingState = TaskStates.Canceled;
            promise = null;
            return false;
        }
        catch (Exception)
        {
            WebAccessingState = TaskStates.Faulted;
            promise = null;
            throw;
        }
    }

    /// <summary>
    /// Gets web result.
    /// </summary>
    /// <returns>The result.</returns>
    /// <exception cref="InvalidOperationException">Data load failed.</exception>
    public async Task<T> GetWebResultAsync()
    {
        if (WebAccessingState == TaskStates.Done)
            return online;
        var t = promise;
        if (WebAccessingState == TaskStates.Working && t != null)
            return await t;
        if (WebAccessingState == TaskStates.Done)
            return online;
        throw new InvalidOperationException("Load data failed.");
    }

    /// <summary>
    /// Gets result.
    /// </summary>
    /// <returns>The result.</returns>
    /// <exception cref="InvalidOperationException">Data load failed.</exception>
    public async Task<T> GetResultAsync()
    {
        if (WebAccessingState == TaskStates.Done)
            return online;
        var t = promise;
        if (WebAccessingState == TaskStates.Working && t != null)
            return await t;
        if (WebAccessingState == TaskStates.Done)
            return online;
        if (HasCache) return cache;
        throw new InvalidOperationException("Load data failed.");
    }
}

/// <summary>
/// The JSON web API client with cache supports.
/// </summary>
public class JsonWebCacheClient
{
    private Func<JsonHttpClient<JsonObjectNode>> handler;

    /// <summary>
    /// Initializes a new instance of the JsonWebCacheClient class.
    /// </summary>
    public JsonWebCacheClient()
    {
    }

    /// <summary>
    /// Initializes a new instance of the JsonWebCacheClient class.
    /// </summary>
    /// <param name="folder">The optional storage folder.</param>
    public JsonWebCacheClient(StorageFolder folder)
    {
        Folder = folder;
    }

    /// <summary>
    /// Initializes a new instance of the JsonWebCacheClient class.
    /// </summary>
    /// <param name="client">The web API client.</param>
    public JsonWebCacheClient(OAuthBasedClient client)
    {
        if (client == null) return;
        handler = () => FillHttpClient(client.Create<JsonObjectNode>());
    }

    /// <summary>
    /// Initializes a new instance of the JsonWebCacheClient class.
    /// </summary>
    /// <param name="client">The web API client.</param>
    /// <param name="folder">The optional storage folder.</param>
    public JsonWebCacheClient(OAuthBasedClient client, StorageFolder folder)
         : this(client)
    {
        Folder = folder;
    }

    /// <summary>
    /// Initializes a new instance of the JsonWebApiClient class.
    /// </summary>
    /// <param name="client">The web API client.</param>
    public JsonWebCacheClient(OAuthClient client)
    {
        if (client == null) return;
        handler = () => FillHttpClient(client.Create<JsonObjectNode>());
    }

    /// <summary>
    /// Initializes a new instance of the JsonWebApiClient class.
    /// </summary>
    /// <param name="client">The web API client.</param>
    /// <param name="folder">The optional storage folder.</param>
    public JsonWebCacheClient(OAuthClient client, StorageFolder folder)
         : this(client)
    {
        Folder = folder;
    }

    /// <summary>
    /// Adds or removes a handler raised on sending.
    /// </summary>
    public event EventHandler<SendingEventArgs> Sending;

    /// <summary>
    /// Gets the data cache.
    /// </summary>
    public DataCacheCollection<JsonObjectNode> Cache { get; } = new();

    /// <summary>
    /// Gets or sets the storage folder to cache data.
    /// </summary>
    public StorageFolder Folder { get; set; }

    /// <summary>
    /// Gets the optional expiration.
    /// </summary>
    public TimeSpan? Expiration
    {
        get => Cache.Expiration;
        set => Cache.Expiration = value;
    }

    /// <summary>
    /// Gets or sets the maxinum count of the elements contained in the cache item collection; or null, if no limitation.
    /// </summary>
    public int? MaxCount
    {
        get => Cache.MaxCount;
        set => Cache.MaxCount = value;
    }

    /// <summary>
    /// Gets or sets the additional tag.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Sets web API client.
    /// </summary>
    /// <param name="client">The web API client.</param>
    public void SetWebApiClient(JsonHttpClient<JsonObjectNode> client)
    {
        if (client == null)
        {
            handler = null;
            return;
        }

        FillHttpClient(client);
        handler = () => client;
    }

    /// <summary>
    /// Sets web API client.
    /// </summary>
    /// <param name="client">The web API client.</param>
    public void SetWebApiClient(Func<JsonHttpClient<JsonObjectNode>> client)
        => handler = client == null ? null : () => FillHttpClient(client());

    /// <summary>
    /// Sets web API client.
    /// </summary>
    /// <param name="client">The web API client.</param>
    public void SetWebApiClient(OAuthBasedClient client)
        => handler = client == null ? null : () => FillHttpClient(client.Create<JsonObjectNode>());

    /// <summary>
    /// Sets web API client.
    /// </summary>
    /// <param name="client">The web API client.</param>
    public void SetWebApiClient(OAuthClient client)
        => handler = client == null ? null : () => FillHttpClient(client.Create<JsonObjectNode>());

    /// <summary>
    /// Gets result with information.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="cancellationToken">An optional cancellation token for the operation.</param>
    /// <returns>The information.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    public WebApiClientResultInfo<JsonObjectNode> Get(WebApiRequestOptions<JsonObjectNode> options, Action<JsonObjectNode, WebApiResultSourceTypes> callback, CancellationToken cancellationToken = default)
    {
        if (options == null) throw new ArgumentNullException(nameof(options), "options should not be null.");
        var info = new WebApiClientResultInfo<JsonObjectNode>();
        var cacheKey = options.CacheKey;
        var uri = options.Uri;
        if (!string.IsNullOrWhiteSpace(cacheKey))
        {
            var folder = Folder;
            if (Cache.TryGetInfo(cacheKey, out var data) && !data.IsExpired(options.CacheTimeout))
            {
                var result = data.Value;
                info.SetCacheResult(result);
                callback?.Invoke(result, WebApiResultSourceTypes.Cache);
                if (uri == null) return info;
                if (!data.IsExpired(options.LimitationDuration)) return info;
            }
            else
            {
                _ = LoadCache(cacheKey, callback, cancellationToken);
            }
        }

        _ = FillWebResult(info, options, uri, callback, cancellationToken);
        return info;
    }

    /// <summary>
    /// Gets result with information.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">An optional cancellation token for the operation.</param>
    /// <returns>The information.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    public WebApiClientResultInfo<JsonObjectNode> Get(WebApiRequestOptions<JsonObjectNode> options, CancellationToken cancellationToken = default)
        => Get(options, null, cancellationToken);

    /// <summary>
    /// Gets result with information.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="cancellationToken">An optional cancellation token for the operation.</param>
    /// <returns>The information.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    public Task<JsonObjectNode> GetAsync(WebApiRequestOptions<JsonObjectNode> options, Action<JsonObjectNode, WebApiResultSourceTypes> callback, CancellationToken cancellationToken = default)
        => Get(options, callback, cancellationToken).GetResultAsync();

    /// <summary>
    /// Gets result with information.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">An optional cancellation token for the operation.</param>
    /// <returns>The information.</returns>
    /// <exception cref="ArgumentNullException">options was null.</exception>
    public Task<JsonObjectNode> GetAsync(WebApiRequestOptions<JsonObjectNode> options, CancellationToken cancellationToken = default)
        => Get(options, null, cancellationToken).GetResultAsync();

    /// <summary>
    /// Gets data online.
    /// </summary>
    /// <param name="uri">The request URI.</param>
    /// <param name="cancellationToken">An optional cancellation token for the operation.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> GetAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        var h = handler;
        var client = h?.Invoke() ?? FillHttpClient(null);
        return client.GetAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Gets data online.
    /// </summary>
    /// <param name="uri">The request URI.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="cancellationToken">An optional cancellation token for the operation.</param>
    /// <returns>The result.</returns>
    public async Task<JsonObjectNode> GetAsync(Uri uri, Action<JsonObjectNode, WebApiResultSourceTypes> callback, CancellationToken cancellationToken = default)
    {
        var h = handler;
        var client = h?.Invoke() ?? FillHttpClient(null);
        var result = await client.GetAsync(uri, cancellationToken);
        callback?.Invoke(result, WebApiResultSourceTypes.Online);
        return result;
    }

    private JsonHttpClient<JsonObjectNode> FillHttpClient(JsonHttpClient<JsonObjectNode> client)
    {
        if (client == null) client = new JsonHttpClient<JsonObjectNode>();
        client.Sending += OnSending;
        return client;
    }

    private async Task FillWebResult(WebApiClientResultInfo<JsonObjectNode> info, WebApiRequestOptions<JsonObjectNode> options, Uri uri, Action<JsonObjectNode, WebApiResultSourceTypes> callback, CancellationToken cancellationToken)
    {
        var task = GetAsync(uri, cancellationToken);
        if (!await info.SetWebResult(task, options.IsValid)) return;
        JsonObjectNode result;
        try
        {
            result = info.GetWebResult();
            Cache[options.CacheKey] = result;
        }
        catch (InvalidOperationException)
        {
            return;
        }

        callback?.Invoke(result, WebApiResultSourceTypes.Online);
        var folder = Folder;
        if (folder != null)
        {
            var s = string.Concat("//", Web.WebFormat.ParseDate(DateTime.Now).ToString("g"), "\r\n", result?.ToString() ?? JsonValues.Null.ToString());
            try
            {
                var file = await folder.CreateFileAsync(GetFileName(options.CacheKey), CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, s);
            }
            catch (InvalidOperationException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (AggregateException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (NotImplementedException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (IOException)
            {
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
            }
        }
    }

    private async Task<JsonObjectNode> LoadCache(string cacheKey, Action<JsonObjectNode, WebApiResultSourceTypes> callback, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) return null;
        var fileName = GetFileName(cacheKey);
        var folder = Folder;
        if (folder == null) return null;
        var file = await folder.GetFileAsync(fileName);
        if (file == null) return null;
        string s = null;
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            s = await FileIO.ReadTextAsync(file);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (AggregateException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
        }

        if (string.IsNullOrWhiteSpace(s)) return null;
        cancellationToken.ThrowIfCancellationRequested();
        DateTime date = DateTime.Now;
        if (s.StartsWith("//"))
        {
            var end = s.IndexOfAny(new[] { '\n', '\r', '\t', ' ' });
            if (end > 0) date = Web.WebFormat.ParseDate(s.Substring(2, end).Trim()) ?? DateTime.Now;
            s = s.Substring(s.IndexOf('\n') + 1);
        }

        var json = JsonObjectNode.TryParse(s);
        if (json == null) return null;
        var expiration = Cache.Expiration;
        if (expiration.HasValue && DateTime.Now - date > expiration.Value) return null;
        if (Cache.Contains(cacheKey)) return json;
        Cache.Add(new DataCacheItemInfo<JsonObjectNode>(cacheKey, json, date));
        callback?.Invoke(json, WebApiResultSourceTypes.Cache);
        return json;
    }

    private void OnSending(object sender, SendingEventArgs e)
        => Sending?.Invoke(this, e);

    private static string GetFileName(string cacheKey)
        => string.Concat(cacheKey, ".cache.json");
}

/// <summary>
/// The options for web API request.
/// </summary>
/// <typeparam name="T">The type of result.</typeparam>
public class WebApiRequestOptions<T>
{
    private readonly Func<T, bool> validate;

    /// <summary>
    /// Initializes a new instance of the WebApiRequestOptions class.
    /// </summary>
    public WebApiRequestOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the WebApiRequestOptions class.
    /// </summary>
    /// <param name="uri">The request URI.</param>
    public WebApiRequestOptions(Uri uri)
        : this()
    {
        Uri = uri;
        LimitationDuration = TimeSpan.Zero;
    }

    /// <summary>
    /// Initializes a new instance of the WebApiRequestOptions class.
    /// </summary>
    /// <param name="uri">The request URI.</param>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="duration">The limited duration to access web resource after latest.</param>
    /// <param name="timeout">Cache timeout.</param>
    public WebApiRequestOptions(Uri uri, string cacheKey, TimeSpan? duration = null, TimeSpan? timeout = null)
    {
        Uri = uri;
        CacheKey = cacheKey;
        LimitationDuration = duration ?? TimeSpan.Zero;
        CacheTimeout = timeout;
    }

    /// <summary>
    /// Initializes a new instance of the WebApiRequestOptions class.
    /// </summary>
    /// <param name="uri">The request URI.</param>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="validate">The handler to validate result from online.</param>
    /// <param name="duration">The limited duration to access web resource after latest.</param>
    /// <param name="timeout">The cache timeout.</param>
    public WebApiRequestOptions(Uri uri, string cacheKey, Func<T, bool> validate, TimeSpan? duration = null, TimeSpan? timeout = null)
        : this(uri, cacheKey, duration, timeout)
    {
        this.validate = validate;
    }

    /// <summary>
    /// Gets the cache key.
    /// </summary>
    public virtual string CacheKey { get; protected set; }

    /// <summary>
    /// Gets the request URI.
    /// </summary>
    public virtual Uri Uri { get; protected set; }

    /// <summary>
    /// Gets the limited duration to access web resource after latest.
    /// </summary>
    public virtual TimeSpan LimitationDuration { get; protected set; }

    /// <summary>
    /// Gets the cache timeout
    /// </summary>
    public virtual TimeSpan? CacheTimeout { get; protected set; }

    /// <summary>
    /// Tests if the result is valid.
    /// </summary>
    /// <param name="data">The result data.</param>
    /// <returns>true if the result data is valid; otherwise, false.</returns>
    public virtual bool IsValid(T data) => validate == null || validate(data);
}
