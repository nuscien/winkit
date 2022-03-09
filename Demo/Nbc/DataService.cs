using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Net;
using Trivial.Text;

namespace Trivial.Demo.Nbc;

/// <summary>
/// 
/// </summary>
public static class DataService
{
    private const string ApiUrl = "https://friendship.nbc.co/v2/graphql?variables=%7B%22app%22:%22nbc%22,%22userId%22:%220%22,%22device%22:%22web%22,%22platform%22:%22web%22,%22language%22:%22en%22,%22oneApp%22:true,%22name%22:%22homepage%22,%22type%22:%22PAGE%22,%22timeZone%22:%22America%2FNew_York%22,%22authorized%22:false,%22ld%22:true,%22profile%22:[%2200000%22]%7D&extensions=%7B%22persistedQuery%22:%7B%22version%22:1,%22sha256Hash%22:%222bd26f1fee864ef6d7727c5e0a0654f665d8cf638b0023018032d0a945a67f2c%22%7D%7D";

    public static async Task<IEnumerable<JsonObjectNode>> GetRawAsync()
    {
        var http = new JsonHttpClient<JsonObjectNode>();
        var json = await http.GetAsync(ApiUrl);
        return json?.TryGetObjectValue("data", "bonanzaPage", "data")?.TryGetArrayValue("sections")?.OfType<JsonObjectNode>()?.Where(ele => ele?.TryGetStringValue("component")?.ToLowerInvariant() == "shelf")?.Select(ele => ele.TryGetObjectValue("data"))?.Where(ele => ele != null);
    }
}
