using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Web;

internal interface IHttpResponseFilling
{
    public void ExecuteResult(HttpResponse response);
}

internal class HttpResponseHeadersFilling(EntityTagHeaderValue entityTag, DateTime? lastModified = null, DateTime? expires = null, CacheControlHeaderValue cacheControl = null) : IHttpResponseFilling
{
    public void ExecuteResult(HttpResponse response)
    {
        if (response == null) return;
        if (entityTag != null) response.Headers.ETag = entityTag.ToString();
        if (lastModified.HasValue) response.Headers.LastModified = ControllerExtensions.ToString(lastModified.Value);
        if (expires.HasValue) response.Headers.Expires = ControllerExtensions.ToString(expires.Value);
        if (cacheControl != null) response.Headers.CacheControl = cacheControl.ToString();
    }
}

internal class HttpResponseStatusFilling(int statusCode) : IHttpResponseFilling
{
    public void ExecuteResult(HttpResponse response)
    {
        if (response == null) return;
        response.StatusCode = statusCode;
    }
}
