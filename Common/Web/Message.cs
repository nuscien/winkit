using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Text;

namespace Trivial.Web;

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
    /// Gets or sets a value indicating whether the source is full trusted.
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
/// The response message for the local standalone web app.
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
    /// <param name="info">The additional information.</param>
    public LocalWebAppResponseMessage(string errorMessage, JsonObjectNode data = null, bool isError = true, JsonObjectNode info = null)
    {
        Message = errorMessage;
        Data = data;
        IsError = isError;
        AdditionalInfo = info;
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppResponseMessage class.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="info">The additional information.</param>
    /// <param name="isError">true if it is failed; otherwise, false.</param>
    public LocalWebAppResponseMessage(JsonObjectNode data, JsonObjectNode info = null, bool isError = false)
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

/// <summary>
/// The notification message for the local standalone web app.
/// </summary>
public class LocalWebAppNotificationMessage
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppNotificationMessage class.
    /// </summary>
    public LocalWebAppNotificationMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppNotificationMessage class.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="info">The additional information.</param>
    public LocalWebAppNotificationMessage(JsonObjectNode data, JsonObjectNode info = null)
    {
        Data = data;
        AdditionalInfo = info;
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppNotificationMessage class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="data">The data.</param>
    /// <param name="info">The additional information.</param>
    public LocalWebAppNotificationMessage(string message, JsonObjectNode data, JsonObjectNode info = null)
        : this(data, info)
    {
        Message = message;
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
}

/// <summary>
/// The context of local web app command handler.
/// </summary>
public class LocalWebAppCommandHandlerContext
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppCommandHandlerContext class.
    /// </summary>
    /// <param name="host">The host.</param>
    /// <param name="window">The window state controller.</param>
    /// <param name="browserHandler">The browser handler.</param>
    public LocalWebAppCommandHandlerContext(LocalWebAppHost host, IBasicWindowStateController window, ILocalWebAppBrowserMessageHandler browserHandler)
    {
        Manifest = host.Manifest;
        WindowStateController = window;
        BrowserHandler = browserHandler;
        DataDirectory = host.DataDirectory;
        DataResources = host.DataResources;
        DataStrings = host.DataStrings;
    }

    /// <summary>
    /// Gets the manifest.
    /// </summary>
    public LocalWebAppManifest Manifest { get; }

    /// <summary>
    /// Gets the window state controller.
    /// </summary>
    public IBasicWindowStateController WindowStateController { get; }

    /// <summary>
    /// Gets the browser handler.
    /// </summary>
    public ILocalWebAppBrowserMessageHandler BrowserHandler { get; }

    /// <summary>
    /// Gets the data directory.
    /// </summary>
    public DirectoryInfo DataDirectory { get; }

    /// <summary>
    /// Gets the data resources.
    /// </summary>
    public JsonObjectNode DataResources { get; }

    /// <summary>
    /// Gets the data strings.
    /// </summary>
    public IDictionary<string, string> DataStrings { get; }
}

/// <summary>
/// The command handler for local standalone web app.
/// </summary>
public interface ILocalWebAppCommandHandler
{
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets or sets the description of the command handler.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the version of the command handler.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="args">The manifest of the local web app.</param>
    /// <returns>The response message.</returns>
    public Task<LocalWebAppResponseMessage> Process(LocalWebAppRequestMessage request, LocalWebAppCommandHandlerContext args);
}

/// <summary>
/// The command handler for local standalone web app.
/// </summary>
public abstract class BaseLocalWebAppCommandHandler : ILocalWebAppCommandHandler
{
    /// <summary>
    /// Initializes a new instance of the BaseLocalWebAppCommandHandler class.
    /// </summary>
    /// <param name="id">The handler identifer.</param>
    /// <param name="version">The handler version.</param>
    public BaseLocalWebAppCommandHandler(string id, string version = null)
    {
        Id = id;
        Version = version;
    }

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the version of the command handler.
    /// </summary>
    public string Description { get; protected set; }

    /// <summary>
    /// Gets the version of the command handler.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="args">The manifest of the local web app.</param>
    /// <returns>The response message.</returns>
    public abstract Task<LocalWebAppResponseMessage> Process(LocalWebAppRequestMessage request, LocalWebAppCommandHandlerContext args);
}
