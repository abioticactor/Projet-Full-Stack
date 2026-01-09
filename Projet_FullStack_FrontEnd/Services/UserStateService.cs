using System.Net.Http.Json;

namespace Projet_FullStack_FrontEnd.Services;

/// <summary>
/// Service pour gérer l'état et les données de l'utilisateur connecté
/// Utilise un pattern Singleton pour partager les données entre tous les composants
/// </summary>
public class UserStateService
{
    private readonly HttpClient _http;
    private readonly AuthService _authService;
    
    // Données utilisateur en cache
    private DresseurData? _currentUser;
    private bool _isLoading = false;

    // Événement pour notifier les composants des changements
    public event Action? OnChange;

    public UserStateService(HttpClient http, AuthService authService)
    {
        _http = http;
        _authService = authService;
    }

    public DresseurData? CurrentUser => _currentUser;
    public bool IsLoading => _isLoading;

    /// <summary>
    /// Charge les données complètes du dresseur depuis l'API
    /// </summary>
    public async Task LoadUserDataAsync()
    {
        if (!_authService.IsAuthenticated)
        {
            _currentUser = null;
            NotifyStateChanged();
            return;
        }

        _isLoading = true;
        NotifyStateChanged();

        try
        {
            // Appel à l'API pour récupérer le profil complet
            var response = await _http.GetAsync("api/dresseurs/profil");
            
            if (response.IsSuccessStatusCode)
            {
                var profil = await response.Content.ReadFromJsonAsync<ProfilResponse>();
                
                if (profil != null)
                {
                    _currentUser = new DresseurData
                    {
                        Id = profil.id ?? _authService.CurrentUserId ?? "",
                        Pseudo = profil.pseudo ?? _authService.CurrentPseudo ?? "",
                        Email = profil.email ?? _authService.CurrentEmail ?? "", // Use profil.email first
                        Amis = profil.amis ?? new List<string>(),
                        Victoires = profil.victoires ?? 0,
                        Defaites = profil.defaites ?? 0,
                        PartiesJouees = profil.partiesJouees ?? 0
                    };
                }
            }
            else
            {
                // Si l'API échoue, utiliser les données du token
                _currentUser = new DresseurData
                {
                    Id = _authService.CurrentUserId ?? "",
                    Pseudo = _authService.CurrentPseudo ?? "",
                    Email = _authService.CurrentEmail ?? "",
                    Amis = new List<string>(),
                    Victoires = 0,
                    Defaites = 0,
                    PartiesJouees = 0
                };
            }
        }
        catch
        {
            // En cas d'erreur, utiliser les données minimales du token
            _currentUser = new DresseurData
            {
                Id = _authService.CurrentUserId ?? "",
                Pseudo = _authService.CurrentPseudo ?? "",
                Email = _authService.CurrentEmail ?? "",
                Amis = new List<string>(),
                Victoires = 0,
                Defaites = 0,
                PartiesJouees = 0
            };
        }
        finally
        {
            _isLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Recharge les données utilisateur (utile après une mise à jour)
    /// </summary>
    public async Task RefreshUserDataAsync()
    {
        await LoadUserDataAsync();
    }

    /// <summary>
    /// Efface les données utilisateur (à la déconnexion)
    /// </summary>
    public void ClearUserData()
    {
        _currentUser = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        Console.WriteLine("NotifyStateChanged invoked"); // Debug log
        OnChange?.Invoke();
    }

    // Classes pour les réponses API
    private class ProfilResponse
    {
        public string? id { get; set; }
        public string? pseudo { get; set; }
        public List<string>? amis { get; set; }
        public int? victoires { get; set; }
        public int? defaites { get; set; }
        public int? partiesJouees { get; set; }
        public string? email { get; set; } // Ajout de la propriété email
    }
}

/// <summary>
/// Modèle de données pour un dresseur
/// </summary>
public class DresseurData
{
    public string Id { get; set; } = "";
    public string Pseudo { get; set; } = "";
    public string Email { get; set; } = "";
    public List<string> Amis { get; set; } = new();
    public int Victoires { get; set; }
    public int Defaites { get; set; }
    public int PartiesJouees { get; set; }
}
