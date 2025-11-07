using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Physical
{
    [BsonElement("height_m")]
    public double HeightM { get; set; }

    [BsonElement("weight_kg")]
    public double WeightKg { get; set; }
}
