using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Region
{
    [BsonElement("name_fr")]
    public string NameFr { get; set; }
}
