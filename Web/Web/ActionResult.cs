using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;

namespace Trivial.Web;

/// <summary>
/// The internal action result for server-sent event.
/// </summary>
/// <typeparam name="T">The type of the source data.</typeparam>
/// <param name="data">The source data to output.</param>
/// <param name="handler">The handler to fill data into response body.</param>
/// <param name="prepare">The preparing callback.</param>
internal class DataHandlingActionResult<T>(T data, Func<T, HttpResponse, Task> handler, Action<HttpResponse> prepare) : IActionResult
{
    private readonly Func<T, HttpResponse, Task> handler = handler;
    private readonly Action<HttpResponse> prepare = prepare;

    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        prepare?.Invoke(context.HttpContext.Response);
        await handler(data, context.HttpContext.Response);
    }
}

/// <summary>
/// The internal action result for server-sent event.
/// </summary>
/// <typeparam name="T">The type of the source data.</typeparam>
internal class PushingCollectionActionResult<T> : IActionResult
{
    private readonly SemaphoreSlim slim1;
    private readonly SemaphoreSlim slim2;
    private readonly Action<HttpResponse> prepare;
    private CollectionResultBuilder<T> data;
    private Action<CollectionResultBuilder<T>> fill;
    private HttpResponse response;

    /// <summary>
    /// Initializes a new instance of the PushingCollectionActionResult class.
    /// </summary>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <param name="builder">The optional builder initialized.</param>
    public PushingCollectionActionResult(Action<CollectionResultBuilder<T>> append, Action<HttpResponse> prepare, CollectionResultBuilder<T> builder = null)
    {
        this.prepare = prepare;
        if (append == null) return;
        slim1 = new(1, 1);
        slim2 = new(1, 1);
        OnInit(builder);
        fill = append;
    }

    /// <summary>
    /// Initializes a new instance of the PushingCollectionActionResult class.
    /// </summary>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <param name="builder">The optional builder initialized.</param>
    public PushingCollectionActionResult(Func<CollectionResultBuilder<T>, Task> append, Action<HttpResponse> prepare, CollectionResultBuilder<T> builder = null)
    {
        this.prepare = prepare;
        if (append == null) return;
        slim1 = new(1);
        slim2 = new(1);
        OnInit(builder);
        fill = data => _ = append(data);
    }

    /// <summary>
    /// Initializes a new instance of the PushingCollectionActionResult class.
    /// </summary>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <param name="builder">The optional builder initialized.</param>
    public PushingCollectionActionResult(Func<CollectionResultBuilder<T>, CancellationToken, Task> append, Action<HttpResponse> prepare, CollectionResultBuilder<T> builder = null)
    {
        this.prepare = prepare;
        if (append == null) return;
        slim1 = new(1);
        slim2 = new(1);
        OnInit(builder);
        fill = data => _ = append(data, default);
    }

    ~PushingCollectionActionResult()
    {
        try
        {
            slim1.Dispose();
            slim2.Dispose();
            data = null;
            response = null;
        }
        catch (InvalidOperationException)
        {
        }
        catch (SemaphoreFullException)
        {
        }
        catch (NullReferenceException)
        {
        }
    }

    /// <inheritdoc />
    public async Task ExecuteResultAsync(ActionContext context)
    {
        if (data == null) return;
        await WaitAsync();
        try
        {
            var response = context.HttpContext.Response;
            this.response = response;
            response.ContentType = ControllerExtensions.sseMime;
            prepare?.Invoke(response);
            fill(data);
            await WaitAsync();
        }
        finally
        {
            Release();
        }
    }

    private void OnInit(CollectionResultBuilder<T> data)
    {
        data ??= new();
        this.data = data;
        data.Ended += OnDataEnd;
        data.Created += OnDataCreated;
    }

    private void OnDataCreated(object sender, DataEventArgs<ServerSentEventInfo> e)
        => _ = OnDataCreated(e);

    private async Task OnDataCreated(DataEventArgs<ServerSentEventInfo> e)
    {
        try
        {
            await slim2.WaitAsync();
        }
        catch (InvalidOperationException)
        {
            return;
        }
        catch (IOException)
        {
            return;
        }
        catch (ArgumentException)
        {
            return;
        }
        catch (NotSupportedException)
        {
            return;
        }

        try
        {
            var stream = response?.Body;
            var data = e?.Data;
            if (stream == null || data == null) return;
            var s = data.ToResponseString(true);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(s));
            await stream.WriteAsync(ControllerExtensions.utf8NewLine);
            await stream.FlushAsync();
        }
        finally
        {
            try
            {
                slim2.Release();
            }
            catch (InvalidOperationException)
            {
            }
            catch (SemaphoreFullException)
            {
            }
            catch (NullReferenceException)
            {
            }
        }
    }

    private void OnDataEnd(object sender, DataEventArgs<bool> e)
    {
        response = null;
        var data = this.data;
        this.data = null;
        try
        {
            if (data == null) return;
            try
            {
                data.Ended -= OnDataEnd;
                data.Created -= OnDataCreated;
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
        finally
        {
            Release();
        }
    }

    private async Task WaitAsync()
    {
        try
        {
            await slim1.WaitAsync();
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
    }

    private void Release()
    {
        try
        {
            if (slim1.CurrentCount == 0) slim1.Release();
        }
        catch (InvalidOperationException)
        {
        }
        catch (SemaphoreFullException)
        {
        }
        catch (NullReferenceException)
        {
        }
    }
}
