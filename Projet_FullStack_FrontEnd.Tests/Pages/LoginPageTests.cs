namespace Projet_FullStack_FrontEnd.Tests.Pages;

public class LoginPageTests
{
    [Fact]
    public void LoginPage_RendersWelcomeMessage()
    {
        using var ctx = new TestContext();
        var cut = ctx.RenderComponent<Projet_FullStack_FrontEnd.Components.Pages.Login>();
        cut.Markup.Contains("Bienvenue dans Pokémon Strikes");
    }
}
