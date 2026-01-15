namespace Projet_FullStack_FrontEnd.Tests.Pages;

public class PokemonDetailsPageTests
{
    [Fact]
    public void PokemonDetails_ShowsLoading_ThenError_OnFailure()
    {
        using var ctx = new TestContext();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost:5122/api/pokemon/pokedex/25").Respond(System.Net.HttpStatusCode.NotFound);
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");
        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddLogging();

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.PokemonDetails>(ps => ps.Add(p => p.number, 25));

        Assert.Contains("Chargement...", cut.Markup);
        cut.WaitForAssertion(() => Assert.Contains("Pokémon introuvable", cut.Markup));
    }

    [Fact]
    public void PokemonDetails_RendersDetails_OnSuccess()
    {
        using var ctx = new TestContext();

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost:5122/api/pokemon/pokedex/6").Respond("application/json",
            "{ \"numericId\": 6, \"nameFr\": \"Dracaufeu\", \"sprites\": { \"frontDefault\": \"/img/charizard.png\" }, \"types\": [ { \"name\": \"Fire\" }, { \"name\": \"Flying\" } ], \"stats\": { \"attack\": { \"value\": 84, \"nameEn\": \"Attack\" }, \"hp\": { \"value\": 78, \"nameEn\": \"HP\" } }, \"description\": \"Un Pokémon dragon flamboyant\" }");

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");
        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddLogging();

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.PokemonDetails>(ps => ps.Add(p => p.number, 6));

        cut.WaitForAssertion(() =>
        {
            var html = cut.Markup;
            Assert.Contains("Dracaufeu", html);
            Assert.Contains("Attack", html);
            Assert.Contains("HP", html);
            Assert.Contains("Un Pokémon dragon flamboyant", html);
            Assert.Contains("/img/charizard.png", html);
        });
    }
}
