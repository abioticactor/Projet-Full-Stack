namespace Projet_FullStack_FrontEnd.Tests.Services;

public class UserStateServiceTests
{
    [Fact]
    public async Task LoadUserData_Anonymous_SetsNull_And_Notifies()
    {
        using var ctx = TestContextFactory.Create();
        var userState = ctx.Services.GetRequiredService<UserStateService>();
        int changeCount = 0;
        userState.OnChange += () => changeCount++;

        await userState.LoadUserDataAsync();

        Assert.Null(userState.CurrentUser);
        Assert.False(userState.IsLoading);
        Assert.True(changeCount >= 2);
    }

    [Fact]
    public async Task LoadUserData_UsesProfileFromApi_WhenAvailable()
    {
        using var ctx = await TestContextFactory.CreateAuthenticatedAsync("u99", "red@example.com", "Red", mock =>
        {
            mock.When("http://localhost:5122/api/dresseurs/profil").Respond("application/json",
                "{ \"id\": \"u100\", \"pseudo\": \"Blue\", \"email\": \"red@example.com\", \"amis\": [\"g1\"], \"victoires\": 3, \"defaites\": 1, \"partiesJouees\": 4 }");
        });

        var userState = ctx.Services.GetRequiredService<UserStateService>();
        await userState.LoadUserDataAsync();

        Assert.NotNull(userState.CurrentUser);
        Assert.Equal("u100", userState.CurrentUser!.Id);
        Assert.Equal("Blue", userState.CurrentUser!.Pseudo);
        Assert.Equal("red@example.com", userState.CurrentUser!.Email);
        Assert.Equal(3, userState.CurrentUser!.Victoires);
        Assert.Equal(1, userState.CurrentUser!.Defaites);
        Assert.Equal(4, userState.CurrentUser!.PartiesJouees);
        Assert.False(userState.IsLoading);
    }

    [Fact]
    public async Task ClearUserData_Clears_And_Notifies()
    {
        using var ctx = await TestContextFactory.CreateAuthenticatedAsync("u42", "user@example.com", "Ash", mock =>
        {
            mock.When("http://localhost:5122/api/dresseurs/profil").Respond("application/json", "{}");
        });

        var userState = ctx.Services.GetRequiredService<UserStateService>();
        await userState.LoadUserDataAsync();
        Assert.NotNull(userState.CurrentUser);

        int notifications = 0;
        userState.OnChange += () => notifications++;

        userState.ClearUserData();

        Assert.Null(userState.CurrentUser);
        Assert.True(notifications >= 1);
    }
}
