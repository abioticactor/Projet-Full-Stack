namespace Projet_FullStack_FrontEnd.Tests.Pages;

public class PartiePageTests
{
    private void SetupAuth(TestContext ctx, string userId = "u1", string pseudo = "Ash")
    {
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var token = JwtTestHelper.CreateJwt(userId, $"{pseudo.ToLower()}@example.com", pseudo);
        ctx.JSInterop.Setup<string>("localStorage.getItem", "authToken").SetResult(token);
    }

    [Fact]
    public void WhenLoading_ShowsSpinner()
    {
        using var ctx = new TestContext();
        SetupAuth(ctx);

        // Mock HTTP + services
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");
        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddLogging();
        ctx.Services.AddScoped<AuthService>();
        ctx.Services.AddScoped<UserStateService>();

        // Initialize auth so RequireAuth passes
        var auth = ctx.Services.GetRequiredService<AuthService>();
        auth.InitializeAsync().GetAwaiter().GetResult();

        // Render component
        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.Partie>();

        // Au premier rendu, UserState.IsLoading est true jusqu'à la fin d'OnInitializedAsync
        Assert.Contains("Chargement…", cut.Markup);
    }

    [Fact]
    public void WhenError_ShowsErrorMessage()
    {
        using var ctx = new TestContext();
        SetupAuth(ctx);

        var mockHttp = new MockHttpMessageHandler();
        // user profil endpoint returns error to force fallback and finish loading
        mockHttp.When("http://localhost:5122/api/dresseurs/profil").Respond(System.Net.HttpStatusCode.InternalServerError);
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");

        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddLogging();
        ctx.Services.AddScoped<AuthService>();
        ctx.Services.AddScoped<UserStateService>();

        // Initialize auth so RequireAuth passes
        var auth = ctx.Services.GetRequiredService<AuthService>();
        auth.InitializeAsync().GetAwaiter().GetResult();

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.Partie>();

        // Simuler un clic sur "Créer une nouvelle partie" pour provoquer une erreur API
        var button = cut.Find("button.btn-primary");
        // Configure create endpoint to fail
        mockHttp.When(HttpMethod.Post, "http://localhost:5122/api/partie/create")
                .Respond(System.Net.HttpStatusCode.InternalServerError);

        button.Click();

        // Attendre que l'état se mette à jour
        cut.WaitForAssertion(() => Assert.Contains("Erreur", cut.Markup), timeout: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void JoinGame_EmptyCode_ShowsValidationMessage()
    {
        using var ctx = new TestContext();
        SetupAuth(ctx);

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost:5122/api/dresseurs/profil").Respond(System.Net.HttpStatusCode.OK, "application/json", "{}");
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5122/");

        ctx.Services.AddScoped(sp => httpClient);
        ctx.Services.AddLogging();
        ctx.Services.AddScoped<AuthService>();
        ctx.Services.AddScoped<UserStateService>();

        // Initialize auth so RequireAuth passes
        var auth = ctx.Services.GetRequiredService<AuthService>();
        auth.InitializeAsync().GetAwaiter().GetResult();

        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.Partie>();

        // Click on second .btn-primary (Join)
        var joinButton = cut.FindAll("button.btn-primary")[1];
        joinButton.Click();

        cut.WaitForAssertion(() => Assert.Contains("Veuillez entrer un code de session", cut.Markup));
    }
}
