// Dans PokéDesc.Business/Services/PartieService.cs
using PokéDesc.Data.Repositories;
using PokéDesc.Domain;
using PokéDesc.Domain.Models;

namespace PokéDesc.Business.Services;

public class PartieService
{
    private readonly PartieRepository _partieRepository;

    public PartieService(PartieRepository partieRepository)
    {
        _partieRepository = partieRepository;
    }

    // Logique pour créer une nouvelle session
    public async Task<Partie> CreerSessionAsync(string dresseur1Id)
    {
        var nouvellePartie = new Partie
        {
            Dresseur1Id = dresseur1Id,
            Statut = "EnAttente",
            CodeSession = GenererCodeUnique() // On doit créer cette petite fonction
        };

        await _partieRepository.CreateAsync(nouvellePartie);
        return nouvellePartie;
    }

    // Logique pour rejoindre une session
    public async Task<Partie> RejoindreSessionAsync(string dresseur2Id, string code)
    {
        var partie = await _partieRepository.GetByCodeAsync(code);

        // Vérifications métier
        if (partie == null)
            throw new Exception("Partie introuvable ou déjà commencée.");
        if (partie.Dresseur1Id == dresseur2Id)
            throw new Exception("Vous ne pouvez pas rejoindre votre propre partie.");

        // Tout est bon, on ajoute le joueur 2 et on démarre
        partie.Dresseur2Id = dresseur2Id;
        partie.Statut = "EnCours"; // La partie peut commencer !

        await _partieRepository.UpdateAsync(partie);
        return partie;
    }

    // Petite fonction privée pour générer un code
    private string GenererCodeUnique()
    {
        // Génère un code simple de 6 caractères (ex: "A8T2B1")
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}