using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Net;
using Trivial.Security;
using Trivial.Text;
using Trivial.Web;

namespace Trivial.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpPost("Register")]
    public async Task<UserModel> Register()
    {
        var json = await Request.ReadBodyAsJsonAsync();
        var user = DemoServer.Instance.Register(json.TryGetStringTrimmedValue("name"), json.TryGetStringTrimmedValue("password"));
        return user;
    }

    [HttpPost("Login")]
    public async Task<TokenInfo> Login()
    {
        var resp = await DemoServer.Instance.SignInAsync(Request.Body);
        return resp.ItemSelected;
    }

    [HttpGet("Data")]
    public IActionResult GetData()
    {
        var jwt = Request.GetJsonWebToken<JsonWebTokenPayload>(DemoServer.Instance.GetSignatureProvider<JsonWebTokenPayload>);
        var json = new JsonObjectNode
        {
            { "str", "Test data" },
            { "user", jwt?.Payload?.Subject },
            { "session", jwt?.Payload?.Id },
        };
        return json.ToActionResult();
    }

    [HttpGet("Stream")]
    public IActionResult Stream()
    {
        var sse = DemoServer.Instance.StreamDataAsync(10);
        return sse.ToActionResult();
    }

    [HttpGet("Jsonl")]
    public IActionResult ListData()
    {
        var sse = DemoServer.Instance.ListDataAsync(10);
        return sse.ToActionResult();
    }

    [HttpGet("Items")]
    public IActionResult PushData()
        => ControllerExtensions.ToActionResult<JsonObjectNode>(DemoServer.Instance.AppendToAsync);
}
