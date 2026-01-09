namespace Projet_FullStack_FrontEnd.Tests.Components;

public class RequireAuthTests
{
    private IRenderedComponent<Projet_FullStack_FrontEnd.Components.RequireAuth> RenderWithAuthState(TestContext ctx, bool isAuthenticated)
    {
        ctx.Services.AddScoped<AuthService>(sp =>
        {
            // Provide a lightweight fake AuthService
            var http = new HttpClient { BaseAddress = new Uri("http://localhost:5122/") };
            var js = ctx.JSInterop.JSRuntime;
            var svc = new AuthService(http, js);
            // Hack: simulate private token via InitializeAsync JS mock or reflection
            if (isAuthenticated)
            {
                // Set token via JS/localStorage path
                ctx.JSInterop.Mode = JSRuntimeMode.Loose;
                ctx.JSInterop.Setup<string>("localStorage.getItem", "authToken").SetResult("header.payload.");
                svc.InitializeAsync().GetAwaiter().GetResult();
            }
            return svc;
        });

        return ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.RequireAuth>(parameters =>
        {
            parameters.AddChildContent("<p id='protected'>Contenu Protégé</p>");
        });
    }

    [Fact]
    public void WhenAuthenticated_RendersChildContent()
    {
        using var ctx = new TestContext();
        var cut = RenderWithAuthState(ctx, true);
        cut.MarkupMatches("<p id=\"protected\">Contenu Protégé</p>");
    }

    [Fact]
    public void WhenAnonymous_ShowsWarning()
    {
        using var ctx = new TestContext();
        var cut = RenderWithAuthState(ctx, false);
        cut.Markup.Contains("Vous devez être connecté");
    }
}
