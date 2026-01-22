using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class EvolutionMember
{
    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("level")]
    public int Level { get; set; }

    [BsonElement("is_baby")]
    public bool IsBaby { get; set; }
}
