using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Sprites
{
    [BsonElement("front_default")]
    public string FrontDefault { get; set; }

    [BsonElement("front_shiny")]
    public string FrontShiny { get; set; }

    [BsonElement("back_default")]
    public string BackDefault { get; set; }

    [BsonElement("back_shiny")]
    public string BackShiny { get; set; }
}
