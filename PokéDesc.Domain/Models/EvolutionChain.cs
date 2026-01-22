using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class EvolutionChain
{
    [BsonElement("count")]
    public int Count { get; set; }

    [BsonElement("base_pokemon")]
    public string BasePokemon { get; set; }

    [BsonElement("chain")]
    public List<EvolutionMember> Chain { get; set; }
}
