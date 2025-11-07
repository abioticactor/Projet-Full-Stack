using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Pokemon
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("id")]
    public int NumericId { get; set; }

    [BsonElement("name_fr")]
    public string NameFr { get; set; }

    [BsonElement("name_en")]
    public string NameEn { get; set; }

    [BsonElement("category")]
    public string Category { get; set; }

    [BsonElement("pokedex_number")]
    public int PokedexNumber { get; set; }

    [BsonElement("generation")]
    public Generation Generation { get; set; }

    [BsonElement("region")]
    public Region Region { get; set; }

    [BsonElement("status")]
    public Status Status { get; set; }

    [BsonElement("breeding")]
    public Breeding Breeding { get; set; }

    [BsonElement("physical")]
    public Physical Physical { get; set; }

    [BsonElement("types")]
    public List<PokemonType> Types { get; set; }

    [BsonElement("abilities")]
    public List<Ability> Abilities { get; set; }

    [BsonElement("forms_count")]
    public int FormsCount { get; set; }

    [BsonElement("stats")]
    public Stats Stats { get; set; }

    [BsonElement("sprites")]
    public Sprites Sprites { get; set; }

    [BsonElement("cries")]
    public Cries Cries { get; set; }

    [BsonElement("moves")]
    public List<string> Moves { get; set; }

    [BsonElement("moves_count")]
    public int MovesCount { get; set; }

    [BsonElement("description")]
    public string Description { get; set; }

    [BsonElement("evolution_chain")]
    public EvolutionChain EvolutionChain { get; set; }
}
