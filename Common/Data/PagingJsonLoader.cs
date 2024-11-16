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
    /// <param name="page">The zero-based page index.</param>
    /// <param name="kind">The source kind.</param>
    /// <param name="pendingTime">The job pending date time.</param>
    /// <param name="requestTime">The data request date time.</param>
    /// <param name="responseTime">The data response date time.</param>
    /// <param name="contextObject">The optional context object.</param>
    public JsonPagingEventArgs(JsonObjectNode value, int page, WebApiResultSourceTypes kind, DateTime pendingTime, DateTime? requestTime = null, DateTime? responseTime = null, object contextObject = null) : this(value, page, kind, pendingTime, contextObject)
    {
        RequestTime = requestTime ?? pendingTime;
        ResponseTime = responseTime ?? DateTime.Now;
    }

    /// <summary>
    /// Initializes a new instance of the PagingDataEventArgs class.
    /// </summary>
    /// <param name="value">The data value.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="kind">The source kind.</param>
    /// <param name="pendingTime">The job pending date time.</param>
    /// <param name="contextObject">The optional context object.</param>
    public JsonPagingEventArgs(JsonObjectNode value, int page, WebApiResultSourceTypes kind, DateTime? pendingTime, object contextObject) : base(value)
    {
        Page = page;
        Kind = kind;
        PendingTime = pendingTime ?? DateTime.Now;
        ContextObject = contextObject;
    }

    /// <summary>
    /// Gets the zero-based page index.
    /// </summary>
    public int Page { get; private set; }

    /// <summary>
    /// Gets the source kind.
    /// </summary>
    public WebApiResultSourceTypes Kind { get; private set; }

    /// <summary>
    /// Gets the job pending date time.
    /// </summary>
    public DateTime PendingTime { get; private set; }

    /// <summary>
    /// Gets the data request date time.
    /// </summary>
    public DateTime RequestTime { get; private set; }

    /// <summary>
    /// Gets the data response date time.
    /// </summary>
    public DateTime ResponseTime { get; private set; }

    /// <summary>
    /// Gets The optional context object.
    /// </summary>
    public object ContextObject { get; private set; }

    /// <summary>
    /// Gets the JSON switch context for first page.
    /// </summary>
    /// <returns>The JSON switch context.</returns>
    public JsonSwitchContext<JsonObjectNode, JsonPagingEventArgs> Switch()
        => Data.Switch(this);

    /// <summary>
    /// Gets the JSON switch context for first page.
    /// </summary>
    /// <returns>The JSON switch context.</returns>
    public JsonSwitchContext<JsonObjectNode, JsonPagingEventArgs> FirstPageSwitch()
        => Switch(Page == 0);

    /// <summary>
    /// Gets the JSON switch context for first page.
    /// </summary>
    /// <param name="sourceType">Source type filter.</param>
    /// <returns>The JSON switch context.</returns>
    public JsonSwitchContext<JsonObjectNode, JsonPagingEventArgs> FirstPageSwitch(WebApiResultSourceTypes sourceType)
        => Switch(Page == 0 && Kind == sourceType);

    /// <summary>
    /// Gets the JSON switch context for first page.
    /// </summary>
    /// <param name="sourceTypes">Source type filter.</param>
    /// <returns>The JSON switch context.</returns>
    public JsonSwitchContext<JsonObjectNode, JsonPagingEventArgs> FirstPageSwitch(IEnumerable<WebApiResultSourceTypes> sourceTypes)
        => Switch(Page == 0 && (sourceTypes is null || sourceTypes.Contains(Kind)));

    /// <summary>
    /// Gets the JSON switch context for first page.
    /// </summary>
    /// <returns>The JSON switch context.</returns>
    public JsonSwitchContext<JsonObjectNode, JsonPagingEventArgs> RestPagesSwitch()
        => Switch(Page > 0);

    /// <summary>
    /// Gets the JSON switch context for first page.
    /// </summary>
    /// <param name="sourceType">Source type filter.</param>
    /// <returns>The JSON switch context.</returns>
    public JsonSwitchContext<JsonObjectNode, JsonPagingEventArgs> RestPagesSwitch(WebApiResultSourceTypes sourceType)
        => Switch(Page > 0 && Kind == sourceType);

    /// <summary>
    /// Gets the JSON switch context for first page.
    /// </summary>
    /// <param name="sourceTypes">Source type filter.</param>
    /// <returns>The JSON switch context.</returns>
    public JsonSwitchContext<JsonObjectNode, JsonPagingEventArgs> RestPagesSwitch(IEnumerable<WebApiResultSourceTypes> sourceTypes)
        => Switch(Page > 0 && (sourceTypes is null || sourceTypes.Contains(Kind)));

    /// <summary>
    /// Gets the JSON switch context for first page.
    /// </summary>
    /// <param name="validate">true if returns the context in service; oterwise, false.</param>
    /// <returns>The JSON switch context.</returns>
    private JsonSwitchContext<JsonObjectNode, JsonPagingEventArgs> Switch(bool validate)
        => validate ? Data.Switch(this) : Data.Switch(this).Default();
}

/// <summary>
/// The paging loader.
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
    /// Occurs on data is loaded.
    /// </summary>
    public event EventHandler<ChangeEventArgs<int>> PageIndexChanged;

    /// <summary>
    /// Gets the zero-based page index.
    /// </summary>
    public int PageIndex { get; private set; } = -1;

    /// <summary>
    /// Gets data.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="contextObject">The optional context object.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The result.</returns>
    protected abstract Task<JsonObjectNode> GetPageDataAsync(int page, object contextObject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Raises the event.
    /// </summary>
    /// <param name="value">The data result.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="kind">The source kind.</param>
    /// <param name="requestTime">The data request date time.</param>
    /// <param name="contextObject">The optional context object.</param>
    protected void ReceiveResult(JsonObjectNode value, int page, WebApiResultSourceTypes kind = WebApiResultSourceTypes.Online, DateTime? requestTime = null, object contextObject = null)
        => ReceiveResult(value, page, false, kind, requestTime, contextObject);

    /// <summary>
    /// Raises the event.
    /// </summary>
    /// <param name="value">The data result.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="requestTime">The data request date time.</param>
    /// <param name="kind">The source kind.</param>
    /// <param name="skipToUpdateCache">true if won't update cache; otherwise, false.</param>
    /// <param name="contextObject">The optional context object.</param>
    protected void ReceiveResult(JsonObjectNode value, int page, DateTime requestTime, WebApiResultSourceTypes kind = WebApiResultSourceTypes.Online, bool skipToUpdateCache = false, object contextObject = null)
        => ReceiveResult(value, page, skipToUpdateCache, kind, requestTime, contextObject);

    /// <summary>
    /// Raises the event.
    /// </summary>
    /// <param name="value">The data result.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="skipToUpdateCache">true if won't update cache; otherwise, false.</param>
    /// <param name="kind">The source kind.</param>
    /// <param name="requestTime">The data request date time.</param>
    /// <param name="contextObject">The optional context object.</param>
    protected void ReceiveResult(JsonObjectNode value, int page, bool skipToUpdateCache, WebApiResultSourceTypes kind = WebApiResultSourceTypes.Online, DateTime? requestTime = null, object contextObject = null)
    {
        if (!skipToUpdateCache) cache[page] = value;
        DataLoaded?.Invoke(this, new(value, page, kind, requestTime, contextObject));
    }

    /// <summary>
    /// Loads data of next page.
    /// </summary>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> LoadNextPageAsync(CancellationToken cancellationToken = default)
        => LoadPageAsync(PageIndex + 1, cancellationToken);

    /// <summary>
    /// Loads data of next page.
    /// </summary>
    /// <param name="contextObject">The optional context object.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result.</returns>
    public Task<JsonObjectNode> LoadNextPageAsync(object contextObject, CancellationToken cancellationToken = default)
        => LoadPageAsync(PageIndex + 1, contextObject, cancellationToken);

    /// <summary>
    /// Resets page index.
    /// </summary>
    public void ResetPageIndex()
    {
        if (PageIndex == -1) return;
        var old = PageIndex;
        PageIndex = -1;
        PageIndexChanged?.Invoke(this, new(old, PageIndex, nameof(PageIndex)));
    }

    /// <summary>
    /// Clears cache.
    /// </summary>
    public void ClearCache()
    {
        cache.Clear();
        ResetPageIndex();
    }

    /// <summary>
    /// Loads data of the specific page.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result; null if skip, or the result is null.</returns>
    public Task<JsonObjectNode> LoadPageAsync(int page, CancellationToken cancellationToken = default)
        => LoadPageAsync(page, false, null, cancellationToken);

    /// <summary>
    /// Loads data of the specific page.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="contextObject">The optional context object.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result; null if skip, or the result is null.</returns>
    public Task<JsonObjectNode> LoadPageAsync(int page, object contextObject, CancellationToken cancellationToken = default)
        => LoadPageAsync(page, false, contextObject, cancellationToken);

    /// <summary>
    /// Loads data of the specific page.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="wait">true if wait to continue; otherwise, false, to skip on working.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result; null if skip, or the result is null.</returns>
    public Task<JsonObjectNode> LoadPageAsync(int page, bool wait, CancellationToken cancellationToken = default)
        => LoadPageAsync(page, wait, null, cancellationToken);

    /// <summary>
    /// Loads data of the specific page.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="wait">true if wait to continue; otherwise, false, to skip on working.</param>
    /// <param name="contextObject">The optional context object.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns>The result; null if skip, or the result is null.</returns>
    public async Task<JsonObjectNode> LoadPageAsync(int page, bool wait, object contextObject, CancellationToken cancellationToken = default)
    {
        if (page < 0) return null;
        if (!wait && slim.CurrentCount < 0) return null;
        var pendingTime = DateTime.Now;
        JsonObjectNode result;
        int old;
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

            old = PageIndex;
            if (cache.TryGetValue(page, out result) && result != null) return result;
            var requestTime = DateTime.Now;
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
                DataLoaded?.Invoke(this, new(result, page, WebApiResultSourceTypes.Online, pendingTime, requestTime, null, contextObject));
            }

            PageIndex = page;
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

        PageIndexChanged?.Invoke(this, new(old, page, nameof(PageIndex)));
        return result;
    }
}
