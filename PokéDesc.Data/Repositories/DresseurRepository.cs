// Dans PokéDesc.Data/Repositories/DresseurRepository.cs
using MongoDB.Driver;
using PokéDesc.Domain;

namespace PokéDesc.Data.Repositories;
public class DresseurRepository
{
    private readonly IMongoCollection<Dresseur> _dresseursCollection;

    // On injecte la base de données pour pouvoir l'utiliser
    public DresseurRepository(IMongoDatabase database)
    {
    _dresseursCollection = database.GetCollection<Dresseur>("Dresseur_Collection");
    }

    // Méthode pour trouver un dresseur par son email
    public async Task<Dresseur> GetByEmailAsync(string email)
    {
        return await _dresseursCollection.Find(d => d.Email == email).FirstOrDefaultAsync();
    }

    public async Task<Dresseur> GetByPseudoAsync(string pseudo)
    {
        return await _dresseursCollection.Find(d => d.Pseudo == pseudo).FirstOrDefaultAsync();
    }

    // Méthode pour créer un nouveau dresseur
    public async Task CreateAsync(Dresseur dresseur)
    {
        await _dresseursCollection.InsertOneAsync(dresseur);
    }

    public async Task UpdateAsync(Dresseur dresseur)
    {
        await _dresseursCollection.ReplaceOneAsync(d => d.Id == dresseur.Id, dresseur);
    }

    // On aura aussi besoin de trouver un dresseur par son ID
    public async Task<Dresseur> GetByIdAsync(string id)
    {
        return await _dresseursCollection.Find(d => d.Id == id).FirstOrDefaultAsync();
    }

    // Méthode pour ajouter ou mettre à jour un Pokémon dans le Pokédex du dresseur
    public async Task UpdatePokedexAsync(string dresseurId, PokemonCapture pokemonCapture)
    {
        var filter = Builders<Dresseur>.Filter.Eq(d => d.Id, dresseurId);
        var update = Builders<Dresseur>.Update.Push(d => d.Pokedex, pokemonCapture);
        await _dresseursCollection.UpdateOneAsync(filter, update);
    }

    // Méthode pour mettre à jour le niveau d'un Pokémon existant dans le Pokédex
    public async Task UpdatePokemonLevelAsync(string dresseurId, int pokemonId, int newLevel)
    {
        var filter = Builders<Dresseur>.Filter.And(
            Builders<Dresseur>.Filter.Eq(d => d.Id, dresseurId),
            Builders<Dresseur>.Filter.ElemMatch(d => d.Pokedex, p => p.PokemonId == pokemonId)
        );
        var update = Builders<Dresseur>.Update.Set("Pokedex.$.Niveau", newLevel);
        await _dresseursCollection.UpdateOneAsync(filter, update);
    }
}