using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Text;

namespace Trivial.UI;

public delegate Task<LocalWebAppMessageResponseBody> LocalWebAppMessageProcessAsync(LocalWebAppMessageRequest request);

public class LocalWebAppMessageRequest
{
    public Uri Uri { get; set; }

    public string TraceId { get; set; }

    public string ActionName { get; set; }

    public string MessageHandlerId { get; set; }

    public JsonObjectNode Data { get; set; }

    public JsonObjectNode AdditionalInfo { get; set; }

    public JsonObjectNode Context { get; set; }
}

public class LocalWebAppMessageResponseBody
{
    public JsonObjectNode Data { get; set; }

    public JsonObjectNode AdditionalInfo { get; set; }

    public string Message { get; set; }

    public bool IsError { get; set; }
}
