using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
