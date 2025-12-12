using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.JSInterop;

namespace Projet_FullStack_FrontEnd.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _jsRuntime;
    private string? _token;
    private string? _currentUserId;
    private string? _currentPseudo;
    private string? _currentEmail;

    public AuthService(HttpClient http, IJSRuntime jsRuntime)
    {
        _http = http;
        _jsRuntime = jsRuntime;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
    public string? Token => _token;
    public string? CurrentUserId => _currentUserId;
    public string? CurrentPseudo => _currentPseudo;
    public string? CurrentEmail => _currentEmail;

    /// <summary>
    /// Initialise l'AuthService en chargeant le token depuis localStorage
    /// Appeler cette méthode au démarrage de l'application
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(_token))
            {
                ExtractUserInfoFromToken(_token);
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
        }
        catch
        {
            // Si localStorage n'est pas disponible (SSR), ignorer
            _token = null;
        }
    }

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
            
            // Extraire les informations du JWT
            ExtractUserInfoFromToken(_token);
            
            // Sauvegarder le token dans localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", _token);
            
            // Configurer l'en-tête Authorization pour les requêtes futures
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        _token = null;
        _currentUserId = null;
        _currentPseudo = null;
        _currentEmail = null;
        _http.DefaultRequestHeaders.Authorization = null;
        
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }
        catch
        {
            // Ignorer si localStorage n'est pas disponible
        }
    }

    /// <summary>
    /// Extrait les informations utilisateur depuis le JWT token
    /// </summary>
    private void ExtractUserInfoFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            _currentUserId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            _currentEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            _currentPseudo = jwtToken.Claims.FirstOrDefault(c => c.Type == "pseudo")?.Value;
        }
        catch
        {
            // Si le token est invalide, réinitialiser
            _currentUserId = null;
            _currentEmail = null;
            _currentPseudo = null;
        }
    }

    private class LoginResponse { public string? token { get; set; } }
    private class ErrorResponse { public string? message { get; set; } }
}
