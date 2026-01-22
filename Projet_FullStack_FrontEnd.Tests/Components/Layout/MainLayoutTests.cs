namespace Projet_FullStack_FrontEnd.Tests.Components.Layout;

public class MainLayoutTests
{
    [Fact]
    public void RendersNavLinks_And_UsesPseudoInCarteDresseurUrl()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Provide AuthService with a pseudo
        ctx.Services.AddScoped(sp =>
        {
            var http = new HttpClient { BaseAddress = new Uri("http://localhost:5122/") };
            var js = ctx.JSInterop.JSRuntime;
            var auth = new AuthService(http, js);
            // Simulate token having pseudo "Pikachu"
            var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\"}"));
            var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"pseudo\":\"Pikachu\"}"));
            var token = $"{header}.{payload}.";
            ctx.JSInterop.Setup<string>("localStorage.getItem", "authToken").SetResult(token);
            auth.InitializeAsync().GetAwaiter().GetResult();
            return auth;
        });

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Layout.MainLayout>();

        // Check menu anchor exists
        cut.Markup.Contains("Menu Principal");
        // Ensure Carte Dresseur link contains pseudo
        Assert.Contains("/carte-dresseur/Pikachu", cut.Markup);
    }
}
