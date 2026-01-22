namespace Projet_FullStack_FrontEnd.Tests.TestUtils;

public static class JwtTestHelper
{
    public static string CreateJwt(string sub, string email, string pseudo)
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"));
        var payloadObj = $"{{\"sub\":\"{sub}\",\"email\":\"{email}\",\"pseudo\":\"{pseudo}\"}}";
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadObj));
        return $"{header}.{payload}.";
    }
}
