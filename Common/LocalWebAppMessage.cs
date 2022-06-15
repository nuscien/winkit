using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Text;

namespace Trivial.UI;

/// <summary>
/// The message process handler for local standalone web app.
/// </summary>
/// <param name="request">The request information.</param>
/// <returns>The response.</returns>
public delegate Task<LocalWebAppResponseMessage> LocalWebAppMessageProcessAsync(LocalWebAppRequestMessage request);

/// <summary>
/// The request message for local standalone web app.
/// </summary>
public class LocalWebAppRequestMessage
{
    /// <summary>
    /// Gets or sets the host URI.
    /// </summary>
    public Uri Uri { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the source is full truested.
    /// </summary>
    public bool IsFullTrusted { get; set; }

    /// <summary>
    /// Gets or sets the trace identifier.
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Gets or sets the message handler identifier.
    /// </summary>
    public string MessageHandlerId { get; set; }

    /// <summary>
    /// Gets or sets the request data.
    /// </summary>
    public JsonObjectNode Data { get; set; }

    /// <summary>
    /// Gets or sets the additional information.
    /// </summary>
    public JsonObjectNode AdditionalInfo { get; set; }

    /// <summary>
    /// Gets or sets the additional context data which will return with result.
    /// </summary>
    public JsonObjectNode Context { get; set; }

    /// <summary>
    /// Gets or sets the data and time on process.
    /// </summary>
    public DateTime ProcessingTime { get; set; } = DateTime.Now;
}

/// <summary>
/// The message response body for the local standalone web app.
/// </summary>
public class LocalWebAppResponseMessage
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppResponseMessage class.
    /// </summary>
    public LocalWebAppResponseMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppResponseMessage class.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="data">The data.</param>
    /// <param name="isError">true if it is failed; otherwise, false.</param>
    public LocalWebAppResponseMessage(string errorMessage, JsonObjectNode data = null, bool isError = true)
    {
        Message = errorMessage;
        Data = data;
        IsError = isError;
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppResponseMessage class.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="info">The additional information.</param>
    /// <param name="isError">true if it is failed; otherwise, false.</param>
    public LocalWebAppResponseMessage(JsonObjectNode data, JsonObjectNode info, bool isError = false)
    {
        Data = data;
        AdditionalInfo = info;
        IsError = isError;
    }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    public JsonObjectNode Data { get; set; }

    /// <summary>
    /// Gets or sets the additional information.
    /// </summary>
    public JsonObjectNode AdditionalInfo { get; set; }

    /// <summary>
    /// Gets or sets the state message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the response is in error.
    /// </summary>
    public bool IsError { get; set; }
}
