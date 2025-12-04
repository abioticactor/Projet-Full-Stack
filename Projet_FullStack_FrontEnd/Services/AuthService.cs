using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Projet_FullStack_FrontEnd.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private string? _token;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public string? Token => _token;

    public async Task<(bool ok, string? error)> RegisterAsync(string pseudo, string email, string password)
    {
        var req = new { Pseudo = pseudo, Email = email, Password = password };
        try
        {
            var resp = await _http.PostAsJsonAsync("api/dresseurs/register", req);
            if (resp.IsSuccessStatusCode)
            {
                return (true, null);
            }
            var err = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, err?.message ?? "Erreur lors de l'inscription");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool ok, string? error)> LoginAsync(string email, string password)
    {
        var req = new { Email = email, Password = password };
        try
        {
            var resp = await _http.PostAsJsonAsync("api/dresseurs/login", req);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
                return (false, err?.message ?? "Erreur lors de la connexion");
            }
            var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            if (body?.token == null)
            {
                return (false, "Token manquant dans la réponse");
            }
            _token = body.token;
            // Set the Authorization header for subsequent requests
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public void Logout()
    {
        _token = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    private class LoginResponse { public string? token { get; set; } }
    private class ErrorResponse { public string? message { get; set; } }
}
