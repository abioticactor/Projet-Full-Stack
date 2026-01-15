namespace Projet_FullStack_FrontEnd.Tests.Pages;

public class HomePageTests
{
    [Fact]
    public void Home_UsesPseudoInTrainerCardUrl_And_RendersButtons()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        ctx.Services.AddScoped(sp =>
        {
            var http = new HttpClient { BaseAddress = new Uri("http://localhost:5122/") };
            var js = ctx.JSInterop.JSRuntime;
            var auth = new AuthService(http, js);
            var token = JwtTestHelper.CreateJwt("u42", "user@example.com", "Red");
            ctx.JSInterop.Setup<string>("localStorage.getItem", "authToken").SetResult(token);
            auth.InitializeAsync().GetAwaiter().GetResult();

            // Debug log to verify CurrentPseudo
            Console.WriteLine("AuthService.CurrentPseudo: " + auth.CurrentPseudo);

            return auth;
        });

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.Home>();

        // Vérifie la présence des boutons principaux
        var html = cut.Markup;
        Assert.Contains("aria-label=\"Carte de Dresseur\"", html);
        Assert.Contains("aria-label=\"Partie\"", html);
        Assert.Contains("aria-label=\"Pokédex\"", html);
        Assert.Contains("aria-label=\"Succès\"", html);

        // Vérifie l'URL incluant le pseudo
        Assert.Contains("/carte-dresseur/Red", html);
    }
}
