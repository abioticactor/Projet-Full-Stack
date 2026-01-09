using MongoDB.Driver;
using PokéDesc.Domain.Models;

namespace PokéDesc.Data.Repositories;

public class PokemonRepository
{
    private readonly IMongoCollection<Pokemon> _collection;

    public PokemonRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Pokemon>("Pokemon_Collection");
    }

    public async Task<List<Pokemon>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<(List<Pokemon> items, int totalCount)> GetPaginatedAsync(int page, int pageSize)
    {
        var totalCount = await _collection.CountDocumentsAsync(_ => true);
        var items = await _collection.Find(_ => true)
            .SortBy(p => p.PokedexNumber)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        return (items, (int)totalCount);
    }

    public async Task<Pokemon> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Pokemon> GetByPokedexNumberAsync(int pokedexNumber)
    {
        return await _collection.Find(p => p.PokedexNumber == pokedexNumber).FirstOrDefaultAsync();
    }

    public async Task<List<Pokemon>> GetByTypeAsync(string typeName)
    {
        return await _collection.Find(p => p.Types.Any(t => t.Name == typeName || t.NameEn == typeName)).ToListAsync();
    }

    public async Task<List<Pokemon>> GetByGenerationAsync(string generationName)
    {
        return await _collection.Find(p => p.Generation.NameFr == generationName || p.Generation.NameEn == generationName).ToListAsync();
    }

    public async Task<Pokemon> CreateAsync(Pokemon pokemon)
    {
        await _collection.InsertOneAsync(pokemon);
        return pokemon;
    }

    public async Task<bool> UpdateAsync(string id, Pokemon pokemon)
    {
        var result = await _collection.ReplaceOneAsync(p => p.Id == id, pokemon);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(p => p.Id == id);
        return result.DeletedCount > 0;
    }
}
