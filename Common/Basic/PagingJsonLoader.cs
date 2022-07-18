using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Text;

namespace Trivial.Data;

/// <summary>
/// Paging data event arguments.
/// </summary>
public class JsonPagingEventArgs : DataEventArgs<JsonObjectNode>
{
    /// <summary>
    /// Initializes a new instance of the PagingDataEventArgs class.
    /// </summary>
    /// <param name="value">The data value.</param>
    /// <param name="page">The page index.</param>
    /// <param name="kind">The source kind.</param>
    public JsonPagingEventArgs(JsonObjectNode value, int page, WebApiResultSourceTypes kind) : base(value)
    {
        Page = page;
        Kind = kind;
    }

    /// <summary>
    /// Gets the page index.
    /// </summary>
    public int Page { get; private set; }

    /// <summary>
    /// Gets the source kind.
    /// </summary>
    public WebApiResultSourceTypes Kind { get; private set; }
}

/// <summary>
/// The film library page.
/// </summary>
public abstract class BaseJsonPagingLoader
{
    private readonly SemaphoreSlim slim = new(1, 1);
    private readonly Dictionary<int, JsonObjectNode> cache = new();
    private readonly bool disableAutoRaise = false;

    /// <summary>
    /// Initializes a new instance of the BasePagingJsonLoader class.
    /// </summary>
    public BaseJsonPagingLoader()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BasePagingJsonLoader class.
    /// </summary>
    /// <param name="disableAutoRaise">true if disable load event auto raise; otherwise, false.</param>
    public BaseJsonPagingLoader(bool disableAutoRaise)
    {
        this.disableAutoRaise = disableAutoRaise;
    }

    /// <summary>
    /// Deconstructs.
    /// </summary>
    ~BaseJsonPagingLoader()
    {
        try
        {
            slim.Dispose();
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
    }

    /// <summary>
    /// Occurs on data is loaded.
    /// </summary>
    public event EventHandler<JsonPagingEventArgs> DataLoaded;

    /// <summary>
    /// Occurs on data is loaded failed because of network error.
    /// </summary>
    public event EventHandler<DataEventArgs<FailedHttpException>> NetworkAccessFailed;

    /// <summary>
    /// Gets the page index.
    /// </summary>
    public int PageIndex { get; private set; } = -1;

    /// <summary>
    /// Gets data.
    /// </summary>
    /// <param name="page">The page index.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The result.</returns>
    protected abstract Task<JsonObjectNode> GetPageDataAsync(int page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Raises the event.
    /// </summary>
    /// <param name="value">The data result.</param>
    /// <param name="page">The page index.</param>
    /// <param name="kind">The source kind.</param>
    protected void ReceiveResult(JsonObjectNode value, int page, WebApiResultSourceTypes kind = WebApiResultSourceTypes.Online)
        => ReceiveResult(value, page, false, kind);

    /// <summary>
    /// Raises the event.
    /// </summary>
    /// <param name="value">The data result.</param>
    /// <param name="page">The page index.</param>
    /// <param name="skipToUpdateCache">true if won't update cache; otherwise, false.</param>
    /// <param name="kind">The source kind.</param>
    protected void ReceiveResult(JsonObjectNode value, int page, bool skipToUpdateCache, WebApiResultSourceTypes kind = WebApiResultSourceTypes.Online)
    {
        if (!skipToUpdateCache) cache[page] = value;
        DataLoaded?.Invoke(this, new(value, page, kind));
    }

    /// <summary>
    /// Loads data of next page.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> LoadNextPageAsync(CancellationToken cancellationToken = default)
        => LoadPageAsync(PageIndex + 1, cancellationToken);

    /// <summary>
    /// Resets page index.
    /// </summary>
    public void ResetPageIndex()
        => PageIndex = -1;

    /// <summary>
    /// Clears cache.
    /// </summary>
    public void ClearCache()
    {
        cache.Clear();
        PageIndex = -1;
    }

    /// <summary>
    /// Loads data of the specific page.
    /// </summary>
    /// <param name="page">The page index.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result; null if skip, or the result is null.</returns>
    public Task<JsonObjectNode> LoadPageAsync(int page, CancellationToken cancellationToken = default)
        => LoadPageAsync(page, false, cancellationToken);

    /// <summary>
    /// Loads data of the specific page.
    /// </summary>
    /// <param name="page">The page index.</param>
    /// <param name="wait">true if wait to continue; otherwise, false, to skip on working.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result; null if skip, or the result is null.</returns>
    public async Task<JsonObjectNode> LoadPageAsync(int page, bool wait, CancellationToken cancellationToken = default)
    {
        if (page < 0) return null;
        if (!wait && slim.CurrentCount < 0) return null;
        try
        {
            try
            {
                await slim.WaitAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (NullReferenceException)
            {
                return null;
            }

            if (cache.TryGetValue(page, out var result) && result != null) return result;
            try
            {
                result = await GetPageDataAsync(page, cancellationToken);
            }
            catch (FailedHttpException ex)
            {
                NetworkAccessFailed?.Invoke(this, new(ex));
            }
            catch (Exception ex)
            {
                if (ex?.InnerException != null && ex.InnerException is FailedHttpException httpEx)
                    NetworkAccessFailed?.Invoke(this, new(httpEx));
                else
                    throw;
            }

            if (result == null) return null;
            if (!disableAutoRaise)
            {
                cache[page] = result;
                DataLoaded?.Invoke(this, new(result, page, WebApiResultSourceTypes.Online));
            }

            PageIndex = page;
            return result;
        }
        finally
        {
            try
            {
                slim.Release();
            }
            catch (InvalidOperationException)
            {
            }
            catch (NullReferenceException)
            {
            }
        }
    }
}
