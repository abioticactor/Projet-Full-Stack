namespace Projet_FullStack_FrontEnd.Tests.TestUtils;

public static class TestContextFactory
{
    public static TestContext Create(Action<IServiceCollection>? configureServices = null, Action<MockHttpMessageHandler>? configureHttp = null)
    {
        var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Mock HttpClient
        var mockHttp = new MockHttpMessageHandler();
        configureHttp?.Invoke(mockHttp);
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");
        ctx.Services.AddScoped(sp => httpClient);

        // Register app services
        ctx.Services.AddScoped<AuthService>();
        ctx.Services.AddScoped<UserStateService>();

        configureServices?.Invoke(ctx.Services);
        return ctx;
    }

    public static async Task<TestContext> CreateAuthenticatedAsync(string sub, string email, string pseudo, Action<MockHttpMessageHandler>? configureHttp = null, Action<IServiceCollection>? configureServices = null)
    {
        var ctx = Create(configureServices, configureHttp);
        var token = JwtTestHelper.CreateJwt(sub, email, pseudo);
        ctx.JSInterop.Setup<string>("localStorage.getItem", "authToken").SetResult(token);
        var auth = ctx.Services.GetRequiredService<AuthService>();
        await auth.InitializeAsync();
        return ctx;
    }

    public static void AddDefaultApiMocks(MockHttpMessageHandler mockHttp)
    {
        // Profil par défaut
        mockHttp.When("http://localhost:5122/api/dresseurs/profil").Respond("application/json",
            "{ \"id\": \"u1\", \"pseudo\": \"Red\", \"email\": \"red@example.com\", \"amis\": [], \"victoires\": 0, \"defaites\": 0, \"partiesJouees\": 0 }");
        // Pokedex minimal
        mockHttp.When("http://localhost:5122/api/pokemon*").Respond("application/json",
            "[{ \"id\": \"p1\", \"pokedexNumber\": 1, \"nameFr\": \"Bulbizarre\", \"types\": [\"Plante\", \"Poison\"] }]");
    }
}
