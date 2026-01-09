namespace Projet_FullStack_FrontEnd.Tests.AppAndRoutes;

public class AppAndRoutesTests
{
    [Fact]
    public void App_ShowsLoading_ThenRoutes()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var token = JwtTestHelper.CreateJwt("u1", "user@example.com", "Ash");
        ctx.JSInterop.Setup<string>("localStorage.getItem", "authToken").SetResult(token);

        ctx.Services.AddLogging();
        var http = new HttpClient { BaseAddress = new Uri("http://localhost:5122/") };
        ctx.Services.AddScoped(sp => http);
        ctx.Services.AddScoped<AuthService>();

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.App>();

        // Show loading initially
        Assert.Contains("Chargement...", cut.Markup);

        // After initialization, Routes component should render
        cut.WaitForAssertion(() => Assert.DoesNotContain("Chargement...", cut.Markup));
    }

    [Fact]
    public void Routes_NotFound_RendersMessage()
    {
        using var ctx = new TestContext();
        ctx.Services.AddLogging(builder => builder.AddFilter("*", LogLevel.Warning));

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Routes>();

        // Create a fake nav manager to navigate
        var nav = ctx.Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("/unknown-page");

        cut.WaitForAssertion(() => Assert.Contains("cette page n'existe pas", cut.Markup.ToLowerInvariant()));
    }
}
