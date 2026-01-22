using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Breeding
{
    [BsonElement("egg_groups")]
    public List<string> EggGroups { get; set; }
}
