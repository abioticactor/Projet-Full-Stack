using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Cries
{
    [BsonElement("latest")]
    public string Latest { get; set; }

    [BsonElement("legacy")]
    public string Legacy { get; set; }
}
