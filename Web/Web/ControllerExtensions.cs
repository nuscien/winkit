﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Tasks;
using Trivial.Text;

namespace Trivial.Web;

/// <summary>
/// The controller extensions.
/// </summary>
public static class ControllerExtensions
{
    private static MethodInfo method;
    private const string CharSet = "charset=utf-8";
    private const string CharSetSep = "; ";
    internal readonly static string jsonMime = string.Concat(JsonValues.JsonMIME, CharSetSep, CharSet);
    internal readonly static string jsonlMime = string.Concat(JsonValues.JsonlMIME, CharSetSep, CharSet);
    internal readonly static string sseMime = string.Concat(WebFormat.ServerSentEventsMIME, CharSetSep, CharSet);
    internal readonly static string textMime = string.Concat(StringExtensions.PlainTextMIME, CharSetSep, CharSet);
    internal readonly static byte[] utf8NewLine = Encoding.UTF8.GetBytes("\n");

    /// <summary>
    /// Gets the content type with UTF-8 character set.
    /// </summary>
    /// <param name="contentType">The content type (MIME).</param>
    /// <returns>A string with the specific content type and character set UTF-8.</returns>
    public static string GetUtf8ContentType(string contentType)
        => string.Concat(contentType, CharSetSep, CharSet);

    /// <summary>
    /// Gets the first string value.
    /// </summary>
    /// <param name="request">The form collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="trim">true if need trim; otherwise, false.</param>
    /// <returns>The string value; or null, if non-exist.</returns>
    public static string GetFirstStringValue(this IFormCollection request, string key, bool trim = false)
    {
        var col = request[key];
        string str = null;
        if (trim)
        {
            foreach (var ele in col)
            {
                if (ele == null) continue;
                var s = ele.Trim();
                if (s.Length == 0)
                {
                    str ??= string.Empty;
                    continue;
                }

                str = s;
            }
        }
        else
        {
            foreach (var ele in col)
            {
                if (ele == null) continue;
                if (ele.Length == 0)
                {
                    str ??= string.Empty;
                    continue;
                }

                str = ele;
            }
        }

        return str;
    }

    /// <summary>
    /// Gets the merged string value.
    /// </summary>
    /// <param name="request">The form collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="split">The splitter charactor.</param>
    /// <param name="trim">true if need trim; otherwise, false.</param>
    /// <returns>The string value; or null, if non-exist.</returns>
    public static string GetMergedStringValue(this IFormCollection request, string key, char split = ',', bool trim = false)
    {
        IEnumerable<string> col = request[key];
        if (trim) col = col.Select(ele => ele?.Trim()).Where(ele => !string.IsNullOrEmpty(ele));
        return string.Join(split, col);
    }

    /// <summary>
    /// Gets the first string value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="trim">true if need trim; otherwise, false.</param>
    /// <returns>The string value; or null, if non-exist.</returns>
    public static string GetFirstStringValue(this IQueryCollection request, string key, bool trim = false)
    {
        var col = request[key];
        string str = null;
        if (trim)
        {
            foreach (var ele in col)
            {
                if (ele == null) continue;
                var s = ele.Trim();
                if (s.Length == 0)
                {
                    str ??= string.Empty;
                    continue;
                }

                str = s;
            }
        }
        else
        {
            foreach (var ele in col)
            {
                if (ele == null) continue;
                if (ele.Length == 0)
                {
                    str ??= string.Empty;
                    continue;
                }

                str = ele;
            }
        }

        return str;
    }

    /// <summary>
    /// Gets the merged string value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="split">The splitter charactor.</param>
    /// <param name="trim">true if need trim; otherwise, false.</param>
    /// <returns>The string value; or null, if non-exist.</returns>
    public static string GetMergedStringValue(this IQueryCollection request, string key, char split = ',', bool trim = false)
    {
        IEnumerable<string> col = request[key];
        if (trim) col = col.Select(ele => ele?.Trim()).Where(ele => !string.IsNullOrEmpty(ele));
        return string.Join(split, col);
    }

    /// <summary>
    /// Gets the integer value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <returns>The number value; or null, if non-exist or parse failed.</returns>
    public static int? TryGetInt32Value(this IQueryCollection request, string key)
    {
        var s = request[key].Select(ele => ele?.Trim()).FirstOrDefault(ele => !string.IsNullOrEmpty(ele));
        if (Maths.Numbers.TryParseToInt32(s, 10, out var r)) return r;
        return null;
    }

    /// <summary>
    /// Gets the integer value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="result">The output result.</param>
    /// <returns>true if parse succeeded; otherwise, false.</returns>
    public static bool TryGetInt32Value(this IQueryCollection request, string key, out int result)
    {
        var r = TryGetInt32Value(request, key);
        if (r.HasValue)
        {
            result = r.Value;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets the integer value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <returns>The number value; or null, if non-exist or parse failed.</returns>
    public static long? TryGetInt64Value(this IQueryCollection request, string key)
    {
        var s = request[key].Select(ele => ele?.Trim()).FirstOrDefault(ele => !string.IsNullOrEmpty(ele));
        if (Maths.Numbers.TryParseToInt64(s, 10, out var r)) return r;
        return null;
    }

    /// <summary>
    /// Gets the integer value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="result">The output result.</param>
    /// <returns>true if parse succeeded; otherwise, false.</returns>
    public static bool TryGetInt64Value(this IQueryCollection request, string key, out long result)
    {
        var r = TryGetInt64Value(request, key);
        if (r.HasValue)
        {
            result = r.Value;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets the floating value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <returns>The number value; or null, if non-exist or parse failed.</returns>
    public static float? TryGetSingleValue(this IQueryCollection request, string key)
    {
        var s = request[key].Select(ele => ele?.Trim()).FirstOrDefault(ele => !string.IsNullOrEmpty(ele));
        if (float.TryParse(s, out var r)) return r;
        return null;
    }

    /// <summary>
    /// Gets the floating value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="result">The output result.</param>
    /// <returns>true if parse succeeded; otherwise, false.</returns>
    public static bool TryGetSingleValue(this IQueryCollection request, string key, out float result)
    {
        var r = TryGetSingleValue(request, key);
        if (r.HasValue)
        {
            result = r.Value;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets the floating value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <returns>The number value; or null, if non-exist or parse failed.</returns>
    public static decimal? TryGetDecimalValue(this IQueryCollection request, string key)
    {
        var s = request[key].Select(ele => ele?.Trim()).FirstOrDefault(ele => !string.IsNullOrEmpty(ele));
        if (decimal.TryParse(s, out var r)) return r;
        return null;
    }

    /// <summary>
    /// Gets the floating value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="result">The output result.</param>
    /// <returns>true if parse succeeded; otherwise, false.</returns>
    public static bool TryGetDecimalValue(this IQueryCollection request, string key, out decimal result)
    {
        var r = TryGetDecimalValue(request, key);
        if (r.HasValue)
        {
            result = r.Value;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Gets the floating value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <returns>The number value; or null, if non-exist or parse failed.</returns>
    public static double? TryGetDoubleValue(this IQueryCollection request, string key)
    {
        var s = request[key].Select(ele => ele?.Trim()).FirstOrDefault(ele => !string.IsNullOrEmpty(ele));
        if (double.TryParse(s, out var r)) return r;
        return null;
    }

    /// <summary>
    /// Gets the floating value.
    /// </summary>
    /// <param name="request">The query collection.</param>
    /// <param name="key">The key.</param>
    /// <param name="result">The output result.</param>
    /// <returns>true if parse succeeded; otherwise, false.</returns>
    public static bool TryGetDoubleValue(this IQueryCollection request, string key, out double result)
    {
        var r = TryGetDoubleValue(request, key);
        if (r.HasValue)
        {
            result = r.Value;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Tries to get a property as boolean.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="key">The property key.</param>
    /// <returns>true if it is true; or false, if it is false; or null, if not supported.</returns>
    public static bool? TryGetBoolean(this IQueryCollection request, string key)
    {
        var plain = request?.GetFirstStringValue(key, true)?.ToLowerInvariant();
        var isPlain = JsonBooleanNode.TryParse(plain);
        return isPlain?.Value;
    }

    /// <summary>
    /// Tries to get a property as boolean.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="key">The property key.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if parse succeeded; otherwise, false.</returns>
    public static bool TryGetBoolean(this IQueryCollection request, string key, out bool result)
    {
        var plain = request?.GetFirstStringValue(key, true)?.ToLowerInvariant();
        var isPlain = JsonBooleanNode.TryParse(plain);
        if (isPlain == null)
        {
            result = false;
            return false;
        }

        result = isPlain.Value;
        return true;
    }

    /// <summary>
    /// Gets the query data.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <returns>The string value; or null, if non-exist.</returns>
    public static async Task<string> ReadBodyAsStringAsync(this HttpRequest request, Encoding encoding = null)
    {
        if (request == null || request.Body == null) return null;
        encoding ??= Encoding.UTF8;
        using var reader = new StreamReader(request.Body, encoding);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Gets the query data.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <returns>The string value; or null, if non-exist.</returns>
    public static async Task<QueryData> ReadBodyAsQueryDataAsync(this HttpRequest request, Encoding encoding = null)
    {
        var query = await ReadBodyAsStringAsync(request, encoding);
        if (query == null) return null;
        var q = new QueryData();
        q.ParseSet(query, false, encoding);
        return q;
    }

    /// <summary>
    /// Gets the JSON object from body.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A JSON object instance; or null, if no body.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public static Task<JsonObjectNode> ReadBodyAsJsonAsync(this HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null || request.Body == null) return null;
        return JsonObjectNode.ParseAsync(request.Body, default, cancellationToken);
    }

    /// <summary>
    /// Gets the JSON object from body.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A JSON object instance; or null, if no body.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public static Task<JsonObjectNode> ReadBodyAsJsonAsync(this HttpRequest request, JsonDocumentOptions options, CancellationToken cancellationToken = default)
    {
        if (request == null || request.Body == null) return null;
        return JsonObjectNode.ParseAsync(request.Body, options, cancellationToken);
    }

    /// <summary>
    /// Gets the JSON array from body.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A JSON array instance; or null, if no body.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public static Task<JsonArrayNode> ReadBodyAsJsonArrayAsync(this HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null || request.Body == null) return null;
        return JsonArrayNode.ParseAsync(request.Body, default, cancellationToken);
    }

    /// <summary>
    /// Gets the JSON array from body.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="options">Options to control the reader behavior during parsing.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A JSON array instance; or null, if no body.</returns>
    /// <exception cref="JsonException">json does not represent a valid single JSON object.</exception>
    /// <exception cref="ArgumentException">options contains unsupported options.</exception>
    public static Task<JsonArrayNode> ReadBodyAsJsonArrayAsync(this HttpRequest request, JsonDocumentOptions options, CancellationToken cancellationToken = default)
    {
        if (request == null || request.Body == null) return null;
        return JsonArrayNode.ParseAsync(request.Body, options, cancellationToken);
    }

    /// <summary>
    /// Parses a JWT string encoded.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="algorithmFactory">The signature algorithm factory.</param>
    /// <param name="verify">true if verify the signature; otherwise, false.</param>
    /// <returns>A JSON web token object.</returns>
    /// <exception cref="InvalidOperationException">Verify failure.</exception>
    /// <exception cref="JsonException">Deserialize failed.</exception>
    public static JsonWebToken<T> GetJsonWebToken<T>(this HttpRequest request, Func<T, string, ISignatureProvider> algorithmFactory, bool verify = true)
    {
        var auth = request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(auth)) return null;
        try
        {
            return JsonWebToken<T>.Parse(auth, algorithmFactory, true);
        }
        catch (ArgumentException)
        {
        }
        catch (FormatException)
        {
        }

        return null;
    }

    /// <summary>
    /// Parses a JWT string encoded.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="algorithm">The signature algorithm.</param>
    /// <param name="verify">true if verify the signature; otherwise, false.</param>
    /// <returns>A JSON web token object.</returns>
    /// <exception cref="InvalidOperationException">Verify failure.</exception>
    /// <exception cref="JsonException">Deserialize failed.</exception>
    public static JsonWebToken<T> GetJsonWebToken<T>(this HttpRequest request, ISignatureProvider algorithm, bool verify = true)
    {
        var auth = request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(auth)) return null;
        try
        {
            return JsonWebToken<T>.Parse(auth, algorithm, true);
        }
        catch (ArgumentException)
        {
        }
        catch (FormatException)
        {
        }

        return null;
    }

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="data">The source data to output.</param>
    /// <param name="handler">The handler to fill data into response body.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult<T>(T data, Func<T, HttpResponse, Task> handler, Action<HttpResponse> prepare = null)
        => new DataHandlingActionResult<T>(data, handler, prepare);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The action result.</returns>
    public static ActionResult ToActionResult(this ChangingResultInfo value)
    {
        if (value == null) return new NotFoundResult();
        var ex = value.GetException();
        var status = ex != null ? (GetStatusCode(ex) ?? 500) : 200;
        if (status >= 300)
        {
            status = value.ErrorCode switch
            {
                ChangeErrorKinds.NotFound => 404,
                ChangeErrorKinds.Busy => 503,
                ChangeErrorKinds.Unsupported => 501,
                ChangeErrorKinds.Conflict => 409,
                _ => status
            };
        }

        return new JsonResult(value)
        {
            StatusCode = status
        };
    }

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The action result.</returns>
    public static ActionResult ToActionResult(this ChangeMethods value)
        => ToActionResult(new ChangingResultInfo(value));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The action result.</returns>
    public static ActionResult ToActionResult(this ChangeErrorKinds value, string message)
        => ToActionResult(new ChangingResultInfo(value, message));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this JsonObjectNode value)
        => new JsonValueNodeActionResult(value, null, null);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this JsonObjectNode value, Action<HttpResponse> prepare)
        => new JsonValueNodeActionResult(value, prepare, null);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="lastModified">The optional last modification date time of the resource.</param>
    /// <param name="expires">The optional expiration date time of the resource.</param>
    /// <param name="cacheControl">An optional policy to control caching in browsers and shared caches.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this JsonObjectNode value, EntityTagHeaderValue entityTag, DateTime? lastModified = null, DateTime? expires = null, CacheControlHeaderValue cacheControl = null)
        => new JsonValueNodeActionResult(value, null, null, new HttpResponseHeadersFilling(entityTag, lastModified, expires, cacheControl));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="lastModified">The optional last modification date time of the resource.</param>
    /// <param name="expires">The optional expiration date time of the resource.</param>
    /// <param name="cacheControl">An optional policy to control caching in browsers and shared caches.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this JsonObjectNode value, Action<HttpResponse> prepare, EntityTagHeaderValue entityTag, DateTime? lastModified = null, DateTime? expires = null, CacheControlHeaderValue cacheControl = null)
        => new JsonValueNodeActionResult(value, prepare, null, new HttpResponseHeadersFilling(entityTag, lastModified, expires, cacheControl));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this JsonObjectNode value, int statusCode, Action<HttpResponse> prepare = null)
        => new JsonValueNodeActionResult(value, prepare, null, new HttpResponseStatusFilling(statusCode));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this JsonArrayNode value)
        => new JsonValueNodeActionResult(value, null, null);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this JsonArrayNode value, Action<HttpResponse> prepare)
        => new JsonValueNodeActionResult(value, prepare, null);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this JsonArrayNode value, int statusCode, Action<HttpResponse> prepare = null)
        => new JsonValueNodeActionResult(value, prepare, null, new HttpResponseStatusFilling(statusCode));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IJsonObjectHost value)
        => new JsonValueNodeActionResult(value?.ToJson(), null, null);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IJsonObjectHost value, Action<HttpResponse> prepare)
        => new JsonValueNodeActionResult(value?.ToJson(), prepare, null);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="lastModified">The optional last modification date time of the resource.</param>
    /// <param name="expires">The optional expiration date time of the resource.</param>
    /// <param name="cacheControl">An optional policy to control caching in browsers and shared caches.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IJsonObjectHost value, EntityTagHeaderValue entityTag, DateTime? lastModified = null, DateTime? expires = null, CacheControlHeaderValue cacheControl = null)
        => new JsonValueNodeActionResult(value?.ToJson(), null, null, new HttpResponseHeadersFilling(entityTag, lastModified, expires, cacheControl));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="lastModified">The optional last modification date time of the resource.</param>
    /// <param name="expires">The optional expiration date time of the resource.</param>
    /// <param name="cacheControl">An optional policy to control caching in browsers and shared caches.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IJsonObjectHost value, Action<HttpResponse> prepare, EntityTagHeaderValue entityTag = null, DateTime? lastModified = null, DateTime? expires = null, CacheControlHeaderValue cacheControl = null)
        => new JsonValueNodeActionResult(value?.ToJson(), prepare, null, new HttpResponseHeadersFilling(entityTag, lastModified, expires, cacheControl));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IJsonObjectHost value, int statusCode, Action<HttpResponse> prepare = null)
        => new JsonValueNodeActionResult(value?.ToJson(), prepare, null, new HttpResponseStatusFilling(statusCode));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="lastModified">The optional last modification date time of the resource.</param>
    /// <param name="expires">The optional expiration date time of the resource.</param>
    /// <param name="cacheControl">An optional policy to control caching in browsers and shared caches.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this BaseJsonRpcResponseObject value, EntityTagHeaderValue entityTag, DateTime? lastModified = null, DateTime? expires = null, CacheControlHeaderValue cacheControl = null)
        => new JsonWriterActionResult(value.Write, null, null, new HttpResponseHeadersFilling(entityTag, lastModified, expires, cacheControl));

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The action result.</returns>
    public static ContentResult ToActionResult(System.Text.Json.Nodes.JsonObject value)
        => new()
        {
            ContentType = jsonMime,
            StatusCode = 200,
            Content = value?.ToJsonString() ?? JsonValues.NullString
        };

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The action result.</returns>
    public static ContentResult ToActionResult(System.Text.Json.Nodes.JsonArray value)
        => new()
        {
            ContentType = jsonMime,
            StatusCode = 200,
            Content = value?.ToJsonString() ?? JsonValues.NullString
        };

    /// <summary>
    /// Writes the event information into a stream.
    /// </summary>
    /// <param name="data">The server-sent event info collection to write.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>The count of server-sent event writen.</returns>
    public static Task<int> WriteToAsync(this IEnumerable<ServerSentEventInfo> data, HttpResponse response)
        => WriteDataToAsync(data, sseMime, ListExtensions.WriteToAsync, response);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IEnumerable<ServerSentEventInfo> data, Action<HttpResponse> prepare = null)
        => ToActionResult(data, WriteToAsync, prepare);

    /// <summary>
    /// Writes the event information into a stream.
    /// </summary>
    /// <param name="data">The server-sent event info collection to write.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>The count of server-sent event writen.</returns>
    public static Task<int> WriteToAsync(this IAsyncEnumerable<ServerSentEventInfo> data, HttpResponse response)
        => WriteDataToAsync(data, sseMime, WriteToAsync, response);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IAsyncEnumerable<ServerSentEventInfo> data, Action<HttpResponse> prepare = null)
        => ToActionResult(data, WriteToAsync, prepare);

    /// <summary>
    /// Writes the event information into a stream.
    /// </summary>
    /// <param name="data">The server-sent event info collection to write.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>The count of the JSON writen.</returns>
    public static Task<int> WriteToAsync(this IAsyncEnumerable<JsonObjectNode> data, HttpResponse response)
        => WriteJsonlToAsync(data, response);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IAsyncEnumerable<JsonObjectNode> data, Action<HttpResponse> prepare = null)
        => ToActionResult(data, WriteToAsync, prepare);

    /// <summary>
    /// Writes the event information into a stream.
    /// </summary>
    /// <param name="data">The server-sent event info collection to write.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>The count of the JSON writen.</returns>
    public static Task<int> WriteToAsync(this IAsyncEnumerable<BaseJsonValueNode> data, HttpResponse response)
        => WriteJsonlToAsync(data, response);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IAsyncEnumerable<BaseJsonValueNode> data, Action<HttpResponse> prepare = null)
        => ToActionResult(data, WriteToAsync, prepare);

    /// <summary>
    /// Writes the event information into a stream.
    /// </summary>
    /// <param name="data">The server-sent event info collection to write.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>The count of the JSON writen.</returns>
    public static Task<int> WriteToAsync(this IAsyncEnumerable<IJsonValueNode> data, HttpResponse response)
        => WriteJsonlToAsync(data, response);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult(this IAsyncEnumerable<IJsonValueNode> data, Action<HttpResponse> prepare = null)
        => ToActionResult(data, WriteToAsync, prepare);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult<T>(Action<CollectionResultBuilder<T>> append, Action<HttpResponse> prepare = null)
        => new PushingCollectionActionResult<T>(append, prepare);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="builder">The optional builder initialized.</param>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult<T>(CollectionResultBuilder<T> builder, Action<CollectionResultBuilder<T>> append, Action<HttpResponse> prepare = null)
        => new PushingCollectionActionResult<T>(append, prepare, builder);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult<T>(Func<CollectionResultBuilder<T>, Task> append, Action<HttpResponse> prepare = null)
        => new PushingCollectionActionResult<T>(append, prepare);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="builder">The optional builder initialized.</param>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult<T>(CollectionResultBuilder<T> builder, Func<CollectionResultBuilder<T>, Task> append, Action<HttpResponse> prepare = null)
        => new PushingCollectionActionResult<T>(append, prepare, builder);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult<T>(Func<CollectionResultBuilder<T>, CancellationToken, Task> append, Action<HttpResponse> prepare = null)
        => new PushingCollectionActionResult<T>(append, prepare);

    /// <summary>
    /// Convert to an action result.
    /// </summary>
    /// <param name="builder">The optional builder initialized.</param>
    /// <param name="append">The callback for collection result builder.</param>
    /// <param name="prepare">The preparing callback.</param>
    /// <returns>The action result.</returns>
    public static IActionResult ToActionResult<T>(CollectionResultBuilder<T> builder, Func<CollectionResultBuilder<T>, CancellationToken, Task> append, Action<HttpResponse> prepare = null)
        => new PushingCollectionActionResult<T>(append, prepare, builder);

    /// <summary>
    /// Writes the strings into a stream.
    /// </summary>
    /// <param name="lines">The lines to write.</param>
    /// <param name="mime">The content type.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>The count of line.</returns>
    public static async Task<int> WriteToAsync(BaseLinesStringTableParser lines, string mime, HttpResponse response)
    {
        var data = lines?.ToLinesString();
        if (data == null) return 0;
        var writer = new StreamWriter(response.Body, Encoding.UTF8);
        var i = 0;
        foreach (var line in data)
        {
            if (i > 0) writer.Write('\n');
            await writer.WriteLineAsync(line);
            i++;
            await writer.FlushAsync();
        }

        return i;
    }

    /// <summary>
    /// Writes the CSV content into a stream.
    /// </summary>
    /// <param name="lines">The lines to write.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>The count of CSV record.</returns>
    public static Task<int> WriteToAsync(this CsvParser lines, HttpResponse response)
        => WriteToAsync(lines, CsvParser.MIME, response);

    /// <summary>
    /// Writes the CSV content into a stream.
    /// </summary>
    /// <param name="lines">The lines to write.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>The count of CSV record.</returns>
    public static Task<int> WriteToAsync(this TsvParser lines, HttpResponse response)
        => WriteToAsync(lines, TsvParser.MIME, response);

    /// <summary>
    /// Converts an exception to action result with exception message.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="ex">The exception.</param>
    /// <param name="ignoreUnknownException">true if return null for unknown exception; otherwise, false.</param>
    /// <returns>The action result.</returns>
    public static ActionResult ExceptionResult(this ControllerBase controller, Exception ex, bool ignoreUnknownException = false)
    {
        if (ex == null) return controller.StatusCode(500);
        var result = new ErrorMessageResult(ex);
        var status = GetStatusCode(ex, ignoreUnknownException);
        if (!status.HasValue) return null;
        return new JsonResult(result)
        {
            StatusCode = status.Value
        };
    }

    /// <summary>
    /// Converts an exception to action result with exception message.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="ex">The exception.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="ignoreUnknownException">true if return null for unknown exception; otherwise, false.</param>
    /// <returns>The action result.</returns>
    public static ActionResult ExceptionResult(this ControllerBase controller, Exception ex, string errorCode, bool ignoreUnknownException = false)
    {
        if (ex == null) return controller.StatusCode(500);
        var result = new ErrorMessageResult(ex, errorCode);
        var status = GetStatusCode(ex, ignoreUnknownException);
        if (!status.HasValue) return null;
        return new JsonResult(result)
        {
            StatusCode = status.Value
        };
    }

    /// <summary>
    /// Converts an exception to action result with exception message.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="status">The HTTP status code.</param>
    /// <param name="ex">The exception.</param>
    /// <returns>The action result.</returns>
    public static ActionResult ExceptionResult(this ControllerBase controller, int status, Exception ex)
    {
        if (ex == null) return controller.StatusCode(status);
        var result = new ErrorMessageResult(ex);
        return new JsonResult(result)
        {
            StatusCode = status
        };
    }

    /// <summary>
    /// Converts an exception to action result with exception message.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="status">The HTTP status code.</param>
    /// <param name="ex">The exception.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>The action result.</returns>
    public static ActionResult ExceptionResult(this ControllerBase controller, int status, Exception ex, string errorCode)
    {
        if (ex == null) return controller.StatusCode(status);
        var result = new ErrorMessageResult(ex, errorCode);
        return new JsonResult(result)
        {
            StatusCode = status
        };
    }

    /// <summary>
    /// Converts an exception to action result with exception message.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="status">The HTTP status code.</param>
    /// <param name="ex">The exception message.</param>
    /// <param name="errorCode">The optional error code.</param>
    /// <returns>The action result.</returns>
#pragma warning disable IDE0060
    public static ActionResult ExceptionResult(this ControllerBase controller, int status, string ex, string errorCode = null)
#pragma warning restore IDE0060
    {
        var result = new ErrorMessageResult(ex, errorCode);
        return new JsonResult(result)
        {
            StatusCode = status
        };
    }

    /// <summary>
    /// Gets the query data instance.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The query data instance.</returns>
    public static QueryData GetQueryData(this IQueryCollection request)
    {
        if (request == null) return null;
        var q = new QueryData();
        foreach (var item in request)
        {
            q.Add(item.Key, item.Value as IEnumerable<string>);
        }

        q.Remove("random");
        return q;
    }

    /// <summary>
    /// Signs in.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="route">The token request route.</param>
    /// <param name="tokenMaker">The token maker.</param>
    /// <returns>The login response.</returns>
    public static async Task<TToken> SignInAsync<TToken, TAccount>(ControllerBase controller, TokenRequestRoute<TAccount> route, Func<TToken> tokenMaker) where TToken : TokenInfo
    {
        TToken result;
        try
        {
            if (controller is null) return default;
            var stream = controller.Request.Body;
            tokenMaker ??= () => Activator.CreateInstance<TToken>();
            if (stream is null)
            {
                result = tokenMaker();
                result.ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest;
                result.ErrorDescription = "The body was empty.";
                return default;
            }

            string input;
            using (var reader = new StreamReader(controller.Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            var r = await route.SignInAsync(input);
            result = r?.ItemSelected as TToken;
            if (result != null) return result;
            result = tokenMaker();
            result.ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest;
            result.ErrorDescription = "Cannot sign in.";
            return result;
        }
        catch (ArgumentException ex)
        {
            result = tokenMaker();
            result.ErrorCode = TokenInfo.ErrorCodeConstants.InvalidRequest;
            result.ErrorDescription = ex.Message;
            return result;
        }
        catch (IOException ex)
        {
            result = tokenMaker();
            result.ErrorCode = TokenInfo.ErrorCodeConstants.ServerError;
            result.ErrorDescription = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Converts a JSON format string to a result.
    /// </summary>
    /// <param name="json">The JSON format string.</param>
    /// <returns>The content result converted.</returns>
    public static ContentResult JsonStringResult(string json)
        => new()
        {
            StatusCode = 200,
            ContentType = jsonMime,
            Content = json
        };

    /// <summary>
    /// Creates a file result.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="mime">The optional MIME content type; or null, if detects automatically.</param>
    /// <returns>A file result; or null, if non-exists.</returns>
    public static FileStreamResult FileResult(FileInfo source, EntityTagHeaderValue entityTag = null, string mime = null)
    {
        if (source == null || !source.Exists) return null;
        mime ??= GetByFileExtension(source.Extension, WebFormat.StreamMIME);
        var result = new FileStreamResult(source.OpenRead(), mime)
        {
            LastModified = source.LastWriteTime,
            FileDownloadName = source.Name,
            EntityTag = entityTag
        };
        return result;
    }

    /// <summary>
    /// Creates a file result.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="mime">The optional MIME content type; or null, if detects automatically.</param>
    /// <returns>A file result; or null, if non-exists.</returns>
    public static FileStreamResult FileResult(IO.BaseFileReferenceInfo<FileInfo> source, EntityTagHeaderValue entityTag = null, string mime = null)
        => FileResult(source?.Source, entityTag, mime);

    /// <summary>
    /// Creates a file result.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="downloadName">The file name used for downloading.</param>
    /// <param name="lastModified">The last modified information associated with the file.</param>
    /// <param name="mime">The optional MIME content type; or null, if detects automatically.</param>
    /// <returns>A file result; or null, if non-exists.</returns>
    public static FileStreamResult FileResult(Stream source, string downloadName, DateTimeOffset? lastModified = null, string mime = null)
        => FileResult(source, downloadName, null, lastModified, mime);

    /// <summary>
    /// Creates a file result.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="downloadName">The file name used for downloading.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="lastModified">The last modified information associated with the file.</param>
    /// <param name="mime">The optional MIME content type; or null, if detects automatically.</param>
    /// <returns>A file result; or null, if stream source is null.</returns>
    public static FileStreamResult FileResult(Stream source, string downloadName, EntityTagHeaderValue entityTag, DateTimeOffset? lastModified = null, string mime = null)
    {
        if (source == null) return null;
        if (string.IsNullOrEmpty(mime) && downloadName?.Contains('.') == true)
        {
            var ext = downloadName.Trim().Split('.').LastOrDefault();
            if (string.IsNullOrEmpty(ext))
                mime = WebFormat.StreamMIME;
            else
                mime = GetByFileExtension("." + ext, WebFormat.StreamMIME);
        }

        var result = new FileStreamResult(source, mime)
        {
            LastModified = lastModified,
            EntityTag = entityTag
        };
        if (!string.IsNullOrWhiteSpace(downloadName))
            result.FileDownloadName = downloadName;
        return result;
    }

    /// <summary>
    /// Creates a file result.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="downloadName">The file name used for downloading.</param>
    /// <param name="enableRangeProcessing">true if enables range requests processing; otherwise, false.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="lastModified">The last modified information associated with the file.</param>
    /// <param name="mime">The optional MIME content type; or null, if detects automatically.</param>
    /// <returns>A file result; or null, if stream source is null.</returns>
    public static FileStreamResult FileResult(Stream source, string downloadName, EntityTagHeaderValue entityTag, bool enableRangeProcessing, DateTimeOffset? lastModified = null, string mime = null)
    {
        var result = FileResult(source, downloadName, entityTag, lastModified, mime);
        if (result == null) return null;
        result.EnableRangeProcessing = enableRangeProcessing;
        return result;
    }

    /// <summary>
    /// Creates a file result.
    /// </summary>
    /// <param name="assembly">The assembly with the embedded file.</param>
    /// <param name="subPath">The sub path of the embedded file.</param>
    /// <param name="downloadName">The file name used for downloading.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="mime">The optional MIME content type; or null, if detects automatically.</param>
    /// <returns>A file result; or null, if non-exists.</returns>
    public static FileStreamResult FileResult(Assembly assembly, string subPath, string downloadName, EntityTagHeaderValue entityTag, string mime = null)
    {
        if (string.IsNullOrWhiteSpace(subPath)) return null;
        if (assembly == null)
            assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(subPath);
        if (stream == null) return null;
        var file = IO.FileSystemInfoUtility.TryGetFileInfo(assembly.Location);
        var lastModified = file?.LastWriteTime;
        if (string.IsNullOrWhiteSpace(downloadName))
            downloadName = subPath.Split(new[] { '\\', '/' }).LastOrDefault();
        return FileResult(stream, downloadName, entityTag, lastModified, mime);
    }

    /// <summary>
    /// Creates a file result.
    /// </summary>
    /// <param name="assembly">The assembly with the embedded file.</param>
    /// <param name="subPath">The sub path of the embedded file.</param>
    /// <param name="entityTag">The entity tag associated with the file.</param>
    /// <param name="mime">The optional MIME content type; or null, if detects automatically.</param>
    /// <returns>A file result; or null, if non-exists.</returns>
    public static FileStreamResult FileResult(Assembly assembly, string subPath, EntityTagHeaderValue entityTag, string mime = null)
        => FileResult(assembly, subPath, null, entityTag, mime);

    /// <summary>
    /// Converts a date time to a string in HTTP-date format (defined by Date and Time Specification in Internet Message Format).
    /// </summary>
    /// <param name="time">The date time to convert.</param>
    /// <returns>A string in HTTP-date format of a specific date and time.</returns>
    public static string ToString(DateTime time)
    {
        // https://www.rfc-editor.org/rfc/rfc5322.html#section-3.3
        var sb = new StringBuilder();
        time = time.ToUniversalTime();
        sb.Append(time.DayOfWeek switch
        {
            DayOfWeek.Sunday => "Sun, ",
            DayOfWeek.Monday => "Mon, ",
            DayOfWeek.Tuesday => "Tue, ",
            DayOfWeek.Wednesday => "Wed, ",
            DayOfWeek.Thursday => "Thu, ",
            DayOfWeek.Friday => "Fri, ",
            DayOfWeek.Saturday => "Sat, ",
            _ => string.Empty
        });
        sb.Append(time.Day.ToString("00"));
        sb.Append(time.Month switch
        {
            1 => " Jan ",
            2 => " Feb ",
            3 => " Mar ",
            4 => " Apr ",
            5 => " May ",
            6 => " Jun ",
            7 => " Jul ",
            8 => " Aug ",
            9 => " Sep ",
            10 => " Oct ",
            11 => " Nov ",
            12 => " Dec ",
            _ => string.Concat(' ', time.Month, ' ')
        });
        sb.Append(time.Year.ToString("0000"));
        sb.Append(' ');
        sb.Append(time.Hour.ToString("00"));
        sb.Append(':');
        sb.Append(time.Minute.ToString("00"));
        sb.Append(':');
        sb.Append(time.Second.ToString("00"));
        sb.Append(" GMT");
        return sb.ToString();
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="request">The JSON-RPC request object.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
    /// <returns>The JSON-RPC response object.</returns>
    public static async Task<BaseJsonRpcResponseObject> ProcessAsync(this JsonRpcRequestRoute route, HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (route == null || request == null) return null;
        var stream = request.Body;
        if (stream == null) return null;
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
        return await route.ProcessAsync(stream, default);
    }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="request">The JSON-RPC request object collection.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
    /// <returns>The JSON-RPC response object collection.</returns>
    public static async IAsyncEnumerable<BaseJsonRpcResponseObject> ProcessBatchAsync(this JsonRpcRequestRoute route, HttpRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (route == null || request == null) yield break;
        var stream = request.Body;
        if (stream == null) yield break;
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
        var req = JsonSerializer.Deserialize<IEnumerable<JsonRpcRequestObject>>(stream);
        var resp = route.ProcessAsync(req, cancellationToken);
        await foreach (var item in resp)
        {
            yield return item;
        }
    }

    /// <summary>
    /// Gets the status code.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="ignoreUnknownException">true if return null for unknown exception; otherwise, false.</param>
    /// <returns>The action result.</returns>
    private static int? GetStatusCode(Exception ex, bool ignoreUnknownException = false)
    {
        if (ex == null) return 500;
        if (ex.InnerException != null)
        {
            if (ex is AggregateException)
            {
                ex = ex.InnerException;
            }
            else if (ex is InvalidOperationException)
            {
                ex = ex.InnerException;
                ignoreUnknownException = false;
            }
        }

        if (ex is SecurityException) return 403;
        else if (ex is UnauthorizedAccessException) return 401;
        else if (ex is NotSupportedException) return 502;
        else if (ex is NotImplementedException) return 502;
        else if (ex is TimeoutException) return 408;
        else if (ex is OperationCanceledException) return 408;
        if (ignoreUnknownException && !(
            ex is InvalidOperationException
            || ex is ArgumentException
            || ex is NullReferenceException
            || ex is System.Data.Common.DbException
            || ex is JsonException
            || ex is System.Runtime.Serialization.SerializationException
            || ex is FailedHttpException
            || ex is IOException
            || ex is ApplicationException
            || ex is InvalidCastException
            || ex is FormatException
            || ex is ArithmeticException
            || ex is ExternalException
            || ex is InvalidDataException)) return null;
        return 500;
    }

    /// <summary>
    /// Gets the MIME content type by file extension part.
    /// </summary>
    /// <param name="fileExtension">The file extension.</param>
    /// <param name="defaultMime">The default MIME content type.</param>
    /// <returns>The MIME content type.</returns>
    private static string GetByFileExtension(string fileExtension, string defaultMime)
    {
        if (string.IsNullOrWhiteSpace(fileExtension)) return null;
        if (method == null)
        {
            method = typeof(WebFormat).GetMethod("GetMime", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
            if (method == null)
                method = typeof(WebFormat).GetMethod("GetMime", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
            if (method == null)
                return defaultMime;
        }

        var r = method.Invoke(null, new object[] { fileExtension });
        if (r == null) return defaultMime;
        try
        {
            return (string)r;
        }
        catch (InvalidCastException)
        {
        }

        return defaultMime;
    }

    /// <summary>
    /// Writes the event information into a stream.
    /// </summary>
    /// <param name="data">The server-sent event info collection to write.</param>
    /// <param name="mime">The content type to return.</param>
    /// <param name="h">The handler to write stream.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    private static Task<int> WriteDataToAsync<T>(this T data, string mime, Func<T, Stream, Encoding, Task<int>> h, HttpResponse response)
    {
        response.ContentType = mime;
        return h(data, response.Body, Encoding.UTF8);
    }

    /// <summary>
    /// Writes the event information into a stream.
    /// </summary>
    /// <param name="data">The server-sent event info collection to write.</param>
    /// <param name="response">The HTTP response to flush.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    private static async Task<int> WriteJsonlToAsync<T>(IAsyncEnumerable<T> data, HttpResponse response) where T : IJsonValueNode
    {
        response.ContentType = jsonlMime;
        if (data == null) return 0;
        var writer = new StreamWriter(response.Body, Encoding.UTF8);
        var i = 0;
        await foreach (var json in data)
        {
            if (i > 0) writer.Write('\n');
            await writer.WriteAsync(json.ToString());
            i++;
            await writer.FlushAsync();
        }

        return i;
    }

    /// <summary>
    /// Gets the server-sent event format string.
    /// </summary>
    /// <param name="col">The input collection.</param>
    /// <param name="stream">The stream.</param>
    /// <param name="encoding">The encoding; or null, by default, uses UTF-8.</param>
    /// <returns>The count of server-sent event information writen.</returns>
    /// <exception cref="InvalidOperationException">The stream is disposed.</exception>
    /// <exception cref="ArgumentException">stream is not writable.</exception>
    private static async Task<int> WriteToAsync(this IAsyncEnumerable<ServerSentEventInfo> col, Stream stream, Encoding encoding = null)
    {
        if (col == null || stream == null) return 0;
        var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8);
        var i = 0;
        await foreach (var item in col)
        {
            if (item == null) continue;
            if (i > 0) writer.Write('\n');
            await writer.WriteAsync(item.ToResponseString(true));
            await writer.FlushAsync();
            i++;
        }

        return i;
    }
}
