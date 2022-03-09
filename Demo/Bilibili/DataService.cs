using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Demo.Bilibili;

/// <summary>
/// The data provider client service.
/// </summary>
public static class DataService
{
    /// <summary>
    /// The URL of homepage.
    /// </summary>
    public const string HomepageUrl = "https://www.bilibili.com";

    /// <summary>
    /// The URL of web API.
    /// </summary>
    public const string ApiUrl = "https://www.bilibili.com/index/ding.json";

    /// <summary>
    /// Gets the top videos cache of animation (douga).
    /// </summary>
    public readonly static List<ItemModel> Animation = new();

    /// <summary>
    /// Gets the top videos cache of series (teleplay).
    /// </summary>
    public readonly static List<ItemModel> Series = new();

    /// <summary>
    /// Gets the top videos cache of kichiku, a kind of original video with extraordinary form of content, きちく.
    /// </summary>
    public readonly static List<ItemModel> Kichiku = new();

    /// <summary>
    /// Gets the top videos cache of dance content.
    /// </summary>
    public readonly static List<ItemModel> Dance = new();

    /// <summary>
    /// Gets the top videos cache of bangumi series, アニメ番組.
    /// </summary>
    public readonly static List<ItemModel> BangumiSeries = new();

    /// <summary>
    /// Gets the top videos cache of fashion content.
    /// </summary>
    public readonly static List<ItemModel> Fashion = new();

    /// <summary>
    /// Gets the top videos cache of life content.
    /// </summary>
    public readonly static List<ItemModel> Life = new();

    /// <summary>
    /// Gets the top videos cache of original content made in China (guochuang).
    /// </summary>
    public readonly static List<ItemModel> ChinaOriginalContent = new();

    /// <summary>
    /// Gets the top videos cache of movie.
    /// </summary>
    public readonly static List<ItemModel> Movie = new();

    /// <summary>
    /// Gets the top videos cache of music.
    /// </summary>
    public readonly static List<ItemModel> Music = new();

    /// <summary>
    /// Gets the top videos cache of technology.
    /// </summary>
    public readonly static List<ItemModel> Technology = new();

    /// <summary>
    /// Gets the top videos cache of game.
    /// </summary>
    public readonly static List<ItemModel> Game = new();

    /// <summary>
    /// Gets the top videos cache of entertainment.
    /// </summary>
    public readonly static List<ItemModel> Entertainment = new();

    public static DateTime? LatestCache { get; private set; }

    public static async Task LoadDataAsync()
    {
        JsonObjectNode json = null;
        try
        {
            var http = new JsonHttpClient<JsonObjectNode>();
            var date = DateTime.Now;
            json = await http.GetAsync(ApiUrl);
            LatestCache = date;
        }
        catch (FailedHttpException)
        {
        }
        catch (JsonException)
        {
        }
        catch (HttpRequestException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }
        catch (ExternalException)
        {
        }

        LoadData(json, false);
    }

    public static void LoadData(JsonObjectNode json, bool updateCacheTime)
    {
        if (json == null) return;
        if (updateCacheTime) LatestCache = DateTime.Now;
        FillData(Animation, json, "douga");
        FillData(Series, json, "teleplay");
        FillData(Kichiku, json, "kichiku");
        FillData(Dance, json, "dance");
        FillData(BangumiSeries, json, "bangumi");
        FillData(Fashion, json, "fashion");
        FillData(Life, json, "life");
        FillData(ChinaOriginalContent, json, "guochuang");
        FillData(Movie, json, "movie");
        FillData(Music, json, "music");
        FillData(Technology, json, "technology");
        FillData(Game, json, "game");
        FillData(Entertainment, json, "ent");
    }

    private static List<JsonObjectNode> LoadData(JsonObjectNode json, string propertyKey)
    {
        json = json?.TryGetObjectValue(propertyKey);
        if (json == null)
        {
            var arr = json?.TryGetArrayValue(propertyKey);
            if (arr == null) return null;
            return arr.OfType<JsonObjectNode>()?.ToList();
        }

        var list = new List<JsonObjectNode>();
        foreach (var item in json)
        {
            if (string.IsNullOrWhiteSpace(item.Key) || !int.TryParse(item.Key, out _) || item.Value is not JsonObjectNode obj) continue;
            list.Add(obj);
        }

        return list;
    }

    private static void FillData(List<ItemModel> target, JsonObjectNode json, string propertyKey)
    {
        var list = LoadData(json, propertyKey);
        target.Clear();
        if (list == null) return;
        target.AddRange(list.Select(ele => (ItemModel)ele).Where(ele => ele != null));
    }
}
