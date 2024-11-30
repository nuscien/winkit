using System.Text.Json.Serialization;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Security;
using Trivial.Text;

namespace Trivial.Web;

public class DemoServer : TokenRequestRoute<UserModel>
{
    public static DemoServer Instance { get; } = new();

    private readonly List<UserModel> users = new();

    public UserModel Register(string name, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password)) return null;
        var user = users.FirstOrDefault(ele => ele.Name == name);
        if (user != null && user.Password == password) return user;
        var payload = new JsonWebTokenPayload
        {
            Id = Guid.NewGuid().ToString(),
            Subject = name,
        };
        user = new UserModel
        {
            Name = name,
            Password = password,
            Token = new JsonWebToken<JsonWebTokenPayload>(payload, GetSignatureProvider(payload, null)).ToEncodedString()
        };
        if (users.Any(ele => ele.Name == name)) return null;
        users.Add(user);
        return user;
    }

    public UserModel Get(string token)
        => string.IsNullOrWhiteSpace(token) ? null : users.FirstOrDefault(ele => ele.Token == token);

    public bool SignOut(string token)
        => !string.IsNullOrWhiteSpace(token) && users.RemoveAll(ele => ele.Token == token) > 0;

    protected override async Task<SelectionRelationship<UserModel, TokenInfo>> SignInAsync(TokenRequest<PasswordTokenRequestBody> req, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var user = users.FirstOrDefault(ele => ele.Name == req.Body.UserName && ele.Password == req.Body.Password.ToUnsecureString());
        if (user is null) return null;
        return new(user, new TokenInfo
        {
            TokenType = TokenInfo.BearerTokenType,
            AccessToken = user.Token,
            UserId = user.Id
        });
    }

    public async IAsyncEnumerable<ServerSentEventInfo> StreamData(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await Task.Delay(1000);
            yield return new ServerSentEventInfo("test-data", "message", new JsonObjectNode()
            {
                { "action", "add" },
                { "value", Guid.NewGuid() },
                { "index", i }
            });
        }
    }

    public ISignatureProvider GetSignatureProvider<T>(T payload, string id)
        => (id?.Trim().ToUpperInvariant() ?? string.Empty) switch
        {
            _ => HashSignatureProvider.CreateHS512("test")
        };
}

public class UserModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }
}
