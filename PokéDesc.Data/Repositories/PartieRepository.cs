// Dans PokéDesc.Data/Repositories/PartieRepository.cs
using MongoDB.Driver;
using PokéDesc.Domain.Models;

namespace PokéDesc.Data.Repositories;

public class PartieRepository
{
    private readonly IMongoCollection<Partie> _partiesCollection;

    public PartieRepository(IMongoDatabase database)
    {
        // Tu peux choisir le nom de collection que tu veux
        _partiesCollection = database.GetCollection<Partie>("Parties_Collection");
    }

    // Créer une nouvelle partie
    public async Task CreateAsync(Partie partie)
    {
        await _partiesCollection.InsertOneAsync(partie);
    }

    // Trouver une partie avec son code
    public async Task<Partie> GetByCodeAsync(string code)
    {
        // On cherche une partie qui a ce code ET qui est en attente
        return await _partiesCollection.Find(p => p.CodeSession == code && p.Statut == "EnAttente").FirstOrDefaultAsync();
    }

    // Trouver une partie par son ID
    public async Task<Partie> GetByIdAsync(string id)
    {
        return await _partiesCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    // Mettre à jour une partie (ex: pour ajouter le joueur 2)
    public async Task UpdateAsync(Partie partie)
    {
        await _partiesCollection.ReplaceOneAsync(p => p.Id == partie.Id, partie);
    }
}