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
}