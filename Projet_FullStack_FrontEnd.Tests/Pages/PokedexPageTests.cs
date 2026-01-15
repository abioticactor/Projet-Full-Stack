namespace Projet_FullStack_FrontEnd.Tests.Pages;

public class PokedexPageTests
{
    private void SetupAuth(TestContext ctx)
    {
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var token = JwtTestHelper.CreateJwt("u1", "user@example.com", "Ash");
        ctx.JSInterop.Setup<string>("localStorage.getItem", "authToken").SetResult(token);
    }

    [Fact]
    public void Pokedex_LoadingAndErrorStates()
    {
        using var ctx = new TestContext();
        SetupAuth(ctx);

        var mockHttp = new MockHttpMessageHandler();
        // Pokedex endpoints error
        mockHttp.When("http://localhost:5122/api/dresseurs/pokedex").Respond(System.Net.HttpStatusCode.InternalServerError);
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");

        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddScoped<AuthService>();
        ctx.Services.AddLogging();

        // Ensure AuthService is initialized so RequireAuth sees authenticated state
        var auth = ctx.Services.GetRequiredService<AuthService>();
        auth.InitializeAsync().GetAwaiter().GetResult();

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.Pokedex>();
        // Initially loading
        Assert.Contains("Chargement de votre Pokédex", cut.Markup);

        // After request fails, expect error
        cut.WaitForAssertion(() => Assert.Contains("Erreur", cut.Markup));
    }

    [Fact]
    public void Pokedex_RendersGrid_WithFilters_WhenDataPresent()
    {
        using var ctx = new TestContext();
        SetupAuth(ctx);

        var mockHttp = new MockHttpMessageHandler();
        // Pokedex returns empty captures
        mockHttp.When("http://localhost:5122/api/dresseurs/pokedex").Respond("application/json", "[]");
        // Pokemon paginated 1 page with 2 items
        mockHttp.When("http://localhost:5122/api/pokemon?page=1&pageSize=100").Respond("application/json",
            "{ \"items\": [ { \"numericId\": 1, \"nameFr\": \"Bulbizarre\", \"sprites\": { \"frontDefault\": \"/img/bulb.png\" }, \"types\": [ { \"name\": \"Grass\" } ] }, { \"numericId\": 4, \"nameFr\": \"Salamèche\", \"sprites\": { \"frontDefault\": \"/img/sala.png\" }, \"types\": [ { \"name\": \"Fire\" } ] } ], \"totalPages\": 1, \"page\": 1, \"pageSize\": 100, \"totalCount\": 2 }");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");

        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddScoped<AuthService>();
        ctx.Services.AddLogging();

        // Initialize auth
        var auth = ctx.Services.GetRequiredService<AuthService>();
        auth.InitializeAsync().GetAwaiter().GetResult();

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.Pokedex>();

        cut.WaitForAssertion(() =>
        {
            var html = cut.Markup;
            // Debug log to inspect rendered output
            Console.WriteLine("Rendered HTML: " + html);

            Assert.Contains("Mon Pokédex", html);
            Assert.Contains("filters-container", html);
            Assert.Contains("pokedex-grid", html);
            Assert.Contains("Bulbizarre", html);
            Assert.Contains("Salamèche", html);
        }, timeout: TimeSpan.FromSeconds(5));
    }
}
