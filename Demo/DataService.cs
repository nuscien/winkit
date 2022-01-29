using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Text;

namespace Trivial.Demo;

internal static class DataService
{
    private static readonly DataCacheCollection<JsonObjectNode> cardCache = new()
    {
        Expiration = TimeSpan.FromHours(6)
    };

    public static Task<JsonObjectNode> GetCardPageData(string channel, int page)
    {
        // https://mesh.if.iqiyi.com/portal/v5/channel/film?uid=0&vip=1&auth=&device=&scale=100&brightness=light&v=8.12.149.5800&mode=page&page=2&adUrl=
        var http = new JsonHttpClient<JsonObjectNode>();
        var url = $"https://mesh.if.iqiyi.com/portal/v5/channel/{channel}?uid=0&vip=1&auth=&device=hrjietpxi2reqqnt4mo73t4kevphmj2m&scale=200&brightness=dark&mode=page&page={page}&v=9.0.0.0";
        return http.GetAsync(url);
    }

    public static async Task<JsonObjectNode> GetCardPageData(string channel, int page, Action<JsonObjectNode> callback)
    {
        if (page == 1 && cardCache.TryGet(channel, out var result)) callback?.Invoke(result);
        result = await GetCardPageData(channel, page);
        cardCache[channel] = result;
        callback?.Invoke(result);
        return result;
    }
}
