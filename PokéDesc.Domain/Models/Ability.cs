using MongoDB.Bson.Serialization.Attributes;

namespace PokéDesc.Domain.Models;

public class Ability
{
    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("name_en")]
    public string NameEn { get; set; }

    [BsonElement("is_hidden")]
    public bool IsHidden { get; set; }

    [BsonElement("slot")]
    public int Slot { get; set; }
}
