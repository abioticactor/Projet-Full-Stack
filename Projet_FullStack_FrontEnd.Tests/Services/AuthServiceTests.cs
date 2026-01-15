namespace Projet_FullStack_FrontEnd.Tests.Services;

public class AuthServiceTests
{
    private static string CreateJwt(string sub, string email, string pseudo)
    {
        // Non-signed JWT for testing (no validation is performed by ReadJwtToken)
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"));
        var payloadObj = $"{{\"sub\":\"{sub}\",\"email\":\"{email}\",\"pseudo\":\"{pseudo}\"}}";
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadObj));
        return $"{header}.{payload}."; // no signature
    }

    [Fact]
    public async Task InitializeAsync_LoadsTokenFromLocalStorage_AndSetsHeaders()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var token = CreateJwt("u1", "ash@example.com", "Ash");
        ctx.JSInterop.Setup<string>("localStorage.getItem", "authToken").SetResult(token);

        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");

        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddScoped<AuthService>();

        var auth = ctx.Services.GetRequiredService<AuthService>();
        await auth.InitializeAsync();

        Assert.True(auth.IsAuthenticated);
        Assert.Equal("u1", auth.CurrentUserId);
        Assert.Equal("ash@example.com", auth.CurrentEmail);
        Assert.Equal("Ash", auth.CurrentPseudo);
        Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization?.Scheme);
        Assert.Equal(token, httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    [Fact]
    public async Task LoginAsync_OnSuccess_StoresTokenAndSetsHeader()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var token = CreateJwt("u2", "misty@example.com", "Misty");

        // Mock API response
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "http://localhost:5122/api/dresseurs/login")
                .Respond("application/json", $"{{ \"token\": \"{token}\" }}");
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");

        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddScoped<AuthService>();

        var auth = ctx.Services.GetRequiredService<AuthService>();
        var (ok, error) = await auth.LoginAsync("misty@example.com", "password");

        Assert.True(ok);
        Assert.Null(error);
        Assert.True(auth.IsAuthenticated);
        Assert.Equal("Misty", auth.CurrentPseudo);
        Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization?.Scheme);
        Assert.Equal(token, httpClient.DefaultRequestHeaders.Authorization?.Parameter);

        // Verify localStorage.setItem called
        var invocations = ctx.JSInterop.Invocations.Where(i => i.Identifier == "localStorage.setItem").ToList();
        Assert.Single(invocations);
        Assert.Equal("authToken", invocations[0].Arguments[0]?.ToString());
        Assert.Equal(token, invocations[0].Arguments[1]?.ToString());
    }

    [Fact]
    public async Task LogoutAsync_ClearsState_And_RemovesLocalStorage()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");

        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddScoped<AuthService>();

        var auth = ctx.Services.GetRequiredService<AuthService>();

        // Set a fake header/token by calling LoginAsync with mocked response
        var token = CreateJwt("u3", "brock@example.com", "Brock");
        mockHttp.When(HttpMethod.Post, "http://localhost:5122/api/dresseurs/login")
                .Respond("application/json", $"{{ \"token\": \"{token}\" }}");
        var (ok, _) = await auth.LoginAsync("brock@example.com", "pwd");
        Assert.True(ok);
        Assert.True(auth.IsAuthenticated);

        await auth.LogoutAsync();

        Assert.False(auth.IsAuthenticated);
        Assert.Null(httpClient.DefaultRequestHeaders.Authorization);

        // Verify localStorage.removeItem called
        var invocations = ctx.JSInterop.Invocations.Where(i => i.Identifier == "localStorage.removeItem").ToList();
        Assert.Single(invocations);
        Assert.Equal("authToken", invocations[0].Arguments[0]?.ToString());
    }
}
